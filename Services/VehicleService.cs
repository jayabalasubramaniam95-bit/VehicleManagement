using Microsoft.AspNetCore.Mvc.Rendering;
using VehicleManagement.Models;
using VehicleManagement.Repositories;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public class VehicleService : IVehicleService
{
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

    // ---------- Index ----------

    public async Task<VehicleListViewModel> GetVehiclesAsync(
        string? search,
        string sortBy,
        string sortDirection,
        int page,
        int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (string.IsNullOrWhiteSpace(sortBy)) sortBy = "OwnerName";
        if (string.IsNullOrWhiteSpace(sortDirection)) sortDirection = "Ascending";

        var totalItems = await _vehicleRepository.CountAsync(search);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var vehicles = await _vehicleRepository.GetPagedAsync(search, sortBy, sortDirection, page, pageSize);

        var items = vehicles
            .Select(vehicle => new VehicleListItemViewModel
            {
                Id = vehicle.Id,
                OwnerName = vehicle.OwnerName,
                ManufacturerName = vehicle.Manufacturer?.Name ?? "Unknown",
                YearOfManufacture = vehicle.YearOfManufacture,
                WeightKg = vehicle.WeightKg,
                CategoryName = vehicle.Category?.Name ?? "Unknown"
            })
            .ToList();

        return new VehicleListViewModel
        {
            Vehicles = items,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    // ---------- Details ----------

    public async Task<VehicleDetailsViewModel?> GetDetailsAsync(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdWithDetailsAsync(id);
        return vehicle == null ? null : MapToDetailsViewModel(vehicle);
    }

    // ---------- Create (GET) ----------

    public async Task<VehicleFormViewModel> GetCreateViewModelAsync()
    {
        var model = new VehicleFormViewModel();
        await PopulateDropdownsAsync(model);
        return model;
    }

    // ---------- Create (POST) ----------

    public async Task<(bool Success, string? ErrorMessage)> CreateAsync(VehicleFormViewModel model)
    {
        if (!await _vehicleRepository.HasManufacturerAsync(model.ManufacturerId))
        {
            return (false, "The selected manufacturer does not exist.");
        }

        if (!await _vehicleRepository.HasCategoryAsync(model.CategoryId))
        {
            return (false, "The selected vehicle category does not exist.");
        }

        var vehicle = new Vehicle
        {
            OwnerName = model.OwnerName.Trim(),
            ManufacturerId = model.ManufacturerId,
            YearOfManufacture = model.YearOfManufacture,
            WeightKg = model.WeightKg,
            CategoryId = model.CategoryId
        };

        await _vehicleRepository.AddAsync(vehicle);
        await _vehicleRepository.SaveChangesAsync();

        return (true, null);
    }

    // ---------- Edit (GET) ----------

    public async Task<VehicleFormViewModel?> GetEditViewModelAsync(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle == null)
        {
            return null;
        }

        var model = new VehicleFormViewModel
        {
            Id = vehicle.Id,
            OwnerName = vehicle.OwnerName,
            ManufacturerId = vehicle.ManufacturerId,
            YearOfManufacture = vehicle.YearOfManufacture,
            WeightKg = vehicle.WeightKg,
            CategoryId = vehicle.CategoryId
        };

        await PopulateDropdownsAsync(model);

        return model;
    }

    // ---------- Edit (POST) ----------

    public async Task<(bool Success, string? ErrorMessage)> UpdateAsync(VehicleFormViewModel model)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(model.Id);

        if (vehicle == null)
        {
            return (false, "Vehicle could not be found.");
        }

        if (!await _vehicleRepository.HasManufacturerAsync(model.ManufacturerId))
        {
            return (false, "The selected manufacturer does not exist.");
        }

        if (!await _vehicleRepository.HasCategoryAsync(model.CategoryId))
        {
            return (false, "The selected vehicle category does not exist.");
        }

        vehicle.OwnerName = model.OwnerName.Trim();
        vehicle.ManufacturerId = model.ManufacturerId;
        vehicle.YearOfManufacture = model.YearOfManufacture;
        vehicle.WeightKg = model.WeightKg;
        vehicle.CategoryId = model.CategoryId;

        _vehicleRepository.Update(vehicle);
        await _vehicleRepository.SaveChangesAsync();

        return (true, null);
    }

    // ---------- Delete (GET) ----------

    public Task<VehicleDetailsViewModel?> GetDeleteViewModelAsync(int id) => GetDetailsAsync(id);

    // ---------- Delete (POST) ----------

    public async Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle == null)
        {
            return (false, "Vehicle could not be found.");
        }

        _vehicleRepository.Delete(vehicle);
        await _vehicleRepository.SaveChangesAsync();

        return (true, null);
    }

    // ---------- Dropdowns ----------

    private async Task PopulateDropdownsAsync(VehicleFormViewModel model)
    {
        var manufacturers = await _manufacturerRepository.GetAllAsync();
        var categories = await _vehicleCategoryRepository.GetAllAsync();

        model.Manufacturers = manufacturers
            .OrderBy(m => m.Name)
            .Select(m => new SelectListItem { Value = m.Id.ToString(), Text = m.Name })
            .ToList();

        model.Categories = categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToList();
    }

    // ---------- Mapping ----------

    private static VehicleDetailsViewModel MapToDetailsViewModel(Vehicle vehicle)
    {
        return new VehicleDetailsViewModel
        {
            Id = vehicle.Id,
            OwnerName = vehicle.OwnerName,
            ManufacturerName = vehicle.Manufacturer?.Name ?? "Unknown",
            YearOfManufacture = vehicle.YearOfManufacture,
            WeightKg = vehicle.WeightKg,
            CategoryName = vehicle.Category?.Name ?? "Unknown"
        };
    }
}