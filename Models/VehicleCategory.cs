using System.ComponentModel.DataAnnotations;

namespace VehicleManagement.Models;

public class VehicleCategory
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal MinWeight { get; set; }

    public decimal? MaxWeight { get; set; }

    [Required]
    [StringLength(500)]
    public string Icon { get; set; } = string.Empty;

    public ICollection<Vehicle> Vehicles { get; set; }
        = new List<Vehicle>();
}