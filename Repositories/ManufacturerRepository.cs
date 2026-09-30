using Microsoft.EntityFrameworkCore;
using VehicleManagement.Data;     // adjust to your DbContext namespace
using VehicleManagement.Models;   // adjust to your entity namespace

namespace VehicleManagement.Repositories;

public class ManufacturerRepository : IManufacturerRepository
{
    private readonly ApplicationDbContext _db;

    public ManufacturerRepository(ApplicationDbContext db) => _db = db;

    public int CountManufacturer(string? search) =>
        Filter(search).Count();

    public List<ManufacturerSummary> GetPage(
        string? search, string sortBy, bool descending, int skip, int take)
    {
        var query = Filter(search);

        var ordered = (sortBy, descending) switch
        {
            ("vehicles", true)  => query.OrderByDescending(m => m.Vehicles.Count).ThenBy(m => m.Name),
            ("vehicles", false) => query.OrderBy(m => m.Vehicles.Count).ThenBy(m => m.Name),
            (_, true)           => query.OrderByDescending(m => m.Name),
            _                   => query.OrderBy(m => m.Name)
        };

        return ordered
            .Skip(skip)
            .Take(take)
            .Select(m => new ManufacturerSummary(m.Id, m.Name, m.IsDefault, m.Vehicles.Count))
            .ToList();
    }

    public Manufacturer? GetWithVehicles(int id) =>
        _db.Manufacturers
           .AsNoTracking()
           .Include(m => m.Vehicles)
           .FirstOrDefault(m => m.Id == id && !m.IsDeleted);

    public Manufacturer? GetById(int id) =>
        _db.Manufacturers.FirstOrDefault(m => m.Id == id && !m.IsDeleted);

    public bool NameExists(string name, int? excludeId = null)
    {
        var normalized = name.Trim().ToLower();
        return _db.Manufacturers.Any(m => m.Name.ToLower() == normalized && m.Id != excludeId && !m.IsDeleted);
    }

    public List<Manufacturer> GetAll()
    {
        return _db.Manufacturers.AsNoTracking().OrderBy(m => m.Name).ToList();
    }

    public bool HasVehicles(int id) =>
        _db.Vehicles.Any(v => v.ManufacturerId == id && !v.IsDeleted);

    public  void Add(Manufacturer manufacturer) =>
         _db.Manufacturers.Add(manufacturer);

    public void Update(Manufacturer manufacturer) =>
        _db.Manufacturers.Update(manufacturer);

    public int SaveChanges() => _db.SaveChanges();

    // Shared by Count and GetPage so both always apply the same filter
    private IQueryable<Manufacturer> Filter(string? search)
    {
        var query = _db.Manufacturers.Where(m => !m.IsDeleted).AsNoTracking();
        return string.IsNullOrWhiteSpace(search)
            ? query
            : query.Where(m => m.Name.Contains(search.Trim()));
    }
}