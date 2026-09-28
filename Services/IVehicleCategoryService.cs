using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleManagement.Models;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public interface IVehicleCategoryService
{
    #region Queries

    VehicleCategory? GetByWeight(decimal weight);

    VehicleCategoryListViewModel GetList();
    Vehicle? GetById(int id);

    #endregion

    #region Update

    void Update(VehicleCategory category);

    #endregion

    #region Delete

    bool Delete(int id);

    #endregion
}