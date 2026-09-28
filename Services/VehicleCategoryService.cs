using VehicleManagement.Models;
using VehicleManagement.Repositories;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Services;

public class VehicleCategoryService : IVehicleCategoryService
{
    #region Dependencies & Constructor

    private readonly IVehicleRepository _vehicleRepository;
    private readonly IVehicleCategoryRepository _categoryRepository;

    public VehicleCategoryService(
        IVehicleRepository vehicleRepository,
        IVehicleCategoryRepository categoryRepository)
    {
        _vehicleRepository = vehicleRepository;
        _categoryRepository = categoryRepository;
    }

    #endregion

    #region Queries

    public VehicleCategory? GetByWeight(decimal weight) =>
        _categoryRepository.GetByWeight(weight);

    public Vehicle? GetById(int id)
    {
        var vehicle = _vehicleRepository.GetById(id);

        if (vehicle is null)
        {
            return null;
        }

        vehicle.Category = _categoryRepository.GetByWeight(vehicle.Weight);

        return vehicle;
    }

    public VehicleCategoryListViewModel GetList()
    {
        var categories = _categoryRepository
            .GetAll()
            .Select(category => new VehicleCategoryItemViewModel
            {
                Id = category.Id,
                Name = category.Name,
                MinWeight = category.MinWeight,
                MaxWeight = category.MaxWeight,
                IsUsedByVehicles = _categoryRepository.HasVehicles(category.Id)
            })
            .ToList();

        return new VehicleCategoryListViewModel { Categories = categories };
    }

    #endregion

    #region Update

    public void Update(VehicleCategory category)
    {
        if (category.MinWeight > category.MaxWeight)
        {
            throw new InvalidOperationException(
                "Minimum weight cannot be greater than maximum weight.");
        }

        _categoryRepository.Update(category);

        var categories = _categoryRepository
            .GetCategoriesExcept(category.Id)
            .Append(category)
            .ToList();

        foreach (var vehicle in _vehicleRepository.GetAll())
        {
            var match = FindCategory(categories, vehicle.Weight);

            if (match is null || vehicle.CategoryId == match.Id)
            {
                continue;
            }

            vehicle.CategoryId = match.Id;
            _vehicleRepository.Update(vehicle);
        }

        _categoryRepository.SaveChanges();
    }

    #endregion

    #region Delete

    public bool Delete(int id)
    {
        var category = _categoryRepository.GetById(id);

        if (category is null || _categoryRepository.HasVehicles(id))
        {
            return false;
        }

        _categoryRepository.Delete(category);
        _categoryRepository.SaveChanges();

        return true;
    }

    #endregion

    #region Helpers

    private static VehicleCategory? FindCategory(
        IEnumerable<VehicleCategory> categories,
        decimal weight) =>
        categories.FirstOrDefault(c =>
            weight >= c.MinWeight &&
            (c.MaxWeight == null || weight < c.MaxWeight));

    #endregion
}