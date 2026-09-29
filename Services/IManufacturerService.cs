using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public enum DeleteResult
{
    Deleted,
    NotFound,
    IsDefault,
    HasVehicles
}

public interface IManufacturerService
{
    ManufacturerListViewModel GetPaged(
        string? search, string sortBy, string sortDirection, int page, int pageSize);

    ManufacturerDetailsViewModel? GetDetails(int id);
    ManufacturerFormViewModel? GetForEdit(int id);

    /// <summary>Case-insensitive uniqueness check. Pass excludeId when editing.</summary>
    bool NameExists(string name, int? excludeId = null);

    void Create(ManufacturerFormViewModel model);
    bool Update(ManufacturerFormViewModel model);
    DeleteResult Delete(int id);
}
