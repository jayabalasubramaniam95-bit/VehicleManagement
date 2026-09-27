using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace VehicleManagement.ViewModels
{
    public class ManufacturerCreateViewModel
    {
        [Required]
    [StringLength(100)]
    [Display(Name = "Manufacturer Name")]
    public string Name { get; set; } = string.Empty;
    }
}