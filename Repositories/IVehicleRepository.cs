using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories
{
    public interface IVehicleRepository
{
    Task<int> CountAsync(string? search);
    Task<IEnumerable<Vehicle>> GetAllAsync();


    Task<List<Vehicle>> GetPagedAsync(
    string? search,
    string sortBy,
    string sortDirection,
    int page,
    int pageSize);

    Task<Vehicle?> GetByIdAsync(int id);

    Task<Vehicle?> GetByIdWithDetailsAsync(int id);

    Task AddAsync(Vehicle vehicle);

    void Update(Vehicle vehicle);

    void Delete(Vehicle vehicle);

    Task<bool> ExistsAsync(int id);

    Task<bool> HasManufacturerAsync(int manufacturerId);

    Task<bool> HasCategoryAsync(int categoryId);

    Task SaveChangesAsync();
}
}