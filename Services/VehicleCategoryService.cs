using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleManagement.Models;
using VehicleManagement.Repositories;

namespace VehicleManagement.Services
{
    public class VehicleCategoryService: IVehicleCategoryService
    {
         private readonly IVehicleCategoryRepository _categoryRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public VehicleCategoryService(
        IVehicleRepository vehicleRepository,
        IVehicleCategoryRepository categoryRepository)
    {
        _vehicleRepository = vehicleRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<List<VehicleCategory>> GetAllAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }

    public async Task<VehicleCategory?> GetByWeightAsync(decimal weight)
    {
        return await _categoryRepository.GetByWeightAsync(weight);
    }

    public async Task<Vehicle?> GetByIdAsync(int id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle is null)
        {
            return null;
        }

        vehicle.Category =
            await _categoryRepository.GetByWeightAsync(vehicle.WeightKg);

        return vehicle;
    }


    }
}