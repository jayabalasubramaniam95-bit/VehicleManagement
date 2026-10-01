using VehicleManagement.Models;   

namespace VehicleManagement.Repositories;

public record ManufacturerSummary(int Id, string Name, bool IsDefault, int VehicleCount);

public interface IManufacturerRepository
{
    int CountManufacturer(string? search);
    List<ManufacturerSummary> GetPageWiseManufacturer(
        string? search, int skip, int take);
    List<Manufacturer> GetAllManufacturer();
    Manufacturer? GetManufacturersWithVehicles(int id);
    Manufacturer? GetManufacturersById(int id);
    bool IsManufacturersNameExists(string name, int? excludeId = null);
    bool IsManufacturersHasVehicles(int id);
    void Add(Manufacturer manufacturer);
    void Update(Manufacturer manufacturer);
}