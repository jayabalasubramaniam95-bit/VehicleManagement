using Microsoft.EntityFrameworkCore;
using VehicleManagement.Data;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories;

public class VehicleRepository : IVehicleRepository
{
    private readonly ApplicationDbContext _context;

    public VehicleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .OrderBy(v => v.OwnerName)
            .ToListAsync();
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<Vehicle?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Vehicles
            .Include(v => v.Manufacturer)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<List<Vehicle>> GetPagedAsync(
    string? search,
    string sortColumn,
    string sortDirection,
    int pageNumber,
    int pageSize)
{
    var query = _context.Vehicles.Include(v => v.Manufacturer).AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        search = search.Trim();

        query = query.Where(v =>
            v.OwnerName.Contains(search) ||
            (v.Manufacturer != null && v.Manufacturer.Name.Contains(search)));
    }

    var descending = sortDirection?.ToLower() == "desc";

    query = sortColumn?.ToLower() switch
    {
        "ownername" => descending
            ? query.OrderByDescending(v => v.OwnerName)
            : query.OrderBy(v => v.OwnerName),

        "manufacturer" => descending
            ? query.OrderByDescending(v => v.Manufacturer!.Name)
            : query.OrderBy(v => v.Manufacturer!.Name),

        "year" => descending
            ? query.OrderByDescending(v => v.YearOfManufacture)
            : query.OrderBy(v => v.YearOfManufacture),

        _ => query.OrderBy(v => v.OwnerName)
    };

    return await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}

    public async Task<int> CountAsync(string? search)
    {
        var query = _context.Vehicles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query
                .Include(v => v.Manufacturer)
                .Where(v =>
                    v.OwnerName.Contains(search) ||
                    (v.Manufacturer != null && v.Manufacturer.Name.Contains(search)));
        }

        return await query.CountAsync();
    }

    public async Task<bool> HasManufacturerAsync(int manufacturerId)
    {
        return await _context.Manufacturers.AnyAsync(m => m.Id == manufacturerId);
    }

    public async Task<bool> HasCategoryAsync(int categoryId)
    {
        return await _context.VehicleCategories.AnyAsync(c => c.Id == categoryId);
    }

    public async Task AddAsync(Vehicle vehicle)
    {
        await _context.Vehicles.AddAsync(vehicle);
    }

    public void Update(Vehicle vehicle)
    {
        _context.Vehicles.Update(vehicle);
    }

    public void Delete(Vehicle vehicle)
    {
        _context.Vehicles.Remove(vehicle);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Vehicles.AnyAsync(v => v.Id == id);
    }

    public async Task DeleteAsync(int id)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle != null)
        {
            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
        }
    }
}