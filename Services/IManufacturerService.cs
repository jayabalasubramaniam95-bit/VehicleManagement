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
    ManufacturerListViewModel GetPageWiseManufacturer( string? search, int page, int pageSize);
    ManufacturerDetailsViewModel? GetManufacturerDetails(int id);
    ManufacturerFormViewModel? GetManufacturerForEdit(int id);
    bool IsManufacturersNameExists(string name, int? excludeId = null);
    void Create(ManufacturerFormViewModel model);
    bool Update(ManufacturerFormViewModel model);
    DeleteResult Delete(int id);
}
