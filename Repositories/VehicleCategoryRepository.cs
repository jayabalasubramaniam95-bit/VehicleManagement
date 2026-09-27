using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VehicleManagement.Data;
using VehicleManagement.Models;

namespace VehicleManagement.Repositories
{
    public class VehicleCategoryRepository : IVehicleCategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public VehicleCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<VehicleCategory>> GetAllAsync()
        {
            return await _context.VehicleCategories.AsNoTracking().OrderBy(c => c.MinWeight).ToListAsync();
        } 
             public async Task<VehicleCategory?> GetByWeightAsync(decimal weight)
        {
            return await _context.VehicleCategories.AsNoTracking().FirstOrDefaultAsync(c => weight >= c.MinWeight && (c.MaxWeight == null || weight < c.MaxWeight));
        }
    }
}