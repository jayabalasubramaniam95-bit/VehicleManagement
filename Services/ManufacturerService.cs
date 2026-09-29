using Microsoft.EntityFrameworkCore;
using VehicleManagement.Data;      // adjust to your DbContext namespace
using VehicleManagement.Models;    // adjust to your entity namespace
using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public class ManufacturerService : IManufacturerService
{
    private readonly ApplicationDbContext _db;

    public ManufacturerService(ApplicationDbContext db) => _db = db;

    public  ManufacturerListViewModel GetPaged(
        string? search, string sortBy, string sortDirection, int page, int pageSize)
    {
        // Whitelist user input: anything unexpected falls back to the default sort
        sortBy = sortBy == "vehicles" ? "vehicles" : "name";
        sortDirection = sortDirection == "desc" ? "desc" : "asc";
        search = search?.Trim();

        var query = _db.Manufacturers.Where(m=>!m.IsDeleted).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search));

        var totalItems = query.Count();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        var ordered = (sortBy, sortDirection) switch
        {
            ("vehicles", "desc") => query.OrderByDescending(m => m.Vehicles.Count).ThenBy(m => m.Name),
            ("vehicles", _)      => query.OrderBy(m => m.Vehicles.Count).ThenBy(m => m.Name),
            (_, "desc")          => query.OrderByDescending(m => m.Name),
            _                    => query.OrderBy(m => m.Name)
        };

        var items = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ManufacturerListItemViewModel
            {
                Id = m.Id,
                Name = m.Name,
                IsDefault = m.IsDefault,
                VehicleCount = m.Vehicles.Count
            })
            .ToList();

        return new ManufacturerListViewModel
        {
            Manufacturers = items,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public ManufacturerDetailsViewModel? GetDetails(int id) =>
        _db.Manufacturers
            .AsNoTracking()
            .Where(m => m.Id == id && !m.IsDeleted)
            .Select(m => new ManufacturerDetailsViewModel
            {
                Id = m.Id,
                Name = m.Name,
                IsDefault = m.IsDefault,
                VehicleCount = m.Vehicles.Count,
                Vehicles = m.Vehicles
                    .Select(v => new VehicleListItemViewModel
                    {
                        Id = v.Id
                    })
                    .ToList()
            })
            .FirstOrDefault();

    public ManufacturerFormViewModel? GetForEdit(int id) =>
        _db.Manufacturers
            .AsNoTracking()
            .Where(m => m.Id == id && !m.IsDeleted)
            .Select(m => new ManufacturerFormViewModel { Id = m.Id, Name = m.Name })
            .FirstOrDefault();

    public bool NameExists(string name, int? excludeId = null)
    {
        var normalized = name.Trim().ToLower();
        return _db.Manufacturers.Any(m =>
            m.Name.ToLower() == normalized && m.Id != excludeId && !m.IsDeleted);
    }

    public  void Create(ManufacturerFormViewModel model)
    {
        _db.Manufacturers.Add(new Manufacturer { Name = model.Name, IsDefault = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        _db.SaveChanges();
    }

    public  bool Update(ManufacturerFormViewModel model)
    {
        var entity = _db.Manufacturers.Find(model.Id);
        if (entity is null) return false;

        entity.Name = model.Name;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return true;
    }

    public  DeleteResult Delete(int id)
    {
        var entity =  _db.Manufacturers.Find(id);
        if (entity is null) return DeleteResult.NotFound;
        if (entity.IsDefault) return DeleteResult.IsDefault;
        if (_db.Vehicles.Any(v => v.ManufacturerId == id)) return DeleteResult.HasVehicles;
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.Manufacturers.Update(entity);
        _db.SaveChanges();
        return DeleteResult.Deleted;
    }
}
