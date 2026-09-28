using Microsoft.EntityFrameworkCore;
using VehicleManagement.Data;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories;

public class ManufacturerRepository : IManufacturerRepository
{
    #region Dependencies & Constructor

    private readonly ApplicationDbContext _context;

    public ManufacturerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    #endregion

    #region Queries

    public IQueryable<Manufacturer> GetQueryable() =>
        _context.Manufacturers.AsNoTracking();

    public List<Manufacturer> GetAll() =>
        _context.Manufacturers
            .AsNoTracking()
            .OrderBy(m => m.Name)
            .ToList();

    public Manufacturer? GetById(int id) =>
        _context.Manufacturers
            .FirstOrDefault(m => m.Id == id);

    public Manufacturer? GetByIdWithVehicles(int id) =>
        _context.Manufacturers
            .AsNoTracking()
            .Include(m => m.Vehicles)
            .FirstOrDefault(m => m.Id == id);

    #endregion

    #region Existence Checks

    public bool Exists(int id) =>
        _context.Manufacturers.Any(m => m.Id == id);

    public bool NameExists(string name, int? excludeId = null) =>
        _context.Manufacturers
            .Any(m => m.Name == name && (excludeId == null || m.Id != excludeId));

    public bool HasVehicles(int id) =>
        _context.Vehicles.Any(v => v.ManufacturerId == id);

    #endregion

    #region Commands

    public void Add(Manufacturer manufacturer) =>
        _context.Manufacturers.Add(manufacturer);

    public void Update(Manufacturer manufacturer) =>
        _context.Manufacturers.Update(manufacturer);

    public void Delete(Manufacturer manufacturer) =>
        _context.Manufacturers.Remove(manufacturer);

    public void SaveChanges() =>
        _context.SaveChanges();

    #endregion
}