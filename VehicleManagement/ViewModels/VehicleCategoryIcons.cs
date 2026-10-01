namespace VehicleManagement.ViewModels;

/// <summary>Predefined icons a category can use. Keys are what gets stored in the database.</summary>
public static class VehicleCategoryIcons
{
    public sealed record Option(string Key, string Label, string CssClass);

    public static readonly IReadOnlyList<Option> All = new[]
    {
        new Option("car-green",  "Car",   "bi-car-front-fill text-success"),
        new Option("van-yellow", "Van",   "bi-truck text-warning"),
        new Option("truck-red",  "Truck", "bi-truck-front-fill text-danger")
    };

    public static bool IsValid(string? key) => All.Any(o => o.Key == key);

    public static string GetCssClass(string? key) =>
        All.FirstOrDefault(o => o.Key == key)?.CssClass ?? "bi-car-front text-secondary";
}
