using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace VehicleManagement.ViewModels;

public class VehicleCategoryFormViewModel : IValidatableObject
{
    public const int NameMaxLength = 50;

    private string _name = string.Empty;

    public int Id { get; set; }

    [Required(ErrorMessage = "{0} is required.")]
    [StringLength(NameMaxLength, MinimumLength = 2,
        ErrorMessage = "{0} must be between {2} and {1} characters.")]
    [Remote(action: "IsNameAvailable", controller: "VehicleCategory",
        AdditionalFields = nameof(Id),
        ErrorMessage = "A category with this name already exists.")]
    [Display(Name = "Category Name")]
    public string Name
    {
        get => _name;
        set => _name = value?.Trim() ?? string.Empty;
    }

    [Required(ErrorMessage = "{0} is required.")]
    [Range(0, 999999.99, ErrorMessage = "{0} must be between {1} and {2}.")]
    [Display(Name = "Minimum Weight (kg)")]
    public decimal MinWeight { get; set; }

    [Range(0.01, 999999.99, ErrorMessage = "{0} must be between {1} and {2}.")]
    [Display(Name = "Maximum Weight (kg)")]
    public decimal? MaxWeight { get; set; }

    [Required(ErrorMessage = "Please choose an icon.")]
    public string? Icon { get; set; }

    public bool IsEdit => Id > 0;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (HasMoreThanTwoDecimals(MinWeight))
            yield return new ValidationResult(
                "Minimum weight can have at most 2 decimal places.", new[] { nameof(MinWeight) });

        if (MaxWeight is { } max)
        {
            if (HasMoreThanTwoDecimals(max))
                yield return new ValidationResult(
                    "Maximum weight can have at most 2 decimal places.", new[] { nameof(MaxWeight) });

            // MaxWeight is exclusive, so equal values would give an empty range
            if (max <= MinWeight)
                yield return new ValidationResult(
                    "Maximum weight must be greater than minimum weight.", new[] { nameof(MaxWeight) });
        }

        if (!string.IsNullOrWhiteSpace(Icon) && !VehicleCategoryIcons.IsValid(Icon))
            yield return new ValidationResult(
                "Please choose one of the available icons.", new[] { nameof(Icon) });
    }

    private static bool HasMoreThanTwoDecimals(decimal value) => decimal.Round(value, 2) != value;
}
