using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories
{
    public interface IManufacturerRepository
    {
        Task<List<Manufacturer>> GetAllAsync();
        IQueryable<Manufacturer> GetQueryable();
        

    Task<Manufacturer?> GetByIdAsync(int id);

    Task AddAsync(Manufacturer manufacturer);

    void Update(Manufacturer manufacturer);

    void Delete(Manufacturer manufacturer);

    Task<bool> ExistsAsync(int id);

    Task<bool> NameExistsAsync(
        string name,
        int? excludeId = null);

    Task<bool> HasVehiclesAsync(int id);

    Task SaveChangesAsync();
    }
}