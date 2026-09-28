using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehicleManagement.Models;

public class Vehicle
{
     public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string OwnerName { get; set; } = string.Empty;

    [Required]
    public int ManufacturerId { get; set; }

    public Manufacturer Manufacturer { get; set; } = null!;

    [Required]
    [Range(1886, 2100)]
    public int YearOfManufacture { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Weight { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public VehicleCategory Category { get; set; } = null!;
}