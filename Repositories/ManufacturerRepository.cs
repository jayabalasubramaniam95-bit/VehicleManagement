using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleManagement.Data;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories
{
    public class ManufacturerRepository : IManufacturerRepository
    {
       private readonly ApplicationDbContext _context;

    public ManufacturerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Manufacturer>> GetAllAsync()
{
    return await _context.Manufacturers
        .AsNoTracking()
        .OrderBy(m => m.Name)
        .ToListAsync();
}

        public IQueryable<Manufacturer> GetQueryable()
    {
        return _context.Manufacturers
            .AsNoTracking();
    }

    public async Task<Manufacturer?> GetByIdAsync(int id)
    {
        return await _context.Manufacturers
            .Include(m => m.Vehicles)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(Manufacturer manufacturer)
    {
        await _context.Manufacturers.AddAsync(manufacturer);
    }

    public void Update(Manufacturer manufacturer)
    {
        _context.Manufacturers.Update(manufacturer);
    }

    public void Delete(Manufacturer manufacturer)
    {
        _context.Manufacturers.Remove(manufacturer);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Manufacturers
            .AnyAsync(m => m.Id == id);
    }

    public async Task<bool> NameExistsAsync(
        string name,
        int? excludeId = null)
    {
        var query = _context.Manufacturers
            .AsQueryable();

        if (excludeId.HasValue)
        {
            query = query.Where(
                m => m.Id != excludeId.Value);
        }

        return await query.AnyAsync(
            m => m.Name.ToLower() == name.ToLower());
    }

    public async Task<bool> HasVehiclesAsync(int id)
    {
        return await _context.Vehicles
            .AnyAsync(v => v.ManufacturerId == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    }
}