using Microsoft.EntityFrameworkCore;
using VehicleManagement.Data;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories;

public class VehicleCategoryRepository : IVehicleCategoryRepository
{
    #region Dependencies & Constructor

    private readonly ApplicationDbContext _context;

    public VehicleCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Soft-deleted categories are never returned
    private IQueryable<VehicleCategory> Active =>
        _context.VehicleCategories.Where(c => !c.IsDeleted);

    #endregion

    #region Queries

    public List<VehicleCategorySummary> GetSummaries() =>
        Active
            .AsNoTracking()
            .OrderBy(c => c.MinWeight)
            .Select(c => new VehicleCategorySummary(
                c.Id,
                c.Name,
                c.Icon,
                c.MinWeight,
                c.MaxWeight,
                _context.Vehicles.Count(v => v.CategoryId == c.Id)))
            .ToList();

    public List<VehicleCategory> GetAll() =>
        Active
            .OrderBy(c => c.MinWeight)
            .ToList();

    public VehicleCategory? GetById(int id) =>
        Active
            .AsNoTracking()
            .FirstOrDefault(c => c.Id == id);

    // MinWeight is inclusive, MaxWeight is exclusive (null = no upper limit).
    public VehicleCategory? GetByWeight(decimal weight) =>
        Active
            .AsNoTracking()
            .FirstOrDefault(c =>
                weight >= c.MinWeight &&
                (c.MaxWeight == null || weight < c.MaxWeight));

    #endregion

    #region Existence Checks

    public bool NameExists(string name, int? excludeId = null)
    {
        var normalized = name.Trim().ToLower();

        return Active.Any(c =>
            c.Name.ToLower() == normalized &&
            (excludeId == null || c.Id != excludeId));
    }

    public bool HasVehicles(int categoryId) =>
        _context.Vehicles.Any(v => v.CategoryId == categoryId);

    #endregion

    #region Commands

    public void Add(VehicleCategory category) =>
        _context.VehicleCategories.Add(category);

    public void Update(VehicleCategory category) =>
        _context.VehicleCategories.Update(category);

    public void SaveChanges() =>
        _context.SaveChanges();

    public void InTransaction(Action work)
    {
        using var transaction = _context.Database.BeginTransaction();
        work();
        transaction.Commit();   // an exception in work() skips this, so the transaction rolls back
    }

    #endregion
}
