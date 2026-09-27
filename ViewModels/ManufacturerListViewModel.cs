using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace VehicleManagement.ViewModels
{
    public class ManufacturerListViewModel
{
    public IEnumerable<ManufacturerListItemViewModel> Manufacturers { get; set; }
        = new List<ManufacturerListItemViewModel>();

    public string? Search { get; set; }

    public string SortBy { get; set; } = "Name";

    public string SortDirection { get; set; } = "Ascending";

    public int CurrentPage { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
}
public class ManufacturerListItemViewModel
{
    public int Id { get; set; }

    [Display(Name = "Manufacturer")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Vehicles")]
    public int VehicleCount { get; set; }
}
}