using VehicleManagement.Models;   

namespace VehicleManagement.Repositories;

public record ManufacturerSummary(int Id, string Name, bool IsDefault, int VehicleCount);

public interface IManufacturerRepository
{
    // Index (list, search, sort, paging)
    int CountManufacturer(string? search);
    List<ManufacturerSummary> GetPage(
        string? search, string sortBy, bool descending, int skip, int take);

    List<Manufacturer> GetAll();

    // Details
    Manufacturer? GetWithVehicles(int id);
    
    // Edit / Delete 
    Manufacturer? GetById(int id);

    // Validation and business rules
    bool NameExists(string name, int? excludeId = null);
    bool HasVehicles(int id);

    // Create / Delete / persist
    void Add(Manufacturer manufacturer);
    void Update(Manufacturer manufacturer);
    int SaveChanges();
}