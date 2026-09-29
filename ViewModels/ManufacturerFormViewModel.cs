using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace VehicleManagement.ViewModels;

/// <summary>
/// Single view model shared by the Create and Edit actions.
/// Id == 0 means "new manufacturer".
/// </summary>
public class ManufacturerFormViewModel
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;

    private string _name = string.Empty;

    public int Id { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(NameMaxLength, MinimumLength = NameMinLength,
        ErrorMessage = "{0} must be between {2} and {1} characters.")]
    [Remote(action: "IsNameAvailable", controller: "Manufacturers",
        AdditionalFields = nameof(Id),
        ErrorMessage = "A manufacturer with this name already exists.")]
    [Display(Name = "Manufacturer Name")]
    public string Name
    {
        get => _name;
        // Trim on the server so "  Toyota " and "Toyota" are never treated as different values.
        set => _name = value?.Trim() ?? string.Empty;
    }

    public bool IsEdit => Id > 0;
}
