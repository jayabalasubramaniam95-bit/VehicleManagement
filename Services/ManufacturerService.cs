using VehicleManagement.Models;
using VehicleManagement.Repositories;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public class ManufacturerService : IManufacturerService
{
    #region Dependencies & Constructor

    private readonly IManufacturerRepository _repository;

    public ManufacturerService(IManufacturerRepository repository)
    {
        _repository = repository;
    }

    #endregion

    #region List (Search, Paging)

    public ManufacturerListViewModel GetManufacturers(
        string? search,
        int page,
        int pageSize)
    {
        pageSize = Math.Max(pageSize, 1);
        search = search?.Trim();

        var query = _repository.GetQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Name.Contains(search));
        }

        var totalItems = query.Count();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        // Keep the page inside 1..totalPages (page 1 when there are no results).
        page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

        var items = query
            .OrderBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ManufacturerListItemViewModel
            {
                Id = m.Id,
                Name = m.Name,
                VehicleCount = m.Vehicles.Count
            })
            .ToList();

        return new ManufacturerListViewModel
        {
            Manufacturers = items,
            Search = search,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    #endregion

    #region Details

    public ManufacturerDetailsViewModel? GetDetailsById(int id)
    {
        var manufacturer = _repository.GetByIdWithVehicles(id);

        if (manufacturer is null)
        {
            return null;
        }

        return new ManufacturerDetailsViewModel
        {
            Id = manufacturer.Id,
            Name = manufacturer.Name,
            VehicleCount = manufacturer.Vehicles.Count,
            Vehicles = manufacturer.Vehicles
                .Select(v => new VehicleListItemViewModel
                {
                    Id = v.Id,
                    OwnerName = v.OwnerName,
                    ManufacturerName = manufacturer.Name,
                    YearOfManufacture = v.YearOfManufacture,
                    Weight = v.Weight,
                    CategoryName = v.Category?.Name ?? string.Empty
                })
                .ToList()
        };
    }

    #endregion

    #region Create

    public bool Create(ManufacturerCreateViewModel model)
    {
        var name = model.Name.Trim();

        if (_repository.NameExists(name))
        {
            return false;
        }

        _repository.Add(new Manufacturer { Name = name });
        _repository.SaveChanges();

        return true;
    }

    #endregion

    #region Edit

    public ManufacturerEditViewModel? GetEdit(int id)
    {
        var manufacturer = _repository.GetById(id);

        return manufacturer is null
            ? null
            : new ManufacturerEditViewModel
            {
                Id = manufacturer.Id,
                Name = manufacturer.Name
            };
    }

    public bool Update(ManufacturerEditViewModel model)
    {
        var manufacturer = _repository.GetById(model.Id);

        if (manufacturer is null)
        {
            return false;
        }

        var name = model.Name.Trim();

        if (_repository.NameExists(name, excludeId: model.Id))
        {
            return false;
        }

        manufacturer.Name = name;

        _repository.Update(manufacturer);
        _repository.SaveChanges();

        return true;
    }

    #endregion

    #region Delete

    public bool Delete(int id)
    {
        var manufacturer = _repository.GetById(id);

        if (manufacturer is null || _repository.HasVehicles(id))
        {
            return false;
        }

        _repository.Delete(manufacturer);
        _repository.SaveChanges();

        return true;
    }

    #endregion
}