namespace VehicleManagement.ViewModels;

public class ManufacturerDetailsViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public int VehicleCount { get; init; }
    public List<VehicleListItemViewModel> Vehicles { get; init; } = new();
}
