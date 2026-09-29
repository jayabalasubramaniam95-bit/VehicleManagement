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
    public string Size { get; set; } = string.Empty;

    public ICollection<Vehicle> Vehicles { get; set; }
        = new List<Vehicle>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
}