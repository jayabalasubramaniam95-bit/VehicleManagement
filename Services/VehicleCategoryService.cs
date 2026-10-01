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

    public VehicleCategory? GetVehicleCategoryByWeight(decimal weight) =>
        _categoryRepository.GetVehicleCategoryByWeight(weight);

    public VehicleCategoryListViewModel GetVehicleCategoryList() => new()
    {
        Categories = _categoryRepository.GetVehicleCategorySummaries()
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

    public VehicleCategoryFormViewModel? GetVehicleCategoryForEdit(int id)
    {
        var category = _categoryRepository.GetVehicleCategoryById(id);
        if (category is null) return null;

        return new VehicleCategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            MinWeight = category.MinWeight,
            MaxWeight = category.MaxWeight
        };
    }

    public bool IsVehicleCategoryNameExists(string name, int? excludeId = null) =>
        _categoryRepository.IsVehicleCategoryNameExists(name, excludeId);

    #endregion

    #region Create

    public CategorySaveResult Create(VehicleCategoryFormViewModel model)
    {
        var categories = _categoryRepository.GetAllVehicleCategory();

        var candidate = new VehicleCategory
        {
            Name = model.Name,
            Icon = model.Icon,
            MinWeight = model.MinWeight,
            MaxWeight = model.MaxWeight,
            UpdatedAt = DateTime.UtcNow
        };

        // Existing ranges already cover 0 -> infinity, so a new category must take over part of one.
        var error = TakeRangeFromExisting(categories, candidate);
        if (error is not null) return CategorySaveResult.InvalidRange(error);

        categories.Add(candidate);

        error = ValidateRanges(categories);
        if (error is not null) return CategorySaveResult.InvalidRange(error);

        _categoryRepository.InTransaction(() =>
        {
            _categoryRepository.Add(candidate);
            RecategoriseVehicles(categories);
        });

        return CategorySaveResult.Success();
    }

    #endregion

    #region Update

    public CategorySaveResult Update(VehicleCategoryFormViewModel model)
    {
        var categories = _categoryRepository.GetAllVehicleCategory();
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

        // Keep neighbours contiguous when a boundary moves.
        if (model.MinWeight != oldMin)
        {
            var previous = categories.FirstOrDefault(c => c.Id != category.Id && c.MaxWeight == oldMin);
            if (previous is not null)
            {
                previous.MaxWeight = model.MinWeight;
                previous.UpdatedAt = now;
            }
        }

        if (oldMax is not null && model.MaxWeight is { } newMax && newMax != oldMax)
        {
            var next = categories.FirstOrDefault(c => c.Id != category.Id && c.MinWeight == oldMax);
            if (next is not null)
            {
                next.MinWeight = newMax;
                next.UpdatedAt = now;
            }
        }

        var error = ValidateRanges(categories);
        if (error is not null) return CategorySaveResult.InvalidRange(error);

        // Entities are tracked, so one SaveChanges (one transaction) covers categories and vehicles.
        RecategoriseVehicles(categories);
        return CategorySaveResult.Success();
    }

    #endregion

    #region Delete

    public CategoryDeleteResult Delete(int id)
    {
        var categories = _categoryRepository.GetAllVehicleCategory();
        var category = categories.FirstOrDefault(c => c.Id == id);

        if (category is null) return CategoryDeleteResult.NotFound;
        if (categories.Count == 1) return CategoryDeleteResult.LastCategory;
        if (_categoryRepository.HasVehicles(id)) return CategoryDeleteResult.HasVehicles;

        var now = DateTime.UtcNow;

        // Hand the range to a neighbour so no gap is left behind.
        var previous = categories.FirstOrDefault(c => c.MaxWeight == category.MinWeight);
        if (previous is not null)
        {
            previous.MaxWeight = category.MaxWeight;
            previous.UpdatedAt = now;
        }
        else
        {
            var next = categories.FirstOrDefault(c => c.MinWeight == category.MaxWeight);
            if (next is not null)
            {
                next.MinWeight = category.MinWeight;
                next.UpdatedAt = now;
            }
        }
        category.IsDeleted = true;
        category.UpdatedAt = now;
        _categoryRepository.Update(category);
        return CategoryDeleteResult.Deleted;
    }

    #endregion

    #region Helpers

    private void RecategoriseVehicles(IReadOnlyCollection<VehicleCategory> categories)
    {
        var now = DateTime.UtcNow;

        foreach (var vehicle in _vehicleRepository.GetAllVehicles())
        {
            var match = FindCategory(categories, vehicle.Weight);
            if (match is null || vehicle.CategoryId == match.Id) continue;

            vehicle.CategoryId = match.Id;
            vehicle.UpdatedAt = now;
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
    /// Makes room for <paramref name="candidate"/> by shrinking an existing category.
    /// Returns an error message, or null on success.
    /// </summary>
    private static string? TakeRangeFromExisting(
        IEnumerable<VehicleCategory> categories,
        VehicleCategory candidate)
    {
        if (candidate.MaxWeight is { } candidateMax && candidateMax <= candidate.MinWeight)
        {
            return $"'{candidate.Name}' must have a maximum weight greater than its minimum weight.";
        }

        var sorted = categories.OrderBy(c => c.MinWeight).ToList();

        // Case 1: continues directly after the highest category
        //  (e.g. 2500–3000 -> 3000–5000).
        var highest = sorted.LastOrDefault();
        if (highest?.MaxWeight is { } highestMax && candidate.MinWeight == highestMax)
        {
            return null;
        }

        // Case 2: sits inside an existing category
        //  (e.g. 2500–∞ -> 2500–3000).
        var host = sorted.FirstOrDefault(c =>
            candidate.MinWeight >= c.MinWeight &&
            UpperBound(candidate) <= UpperBound(c));

        if (host is null)
        {
            return "The new range must either fit inside an existing category " +
                   "or start exactly where the current highest category ends.";
        }

        var sameMin = candidate.MinWeight == host.MinWeight;
        var sameMax = UpperBound(candidate) == UpperBound(host);

        if (sameMin && sameMax)
        {
            return $"'{host.Name}' already covers exactly this range ({Describe(host)}).";
        }

        if (sameMin)
        {
            host.MinWeight = candidate.MaxWeight!.Value; // candidate takes the bottom part
        }
        else if (sameMax)
        {
            host.MaxWeight = candidate.MinWeight; // candidate takes the top part
        }
        else
        {
            return $"'{host.Name}' ({Describe(host)}) would be split in two. " +
                   "Start or end the new range at the same boundary as that category.";
        }

        host.UpdatedAt = DateTime.UtcNow;
        return null;
    }

    /// <summary>
    /// Ensures the ranges start at 0, are valid, and connect with no gaps or overlaps.
    /// Returns an error message, or null when valid.
    /// </summary>
    private static string? ValidateRanges(IEnumerable<VehicleCategory> categories)
    {
        var sorted = categories.OrderBy(c => c.MinWeight).ToList();

        if (sorted.Count == 0)
            return "At least one vehicle category is required.";

        if (sorted[0].MinWeight != 0)
        {
            return "The lowest category must start at 0 kg " +
                   $"('{sorted[0].Name}' starts at {sorted[0].MinWeight:0.##} kg).";
        }

        for (var i = 0; i < sorted.Count; i++)
        {
            var current = sorted[i];
            var isLast = i == sorted.Count - 1;

            if (current.MaxWeight is { } max && max <= current.MinWeight)
            {
                return $"'{current.Name}' would have an empty or invalid range " +
                       $"({Describe(current)}). Adjust the weights.";
            }

            // The last category may be bounded (2500–3000) or open-ended (2500–∞).
            if (isLast) break;

            var next = sorted[i + 1];

            if (current.MaxWeight is not { } currentMax)
            {
                return $"'{current.Name}' has no upper limit, but another " +
                       $"category ('{next.Name}') exists after it.";
            }

            if (currentMax < next.MinWeight)
                return $"Gap between {currentMax:0.##}–{next.MinWeight:0.##} kg is not allowed.";

            if (currentMax > next.MinWeight)
                return $"Overlap between {next.MinWeight:0.##}–{currentMax:0.##} kg is not allowed.";
        }

        return null;
    }

    #endregion
}