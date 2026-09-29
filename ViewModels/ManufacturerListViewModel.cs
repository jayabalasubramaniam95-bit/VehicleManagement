namespace VehicleManagement.ViewModels;

public class ManufacturerListViewModel
{
    public List<ManufacturerListItemViewModel> Manufacturers { get; init; } = new();

    // Query state (echoed back so the view can rebuild sort / paging links)
    public string? Search { get; init; }
    public string SortBy { get; init; } = "name";
    public string SortDirection { get; init; } = "asc";
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalItems { get; init; }

    // Derived values: no need to store them, so they can never get out of sync
    public bool Descending => SortDirection == "desc";
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
    public int FirstItem => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int LastItem => Math.Min(CurrentPage * PageSize, TotalItems);
}

public class ManufacturerListItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int VehicleCount { get; init; }

    /// <summary>Seeded manufacturers (Mazda, Mercedes, ...) that must never be deleted.</summary>
    public bool IsDefault { get; init; }

    public bool CanDelete => !IsDefault && VehicleCount == 0;

    public string? DeleteBlockedReason =>
        IsDefault ? "Default manufacturers cannot be deleted."
        : VehicleCount > 0 ? "Manufacturers with vehicles cannot be deleted."
        : null;
}
