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

    // Every read starts here, so deleted vehicles (or vehicles of deleted manufacturers) never leak in
    private IQueryable<Vehicle> Active =>
        _context.Vehicles.Where(v => !v.IsDeleted && !v.Manufacturer.IsDeleted);

    #endregion

    #region Queries

    public List<Vehicle> GetAll() =>
        Active
            .OrderBy(v => v.OwnerName)
            .ToList();

    public Vehicle? GetById(int id) =>
        Active.FirstOrDefault(v => v.Id == id);

    public Vehicle? GetByIdWithDetails(int id) =>
        Active
            .AsNoTracking()
            .Include(v => v.Manufacturer)
            .Include(v => v.Category)
            .FirstOrDefault(v => v.Id == id);

    public List<VehicleSummary> GetPaged(string? search, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Max(pageSize, 1);

        return ApplySearch(Active, search)
            .AsNoTracking()
            .OrderBy(v => v.OwnerName)
            .ThenBy(v => v.Id)                      // stable order so paging never repeats or skips rows
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VehicleSummary(
                v.Id,
                v.OwnerName,
                v.Manufacturer.Name,
                v.YearOfManufacture,
                v.Weight,
                v.Category != null ? v.Category.Name : null,
                v.Category != null ? v.Category.Icon : null))
            .ToList();
    }

    // Uses the same base query as GetPaged, so the count always matches the rows
    public int Count(string? search) =>
        ApplySearch(Active, search).Count();

    #endregion

    #region Existence Checks

    public bool HasManufacturer(int manufacturerId) =>
        _context.Manufacturers.Any(m => !m.IsDeleted && m.Id == manufacturerId);

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
        if (string.IsNullOrWhiteSpace(search)) return query;

        var term = search.Trim();

        return query.Where(v =>
            v.OwnerName.Contains(term) ||
            v.Manufacturer.Name.Contains(term) ||
            (v.Category != null && v.Category.Name.Contains(term)));
    }

    #endregion
}
