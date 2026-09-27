using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace VehicleManagement.ViewModels
{
    public class VehicleListViewModel
    {
   public IEnumerable<VehicleListItemViewModel> Vehicles { get; set; }
        = new List<VehicleListItemViewModel>();

    public string? Search { get; set; }

    public string SortBy { get; set; } = "OwnerName";

    public string SortDirection { get; set; } = "Ascending";

    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
    }

    public class VehicleListItemViewModel
{
      public int Id { get; set; }

    [Display(Name = "Owner")]
    public string OwnerName { get; set; } = string.Empty;

    [Display(Name = "Manufacturer")]
    public string ManufacturerName { get; set; } = string.Empty;

    [Display(Name = "Year")]
    public int YearOfManufacture { get; set; }

    [Display(Name = "Weight (kg)")]
    public decimal WeightKg { get; set; }

    [Display(Name = "Category")]
    public string CategoryName { get; set; } = string.Empty;
}
}