using System.ComponentModel.DataAnnotations;
namespace VehicleManagement.ViewModels
{
    public class VehicleCategoryEditViewModel
    {
         public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Minimum Weight")]
        public decimal MinWeight { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Display(Name = "Maximum Weight")]
        public decimal MaxWeight { get; set; }
    }
}