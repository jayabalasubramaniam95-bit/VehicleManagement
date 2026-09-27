using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public interface IVehicleService
{
    Task<VehicleListViewModel> GetVehiclesAsync(
        string? search,
        string sortBy,
        string sortDirection,
        int page,
        int pageSize);

    Task<VehicleDetailsViewModel?> GetDetailsAsync(int id);

    Task<VehicleFormViewModel> GetCreateViewModelAsync();

    Task<VehicleFormViewModel?> GetEditViewModelAsync(int id);

    Task<(bool Success, string? ErrorMessage)> CreateAsync(VehicleFormViewModel model);

    Task<(bool Success, string? ErrorMessage)> UpdateAsync(VehicleFormViewModel model);

    Task<VehicleDetailsViewModel?> GetDeleteViewModelAsync(int id);

    Task<(bool Success, string? ErrorMessage)> DeleteAsync(int id);
}