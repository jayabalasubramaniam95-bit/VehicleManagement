using System.ComponentModel.DataAnnotations;

namespace VehicleManagement.Models;

public class Manufacturer
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Vehicle> Vehicles { get; set; }
        = new List<Vehicle>();
}