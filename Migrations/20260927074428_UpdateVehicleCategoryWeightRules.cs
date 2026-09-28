using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehicleCategoryWeightRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeightKg",
                table: "Vehicles",
                newName: "Weight");

            migrationBuilder.RenameColumn(
                name: "Icon",
                table: "VehicleCategories",
                newName: "Size");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Weight",
                table: "Vehicles",
                newName: "WeightKg");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "VehicleCategories",
                newName: "Icon");
        }
    }
}
