using VehicleManagement.Models;

namespace VehicleManagement.Repositories;

public interface IVehicleRepository
{
    #region Queries

    IEnumerable<Vehicle> GetAll();

    Vehicle? GetById(int id);

    Vehicle? GetByIdWithDetails(int id);

    List<Vehicle> GetPaged(string? search, int pageNumber, int pageSize);

    int Count(string? search);

    #endregion

    #region Existence Checks

    bool Exists(int id);
    bool HasManufacturer(int manufacturerId);

    bool HasCategory(int categoryId);

    #endregion

    #region Commands

    void Add(Vehicle vehicle);

    void Update(Vehicle vehicle);

    void SaveChanges();

    #endregion
}