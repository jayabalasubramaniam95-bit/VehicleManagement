using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleManagement.Models;
using VehicleManagement.Repositories;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public class VehicleService : IVehicleService
{
    #region Constants

    private const int DefaultPageSize = 10;
    private const string Unknown = "Unknown";

    private const string VehicleNotFoundError = "Vehicle could not be found.";
    private const string ManufacturerNotFoundError = "The selected manufacturer does not exist.";
    private const string CategoryNotFoundError = "The selected vehicle category does not exist.";

    #endregion

    #region Dependencies & Constructor

    private readonly IVehicleRepository _vehicleRepository;
    private readonly IManufacturerRepository _manufacturerRepository;
    private readonly IVehicleCategoryRepository _vehicleCategoryRepository;

    public VehicleService(
        IVehicleRepository vehicleRepository,
        IManufacturerRepository manufacturerRepository,
        IVehicleCategoryRepository vehicleCategoryRepository)
    {
        _vehicleRepository = vehicleRepository;
        _manufacturerRepository = manufacturerRepository;
        _vehicleCategoryRepository = vehicleCategoryRepository;
    }

    #endregion

    #region List (Search, Paging)

    public VehicleListViewModel GetVehicles(
        string? search,
        int page,
        int pageSize)
    {
        if (pageSize <= 0)
        {
            pageSize = DefaultPageSize;
        }

        var totalItems = _vehicleRepository.Count(search);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

        var items = _vehicleRepository
            .GetPaged(search, page, pageSize)
            .Select(vehicle => new VehicleListItemViewModel
            {
                Id = vehicle.Id,
                OwnerName = vehicle.OwnerName,
                ManufacturerName = vehicle.Manufacturer?.Name ?? Unknown,
                YearOfManufacture = vehicle.YearOfManufacture,
                Weight = vehicle.Weight,
                CategoryName = vehicle.Category?.Name ?? Unknown
            })
            .ToList();

        return new VehicleListViewModel
        {
            Vehicles = items,
            Search = search,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    #endregion

    #region Details

    // Returns null when the id does not exist.
    public VehicleDetailsViewModel? GetDetails(int id)
    {
        var vehicle = _vehicleRepository.GetByIdWithDetails(id);
        return vehicle is null ? null : MapToDetailsViewModel(vehicle);
    }

    #endregion

    #region Create

    public VehicleFormViewModel GetCreateViewModel()
    {
        var model = new VehicleFormViewModel();
        PopulateDropdowns(model);
        return model;
    }

    // The category is not chosen by the user. It is derived from the weight.
    public (bool Success, string? ErrorMessage) Create(VehicleFormViewModel model)
    {
        if (!_vehicleRepository.HasManufacturer(model.ManufacturerId))
        {
            return (false, ManufacturerNotFoundError);
        }

        if (!TryResolveCategoryId(model.Weight, out var categoryId))
        {
            return (false, CategoryNotFoundError);
        }

        _vehicleRepository.Add(new Vehicle
        {
            OwnerName = model.OwnerName.Trim(),
            ManufacturerId = model.ManufacturerId,
            YearOfManufacture = model.YearOfManufacture,
            Weight = model.Weight,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow
        });
        _vehicleRepository.SaveChanges();
        return (true, null);
    }

    #endregion

    #region Edit

    // Loads the data for the edit form (null when the id does not exist).
    public VehicleFormViewModel? GetEditViewModel(int id)
    {
        var vehicle = _vehicleRepository.GetById(id);
        if (vehicle is null){ return null; }
        var model = new VehicleFormViewModel
        {
            Id = vehicle.Id,
            OwnerName = vehicle.OwnerName,
            ManufacturerId = vehicle.ManufacturerId,
            YearOfManufacture = vehicle.YearOfManufacture,
            Weight = vehicle.Weight,
            CategoryId = vehicle.CategoryId
        };
        PopulateDropdowns(model);
        return model;
    }

    // The category is re-derived from the weight on every update.
    public (bool Success, string? ErrorMessage) Update(VehicleFormViewModel model)
    {
        var vehicle = _vehicleRepository.GetById(model.Id);

        if (vehicle is null)
        {
            return (false, VehicleNotFoundError);
        }

        if (!_vehicleRepository.HasManufacturer(model.ManufacturerId))
        {
            return (false, ManufacturerNotFoundError);
        }

        if (!TryResolveCategoryId(model.Weight, out var categoryId))
        {
            return (false, CategoryNotFoundError);
        }

        vehicle.OwnerName = model.OwnerName.Trim();
        vehicle.ManufacturerId = model.ManufacturerId;
        vehicle.YearOfManufacture = model.YearOfManufacture;
        vehicle.Weight = model.Weight;
        vehicle.CategoryId = categoryId;
        vehicle.UpdatedAt = DateTime.UtcNow;
        _vehicleRepository.Update(vehicle);
        _vehicleRepository.SaveChanges();

        return (true, null);
    }

    #endregion

    #region Delete

    public VehicleDetailsViewModel? GetDeleteViewModel(int id) =>
        GetDetails(id);

    public (bool Success, string? ErrorMessage) Delete(int id)
    {
        var vehicle = _vehicleRepository.GetById(id);
        if (vehicle is null) {  return (false, VehicleNotFoundError);}
        vehicle.IsDeleted = true;
        vehicle.UpdatedAt = DateTime.UtcNow;
        _vehicleRepository.Update(vehicle);
        _vehicleRepository.SaveChanges();
        return (true, null);
    }

    #endregion

    #region Helpers

    // Finds the category whose weight range contains the given weight.
    private bool TryResolveCategoryId(decimal weight, out int categoryId)
    {
        var category = _vehicleCategoryRepository.GetByWeight(weight);
        categoryId = category?.Id ?? 0;
        return category is not null;
    }

    private void PopulateDropdowns(VehicleFormViewModel model)
    {
        // The repository already returns manufacturers ordered by name.
        // model.Manufacturers = _manufacturerRepository
        //     .GetAll()
        //     .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
        //     .ToList();

        model.Categories = _vehicleCategoryRepository
            .GetAll()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
    }

    private static VehicleDetailsViewModel MapToDetailsViewModel(Vehicle vehicle) =>
        new()
        {
            Id = vehicle.Id,
            OwnerName = vehicle.OwnerName,
            ManufacturerName = vehicle.Manufacturer?.Name ?? Unknown,
            YearOfManufacture = vehicle.YearOfManufacture,
            WeightKg = vehicle.Weight,
            CategoryName = vehicle.Category?.Name ?? Unknown
        };

    #endregion
}