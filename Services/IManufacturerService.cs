using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Services
{
    public interface IManufacturerService
    {
    ManufacturerListViewModel GetManufacturers( string? search, int page, int pageSize);

    ManufacturerDetailsViewModel GetDetailsById(int id);

    ManufacturerEditViewModel GetEdit(int id);
    bool Create(ManufacturerCreateViewModel model);

    bool Update(ManufacturerEditViewModel model);

    bool Delete(int id);
    }
}