namespace VehicleManagement.ViewModels
{
     public class VehicleCategoryListViewModel
    {
        public List<VehicleCategoryItemViewModel> Categories { get; set; } = new();
    }

    public class VehicleCategoryItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal MinWeight { get; set; }

        public decimal? MaxWeight { get; set; }


        public string WeightRange
        {
            get
            {
                if (MaxWeight.HasValue)
                {
                    return $"{MinWeight:0.##} – {MaxWeight.Value:0.##} kg";
                }

                return $"{MinWeight:0.##} kg and above";
            }
        }

        public bool IsUsedByVehicles { get; set; }
    }
}