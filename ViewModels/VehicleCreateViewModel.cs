using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VehicleManagement.ViewModels;

public class VehicleCreateViewModel
{
    [Required(ErrorMessage = "Owner name is required.")]
    [StringLength(100, ErrorMessage = "Owner name cannot exceed 100 characters.")]
    [Display(Name = "Owner Name")]
    public string OwnerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a manufacturer.")]
    [Display(Name = "Manufacturer")]
    public int? ManufacturerId { get; set; }

    [Required(ErrorMessage = "Year of manufacture is required.")]
    [Range(1886, 2100, ErrorMessage = "Year must be between 1886 and 2100.")]
    [Display(Name = "Year of Manufacture")]
    public int? YearOfManufacture { get; set; }

    [Required(ErrorMessage = "Weight is required.")]
    [Range(0.01, 1000000, ErrorMessage = "Weight must be greater than 0.")]
    [Display(Name = "Weight (kg)")]
    public decimal? Weight { get; set; }

    public IEnumerable<SelectListItem> Manufacturers { get; set; }
        = new List<SelectListItem>();
}
