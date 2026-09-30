using VehicleManagement.Models;

namespace VehicleManagement.Repositories;

/// <summary>Read projection for the list page: one query, no entity graphs loaded.</summary>
public record VehicleSummary(
    int Id,
    string OwnerName,
    string ManufacturerName,
    int YearOfManufacture,
    decimal Weight,
    string? CategoryName,
    string? CategoryIcon);

/// <summary>Contains only what VehicleService and VehicleCategoryService need.</summary>
public interface IVehicleRepository
{
    #region Queries

    /// <summary>All active vehicles, tracked (used by VehicleCategoryService to re-categorise).</summary>
    List<Vehicle> GetAll();

    /// <summary>Single vehicle, tracked (used by update and delete).</summary>
    Vehicle? GetById(int id);

    /// <summary>Single vehicle with manufacturer and category, not tracked (details and edit form).</summary>
    Vehicle? GetByIdWithDetails(int id);

    List<VehicleSummary> GetPaged(string? search, int pageNumber, int pageSize);

    int Count(string? search);

    #endregion

    #region Existence Checks

    bool HasManufacturer(int manufacturerId);

    #endregion

    #region Commands

    void Add(Vehicle vehicle);

    void Update(Vehicle vehicle);

    void SaveChanges();

    #endregion
}
