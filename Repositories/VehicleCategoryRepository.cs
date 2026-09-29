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

    #endregion

    #region Queries

    public List<VehicleCategory> GetAll() =>
        _context.VehicleCategories
            .AsNoTracking()
            .OrderBy(c => c.MinWeight)
            .ToList();

    public VehicleCategory? GetById(int id) =>
        _context.VehicleCategories
            .FirstOrDefault(c => c.Id == id);

    // MinWeight is inclusive, MaxWeight is exclusive (null = no upper limit).
    public VehicleCategory? GetByWeight(decimal weight) =>
        _context.VehicleCategories
            .AsNoTracking()
            .FirstOrDefault(c =>
                weight >= c.MinWeight &&
                (c.MaxWeight == null || weight < c.MaxWeight));

    public List<VehicleCategory> GetCategoriesExcept(int id) =>
        _context.VehicleCategories
            .AsNoTracking()
            .Where(c => c.Id != id)
            .OrderBy(c => c.MinWeight)
            .ToList();

    #endregion

    #region Existence Checks

    public bool NameExists(string name, int? excludeId = null) =>
        _context.VehicleCategories
            .Any(c => c.Name == name && (excludeId == null || c.Id != excludeId));

    public bool HasVehicles(int categoryId) =>
        _context.Vehicles.Any(v => v.CategoryId == categoryId);

    #endregion

    #region Commands

    public void Update(VehicleCategory category) =>
        _context.VehicleCategories.Update(category);

    public void SaveChanges() =>
        _context.SaveChanges();

    #endregion
}