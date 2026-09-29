using VehicleManagement.Models;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public enum CategorySaveStatus
{
    Success,
    NotFound,
    InvalidRange
}

public enum CategoryDeleteResult
{
    Deleted,
    NotFound,
    HasVehicles,
    LastCategory
}

public sealed record CategorySaveResult(CategorySaveStatus Status, string? Error = null)
{
    public static CategorySaveResult Success() => new(CategorySaveStatus.Success);
    public static CategorySaveResult NotFound() => new(CategorySaveStatus.NotFound);
    public static CategorySaveResult InvalidRange(string error) => new(CategorySaveStatus.InvalidRange, error);
}

public interface IVehicleCategoryService
{
    #region Queries

    /// <summary>Used by the vehicle module to auto-assign a category from a weight.</summary>
    VehicleCategory? GetByWeight(decimal weight);

    VehicleCategoryListViewModel GetList();

    VehicleCategoryFormViewModel? GetForEdit(int id);

    bool NameExists(string name, int? excludeId = null);

    #endregion

    #region Commands

    CategorySaveResult Create(VehicleCategoryFormViewModel model);

    CategorySaveResult Update(VehicleCategoryFormViewModel model);

    CategoryDeleteResult Delete(int id);

    #endregion
}
