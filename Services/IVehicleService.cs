using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public interface IVehicleService
{
    #region List (Search, Paging)

    VehicleListViewModel GetVehicles(string? search, int page, int pageSize);

    #endregion

    #region Details

    VehicleDetailsViewModel? GetDetails(int id);

    #endregion

    #region Create

    VehicleFormViewModel GetCreateViewModel();

    (bool Success, string? ErrorMessage) Create(VehicleFormViewModel model);

    #endregion

    #region Edit

    VehicleFormViewModel? GetEditViewModel(int id);

    (bool Success, string? ErrorMessage) Update(VehicleFormViewModel model);

    #endregion

    #region Delete

    VehicleDetailsViewModel? GetDeleteViewModel(int id);

    (bool Success, string? ErrorMessage) Delete(int id);

    #endregion
}