using Microsoft.EntityFrameworkCore;
using VehicleManagement.Data;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories;

public class VehicleRepository : IVehicleRepository
{
    #region Dependencies & Constructor

    private readonly ApplicationDbContext _context;

    public VehicleRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    #endregion

    #region Queries

    public IEnumerable<Vehicle> GetAll() =>
     _context.Vehicles.Where(v => !v.IsDeleted).Include(v => v.Manufacturer).Where(v => !v.Manufacturer.IsDeleted).OrderBy(v => v.OwnerName).ToList();

    public Vehicle? GetById(int id) =>
        _context.Vehicles.Where(v => !v.IsDeleted)
            .Include(v => v.Manufacturer).Where(v => !v.Manufacturer.IsDeleted)
            .FirstOrDefault(v => v.Id == id );

    public Vehicle? GetByIdWithDetails(int id) =>
        GetById(id);

    public List<Vehicle> GetPaged(string? search, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Max(pageSize, 1);

        return ApplySearch(_context.Vehicles.Include(v => v.Manufacturer).Where(v => !v.Manufacturer.IsDeleted), search)
            .OrderBy(v => v.OwnerName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public int Count(string? search) =>
        ApplySearch(_context.Vehicles, search).Count();

    #endregion

    #region Existence Checks

    public bool Exists(int id) =>
        _context.Vehicles.Any(v =>  !v.IsDeleted && v.Id == id);

    public bool HasManufacturer(int manufacturerId) =>
        _context.Manufacturers.Any(m => !m.IsDeleted && m.Id == manufacturerId );

    public bool HasCategory(int categoryId) =>
        _context.VehicleCategories.Any(c => !c.IsDeleted && c.Id == categoryId);

    #endregion

    #region Commands

    public void Add(Vehicle vehicle) =>
        _context.Vehicles.Add(vehicle);

    public void Update(Vehicle vehicle) =>
        _context.Vehicles.Update(vehicle);

    public void SaveChanges() =>
        _context.SaveChanges();

    #endregion

    #region Helpers

    private static IQueryable<Vehicle> ApplySearch(IQueryable<Vehicle> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) { return query; }
        search = search.Trim();
        return query.Where(v => !v.IsDeleted &&
            v.OwnerName.Contains(search) ||
            (v.Manufacturer != null && v.Manufacturer.Name.Contains(search)));
    }

    #endregion
}
