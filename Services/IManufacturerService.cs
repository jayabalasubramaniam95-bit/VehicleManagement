using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Services
{
    public interface IManufacturerService
    {
         Task<ManufacturerListViewModel> GetManufacturersAsync(
        string? search,
        string sortBy,
        string sortDirection,
        int page,
        int pageSize);

    Task<ManufacturerDetailsViewModel?> GetDetailsAsync(int id);

    Task<bool> CreateAsync(
        ManufacturerCreateViewModel model);

    Task<ManufacturerEditViewModel?> GetEditAsync(int id);

    Task<bool> UpdateAsync(
        ManufacturerEditViewModel model);

    Task<bool> DeleteAsync(int id);
    }
}