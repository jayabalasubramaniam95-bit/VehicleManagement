using VehicleManagement.Models;

namespace VehicleManagement.Repositories;

/// <summary>Read projection for the list page: one query instead of one HasVehicles call per row.</summary>
public record VehicleCategorySummary(
    int Id, string Name, string? Icon, decimal MinWeight, decimal? MaxWeight, int VehicleCount);

/// <summary>Contains only what VehicleCategoryService needs.</summary>
public interface IVehicleCategoryRepository
{
    #region Queries

    List<VehicleCategorySummary> GetSummaries();

    /// <summary>All active categories, tracked, ordered by MinWeight (used by write operations).</summary>
    List<VehicleCategory> GetAll();

    /// <summary>Single category, not tracked (used to fill the edit form).</summary>
    VehicleCategory? GetById(int id);

    VehicleCategory? GetByWeight(decimal weight);

    #endregion

    #region Existence Checks

    bool NameExists(string name, int? excludeId = null);

    bool HasVehicles(int categoryId);

    #endregion

    #region Commands

    void Add(VehicleCategory category);

    void Update(VehicleCategory category);

    void SaveChanges();

    /// <summary>Runs several SaveChanges calls as one all-or-nothing unit.</summary>
    void InTransaction(Action work);

    #endregion
}
