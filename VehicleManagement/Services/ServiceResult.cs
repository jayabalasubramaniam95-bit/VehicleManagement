namespace VehicleManagement.Services;

/// <summary>Outcome of a command: success, or a user-friendly error message.</summary>
public sealed record ServiceResult(bool Success, string? ErrorMessage = null)
{
    public static ServiceResult Ok() => new(true);
    public static ServiceResult Fail(string message) => new(false, message);
}
