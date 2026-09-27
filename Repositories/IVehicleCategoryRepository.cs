using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories
{
    public interface IVehicleCategoryRepository
    {
        Task<List<VehicleCategory>> GetAllAsync(); 
        Task<VehicleCategory?> GetByWeightAsync(decimal weight);
    }
}