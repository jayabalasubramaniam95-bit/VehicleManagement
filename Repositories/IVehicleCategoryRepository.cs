using VehicleManagement.Models;

namespace VehicleManagement.Repositories;

public record VehicleCategorySummary(
    int Id, string Name, string? Icon, decimal MinWeight, decimal? MaxWeight, int VehicleCount);

public interface IVehicleCategoryRepository
{
    #region Queries

    List<VehicleCategorySummary> GetVehicleCategorySummaries();

    List<VehicleCategory> GetAllVehicleCategory();

    VehicleCategory? GetVehicleCategoryById(int id);

    VehicleCategory? GetVehicleCategoryByWeight(decimal weight);

    #endregion

    #region Existence Checks

    bool IsVehicleCategoryNameExists(string name, int? excludeId = null);

    bool HasVehicles(int categoryId);

    #endregion

    #region Commands

    void Add(VehicleCategory category);

    void Update(VehicleCategory category);

    void InTransaction(Action work);

    #endregion
}
