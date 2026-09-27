using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VehicleManagement.ViewModels
{
    public class VehicleEditViewModel
    {
          public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Owner")]
    public string OwnerName { get; set; } = string.Empty;


    [Required]
    [Display(Name = "Manufacturer")]
    public int ManufacturerId { get; set; }


    [Required]
    [Range(1886, 2100)]
    [Display(Name = "Year of Manufacture")]
    public int YearOfManufacture { get; set; }


    [Required]
    [Range(0.01, double.MaxValue)]
    [Display(Name = "Weight (kg)")]
    public decimal WeightKg { get; set; }


    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }


    public IEnumerable<SelectListItem> Manufacturers { get; set; }
        = new List<SelectListItem>();


    public IEnumerable<SelectListItem> Categories { get; set; }
        = new List<SelectListItem>();

    }
}