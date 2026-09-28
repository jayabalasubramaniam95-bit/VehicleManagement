using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories
{
    public interface IVehicleCategoryRepository
    {
        #region Queries

        List<VehicleCategory> GetAll();

        VehicleCategory? GetById(int id);

        VehicleCategory? GetByWeight(decimal weight);

        List<VehicleCategory> GetCategoriesExcept(int id);

        #endregion

        #region Existence Checks

        bool NameExists(string name, int? excludeId = null);

        bool HasVehicles(int categoryId);

        #endregion

        #region Commands

        void Update(VehicleCategory category);

        void Delete(VehicleCategory category);

        void SaveChanges();

        #endregion
    }
}