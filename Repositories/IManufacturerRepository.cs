using VehicleManagement.Models;

namespace VehicleManagement.Repositories;

public interface IManufacturerRepository
{
    #region Queries

    IQueryable<Manufacturer> GetQueryable();

    List<Manufacturer> GetAll();

    Manufacturer? GetById(int id);

    Manufacturer? GetByIdWithVehicles(int id);

    #endregion

    #region Existence Checks


    bool Exists(int id);

    bool NameExists(
        string name,
        int? excludeId = null);

    bool HasVehicles(int id);

    #endregion

    #region Commands

    void Add(Manufacturer manufacturer);

    void Update(Manufacturer manufacturer);

    void Delete(Manufacturer manufacturer);

    void SaveChanges();

    #endregion
}