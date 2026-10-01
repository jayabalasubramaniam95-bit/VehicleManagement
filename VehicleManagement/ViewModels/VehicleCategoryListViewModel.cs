using System.Globalization;

namespace VehicleManagement.ViewModels;

public class VehicleCategoryListViewModel
{
    public List<VehicleCategoryItemViewModel> Categories { get; init; } = new();
}

public class VehicleCategoryItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Icon { get; init; }
    public decimal MinWeight { get; init; }
    public decimal? MaxWeight { get; init; }
    public int VehicleCount { get; init; }

    public bool IsUsedByVehicles => VehicleCount > 0;

    public string WeightRange => MaxWeight is { } max
        ? $"{MinWeight:0.##} – {max:0.##} kg"
        : $"{MinWeight:0.##} kg and above";

    /// <summary>Culture-neutral number so the browser can sort the Weight Range column numerically.</summary>
    public string MinWeightSortKey => MinWeight.ToString(CultureInfo.InvariantCulture);
}
