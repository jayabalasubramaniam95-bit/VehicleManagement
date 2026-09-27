using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VehicleManagement.ViewModels
{
    public class ManufacturerDetailsViewModel
    {
        public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int VehicleCount { get; set; }

    public IEnumerable<VehicleListItemViewModel> Vehicles { get; set; }
        = new List<VehicleListItemViewModel>();
    }
}