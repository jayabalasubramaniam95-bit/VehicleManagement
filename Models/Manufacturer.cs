using System.ComponentModel.DataAnnotations;

namespace VehicleManagement.Models;

public class Manufacturer
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }    

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public ICollection<Vehicle> Vehicles { get; set; }
        = new List<Vehicle>();
}