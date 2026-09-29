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

    public VehicleCategoryListViewModel GetList() => new()
    {
        Categories = _categoryRepository.GetSummaries()
            .Select(s => new VehicleCategoryItemViewModel
            {
                Id = s.Id,
                Name = s.Name,
                Icon = s.Icon,
                MinWeight = s.MinWeight,
                MaxWeight = s.MaxWeight,
                VehicleCount = s.VehicleCount
            })
            .ToList()
    };

    public VehicleCategoryFormViewModel? GetForEdit(int id)
    {
        var category = _categoryRepository.GetById(id);

        return category is null
            ? null
            : new VehicleCategoryFormViewModel
            {
                Id = category.Id,
                Name = category.Name,
                MinWeight = category.MinWeight,
                MaxWeight = category.MaxWeight,
                Icon = category.Icon
            };
    }

    public bool NameExists(string name, int? excludeId = null) =>
        _categoryRepository.NameExists(name, excludeId);

    #endregion

    #region Create

    public CategorySaveResult Create(VehicleCategoryFormViewModel model)
    {
        var categories = _categoryRepository.GetAll();

        var candidate = new VehicleCategory
        {
            Name = model.Name,
            Icon = model.Icon,
            MinWeight = model.MinWeight,
            MaxWeight = model.MaxWeight,
            UpdatedAt = DateTime.UtcNow
        };

        // The ranges already cover 0 -> infinity, so a new category has to take over part of one
        var error = TakeRangeFromExisting(categories, candidate);
        if (error is not null) return CategorySaveResult.InvalidRange(error);

        categories.Add(candidate);

        error = ValidateRanges(categories);
        if (error is not null) return CategorySaveResult.InvalidRange(error);

        _categoryRepository.InTransaction(() =>
        {
            _categoryRepository.Add(candidate);
            _categoryRepository.SaveChanges();      // candidate.Id is known after this

            RecategoriseVehicles(categories);
            _categoryRepository.SaveChanges();
        });

        return CategorySaveResult.Success();
    }

    #endregion

    #region Update

    public CategorySaveResult Update(VehicleCategoryFormViewModel model)
    {
        var categories = _categoryRepository.GetAll();
        var category = categories.FirstOrDefault(c => c.Id == model.Id);
        if (category is null) return CategorySaveResult.NotFound();

        var oldMin = category.MinWeight;
        var oldMax = category.MaxWeight;
        var now = DateTime.UtcNow;

        category.Name = model.Name;
        category.Icon = model.Icon;
        category.MinWeight = model.MinWeight;
        category.MaxWeight = model.MaxWeight;
        category.UpdatedAt = now;

        var previous = categories.FirstOrDefault(c => c.Id != category.Id && c.MaxWeight == oldMin);
        if (previous is not null && model.MinWeight != oldMin)
        {
            previous.MaxWeight = model.MinWeight;
            previous.UpdatedAt = now;
        }

        var next = oldMax is null
            ? null
            : categories.FirstOrDefault(c => c.Id != category.Id && c.MinWeight == oldMax);
        if (next is not null && model.MaxWeight is { } newMax && newMax != oldMax)
        {
            next.MinWeight = newMax;
            next.UpdatedAt = now;
        }

        var error = ValidateRanges(categories);
        if (error is not null) return CategorySaveResult.InvalidRange(error);

        // Entities are tracked, so this is one SaveChanges (one transaction) for categories and vehicles
        RecategoriseVehicles(categories);
        _categoryRepository.SaveChanges();

        return CategorySaveResult.Success();
    }

    #endregion

    #region Delete

    public CategoryDeleteResult Delete(int id)
    {
        var categories = _categoryRepository.GetAll();
        var category = categories.FirstOrDefault(c => c.Id == id);

        if (category is null) return CategoryDeleteResult.NotFound;
        if (categories.Count == 1) return CategoryDeleteResult.LastCategory;
        if (_categoryRepository.HasVehicles(id)) return CategoryDeleteResult.HasVehicles;

        // Hand the range to a neighbour so no gap is left behind
        var previous = categories.FirstOrDefault(c => c.MaxWeight == category.MinWeight);
        if (previous is not null)
        {
            previous.MaxWeight = category.MaxWeight;
            previous.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var next = categories.FirstOrDefault(c => c.MinWeight == category.MaxWeight);
            if (next is not null)
            {
                next.MinWeight = category.MinWeight;
                next.UpdatedAt = DateTime.UtcNow;
            }
        }

        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;
        _categoryRepository.Update(category);
        _categoryRepository.SaveChanges();
        return CategoryDeleteResult.Deleted;
    }

    #endregion

    #region Helpers

    private void RecategoriseVehicles(IReadOnlyCollection<VehicleCategory> categories)
    {
        foreach (var vehicle in _vehicleRepository.GetAll())
        {
            var match = FindCategory(categories, vehicle.Weight);

            if (match is null || vehicle.CategoryId == match.Id)
            {
                continue;
            }

            vehicle.CategoryId = match.Id;
            vehicle.UpdatedAt = DateTime.UtcNow;
            _vehicleRepository.Update(vehicle);
        }
    }

    private static VehicleCategory? FindCategory(IEnumerable<VehicleCategory> categories, decimal weight) =>
        categories.FirstOrDefault(c => weight >= c.MinWeight && weight < UpperBound(c));

    private static decimal UpperBound(VehicleCategory c) => c.MaxWeight ?? decimal.MaxValue;

    private static string Describe(VehicleCategory c) => c.MaxWeight is { } max
        ? $"{c.MinWeight:0.##}–{max:0.##} kg"
        : $"{c.MinWeight:0.##} kg and above";

    /// <summary>
    /// Shrinks the existing category that contains the new range. Returns an error message,
    /// or null when the new range could be carved out cleanly.
    /// </summary>
    private static string? TakeRangeFromExisting(IEnumerable<VehicleCategory> categories, VehicleCategory candidate)
    {
        
        var host = categories.FirstOrDefault(c =>
            candidate.MinWeight >= c.MinWeight && UpperBound(candidate) <= UpperBound(c));

        if (host is null)
            return "This range overlaps existing categories or falls outside them. " +
                   "A new category must take over part of one existing range.";

        var sameMin = candidate.MinWeight == host.MinWeight;
        var sameMax = UpperBound(candidate) == UpperBound(host);

        if (sameMin && sameMax)
            return $"'{host.Name}' already covers exactly this range ({Describe(host)}).";

        if (sameMin)
            host.MinWeight = candidate.MaxWeight!.Value;   // new category takes the bottom slice
        else if (sameMax)
            host.MaxWeight = candidate.MinWeight;          // new category takes the top slice
        else
            return $"'{host.Name}' ({Describe(host)}) would be split in two. " +
                   "Start or end the new range at the same boundary as that category.";

        host.UpdatedAt = DateTime.UtcNow;
        return null;
    }

    /// <summary>Checks the complete set of ranges. Returns the first problem found, or null if valid.</summary>
    private static string? ValidateRanges(IEnumerable<VehicleCategory> categories)
    {
        var sorted = categories.OrderBy(c => c.MinWeight).ToList();

        if (sorted[0].MinWeight != 0)
            return $"The lowest category must start at 0 kg ('{sorted[0].Name}' starts at {sorted[0].MinWeight:0.##} kg).";

        for (var i = 0; i < sorted.Count; i++)
        {
            var current = sorted[i];
            var isLast = i == sorted.Count - 1;

            if (current.MaxWeight is not { } max)
            {
                if (!isLast)
                    return $"Only the highest category can have no upper limit ('{current.Name}').";
                continue;
            }

            if (isLast)
                return $"The highest category ('{current.Name}') must have no upper limit. Leave Maximum Weight empty.";

            if (max <= current.MinWeight)
                return $"'{current.Name}' would have an empty range ({Describe(current)}). Adjust the weights.";

            var next = sorted[i + 1];

            if (max < next.MinWeight)
                return $"Gap between {max:0.##}–{next.MinWeight:0.##} kg is not allowed.";

            if (max > next.MinWeight)
                return $"Overlap between {next.MinWeight:0.##}–{max:0.##} kg is not allowed.";
        }

        return null;
    }

    #endregion
}
