using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VehicleManagement.Data;
using VehicleManagement.Models;
using VehicleManagement.Services;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Tests.Integration;

public class VehicleIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task Vehicles_Index_Returns_Success()
    {
        // Arrange
        await using var factory = new VehicleApiFactory();

        await TestDatabase.InitializeAsync(factory.Services);

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Vehicles/Index");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Vehicles_Index_Contains_Vehicles_Page()
    {
        // Arrange
        await using var factory = new VehicleApiFactory();

        await TestDatabase.InitializeAsync(factory.Services);

        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Vehicles/Index");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("Vehicles", content);
    }

    [Fact]
    public async Task CreateVehicle_WithMediumWeight_AssignsMediumCategory()
    {
        // Arrange

        var service = Scope.ServiceProvider
            .GetRequiredService<IVehicleService>();

        var dbContext = Scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var manufacturer = await dbContext.Manufacturers
            .FirstAsync(m => !m.IsDeleted);

        var mediumCategory = await dbContext.VehicleCategories
            .FirstAsync(c =>
                !c.IsDeleted &&
                c.Name == "Medium");

        var model = new VehicleFormViewModel
        {
            OwnerName = "Integration Test Vehicle",
            ManufacturerId = manufacturer.Id,
            YearOfManufacture = 2020,
            Weight = 1000
        };

        // Act
        var result = service.Create(model);

        // Assert
        Assert.True(result.Success);

        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .FirstAsync(v => v.OwnerName == "Integration Test Vehicle");

        Assert.Equal(mediumCategory.Id, vehicle.CategoryId);
    }

    [Fact]
    public async Task CreateVehicle_WithLightWeight_AssignsLightCategory()
    {
        // Arrange

        var service = Scope.ServiceProvider
            .GetRequiredService<IVehicleService>();

        var dbContext = Scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var manufacturer = await dbContext.Manufacturers
            .FirstAsync(m => !m.IsDeleted);

        var lightCategory = await dbContext.VehicleCategories
            .FirstAsync(c => !c.IsDeleted && c.Name == "Light");

        var model = new VehicleFormViewModel
        {
            OwnerName = "Light Integration Test",
            ManufacturerId = manufacturer.Id,
            YearOfManufacture = 2020,
            Weight = 100
        };

        // Act
        var result = service.Create(model);

        // Assert
        Assert.True(result.Success);

        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .FirstAsync(v => v.OwnerName == "Light Integration Test");

        Assert.Equal(lightCategory.Id, vehicle.CategoryId);
    }
    [Fact]
    public async Task CreateVehicle_WithHeavyWeight_AssignsHeavyCategory()
    {
        // Arrange

        var service = Scope.ServiceProvider
            .GetRequiredService<IVehicleService>();

        var dbContext = Scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var manufacturer = await dbContext.Manufacturers
            .FirstAsync(m => !m.IsDeleted);

        var heavyCategory = await dbContext.VehicleCategories
            .FirstAsync(c => !c.IsDeleted && c.Name == "Heavy");

        var model = new VehicleFormViewModel
        {
            OwnerName = "Heavy Integration Test",
            ManufacturerId = manufacturer.Id,
            YearOfManufacture = 2020,
            Weight = 3000
        };

        // Act
        var result = service.Create(model);

        // Assert
        Assert.True(result.Success);

        var vehicle = await dbContext.Vehicles
            .AsNoTracking()
            .FirstAsync(v => v.OwnerName == "Heavy Integration Test");

        Assert.Equal(heavyCategory.Id, vehicle.CategoryId);
    }

    [Fact]
    public async Task UpdateVehicle_WhenWeightChanges_RecalculatesCategory()
    {
        // Arrange       

        var service = Scope.ServiceProvider
            .GetRequiredService<IVehicleService>();

        var dbContext = Scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var manufacturer = await dbContext.Manufacturers
            .FirstAsync(m => !m.IsDeleted);

        var mediumCategory = await dbContext.VehicleCategories
            .FirstAsync(c => !c.IsDeleted && c.Name == "Medium");

        var heavyCategory = await dbContext.VehicleCategories
            .FirstAsync(c => !c.IsDeleted && c.Name == "Heavy");

        // Create a vehicle in the Medium range.
        var createModel = new VehicleFormViewModel
        {
            OwnerName = "Category Update Integration Test",
            ManufacturerId = manufacturer.Id,
            YearOfManufacture = 2020,
            Weight = 1000
        };

        var createResult = service.Create(createModel);

        Assert.True(createResult.Success);

        var vehicle = await dbContext.Vehicles
            .FirstAsync(v => v.OwnerName == "Category Update Integration Test");

        Assert.Equal(mediumCategory.Id, vehicle.CategoryId);

        // Act - change the weight from Medium to Heavy.
        var updateModel = new VehicleFormViewModel
        {
            Id = vehicle.Id,
            OwnerName = vehicle.OwnerName,
            ManufacturerId = manufacturer.Id,
            YearOfManufacture = vehicle.YearOfManufacture,
            Weight = 3000
        };

        var updateResult = service.Update(updateModel);

        // Assert
        Assert.True(updateResult.Success);

        var updatedVehicle = await dbContext.Vehicles
            .AsNoTracking()
            .FirstAsync(v => v.Id == vehicle.Id);

        Assert.Equal(3000, updatedVehicle.Weight);
        Assert.Equal(heavyCategory.Id, updatedVehicle.CategoryId);
    }
    [Fact]
    public async Task CreateVehicle_WithInvalidManufacturer_Fails()
    {
        // Arrange
        
        var service = Scope.ServiceProvider
            .GetRequiredService<IVehicleService>();

        var dbContext = Scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var model = new VehicleFormViewModel
        {
            OwnerName = "Invalid Manufacturer Test",
            ManufacturerId = 999999,
            YearOfManufacture = 2020,
            Weight = 1000
        };

        // Act
        var result = service.Create(model);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(
            "The selected manufacturer does not exist.",
            result.ErrorMessage);

        var vehicleExists = await dbContext.Vehicles
            .AnyAsync(v => v.OwnerName == "Invalid Manufacturer Test");

        Assert.False(vehicleExists);
    }

    [Fact]
    public async Task CreateVehicle_WhenWeightIsOutsideAllCategories_Fails()
    {
        // Arrange        
        var service = Scope.ServiceProvider
            .GetRequiredService<IVehicleService>();

        var dbContext = Scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var manufacturer = await dbContext.Manufacturers
            .FirstAsync(m => !m.IsDeleted);

        // Use a weight that is outside the configured category ranges.
        var model = new VehicleFormViewModel
        {
            OwnerName = "Invalid Weight Test",
            ManufacturerId = manufacturer.Id,
            YearOfManufacture = 2020,
            Weight = -10
        };

        // Act
        var result = service.Create(model);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(
            "No vehicle category covers this weight. Check the category ranges.",
            result.ErrorMessage);

        var vehicleExists = await dbContext.Vehicles
            .AnyAsync(v => v.OwnerName == "Invalid Weight Test");

        Assert.False(vehicleExists);
    }

    [Fact]
public async Task UpdateVehicle_WhenVehicleDoesNotExist_Fails()
{
    // Arrange
    
    var service = Scope.ServiceProvider
        .GetRequiredService<IVehicleService>();

    var dbContext = Scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var manufacturer = await dbContext.Manufacturers
        .FirstAsync(m => !m.IsDeleted);

    var model = new VehicleFormViewModel
    {
        Id = 999999,
        OwnerName = "Non Existing Vehicle",
        ManufacturerId = manufacturer.Id,
        YearOfManufacture = 2020,
        Weight = 1000
    };

    // Act
    var result = service.Update(model);

    // Assert
    Assert.False(result.Success);
    Assert.Equal(
        "Vehicle could not be found.",
        result.ErrorMessage);

    var vehicleExists = await dbContext.Vehicles
        .AnyAsync(v => v.Id == 999999);

    Assert.False(vehicleExists);
}
[Fact]
public async Task DeleteVehicle_WhenVehicleDoesNotExist_Fails()
{
    // Arrange
    
    var service = Scope.ServiceProvider
        .GetRequiredService<IVehicleService>();

    var dbContext = Scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    const int nonExistingVehicleId = 999999;

    // Act
    var result = service.Delete(nonExistingVehicleId);

    // Assert
    Assert.False(result.Success);
    Assert.Equal(
        "Vehicle could not be found.",
        result.ErrorMessage);

    var vehicleExists = await dbContext.Vehicles
        .AnyAsync(v => v.Id == nonExistingVehicleId);

    Assert.False(vehicleExists);
}
[Fact]
public async Task DeleteVehicle_SetsIsDeletedTrue()
{
    // Arrange
        var service = Scope.ServiceProvider
        .GetRequiredService<IVehicleService>();

    var dbContext = Scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var manufacturer = await dbContext.Manufacturers
        .FirstAsync(m => !m.IsDeleted);

    var vehicle = new Vehicle
    {
        OwnerName = "Soft Delete Integration Test",
        ManufacturerId = manufacturer.Id,
        YearOfManufacture = 2020,
        Weight = 1000,
        CategoryId = await dbContext.VehicleCategories
            .Where(c => !c.IsDeleted && c.Name == "Medium")
            .Select(c => c.Id)
            .FirstAsync(),
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    dbContext.Vehicles.Add(vehicle);
    await dbContext.SaveChangesAsync();

    var vehicleId = vehicle.Id;

    // Act
    var result = service.Delete(vehicleId);

    // Assert
    Assert.True(result.Success);

    var deletedVehicle = await dbContext.Vehicles
        .AsNoTracking()
        .FirstAsync(v => v.Id == vehicleId);

    Assert.True(deletedVehicle.IsDeleted);
}
[Fact]
public async Task CreateVehicle_AtCategoryBoundaries_AssignsCorrectCategory()
{
    // Arrange
    var service = Scope.ServiceProvider
        .GetRequiredService<IVehicleService>();

    var dbContext = Scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    var manufacturer = await dbContext.Manufacturers
        .FirstAsync(m => !m.IsDeleted);

    var lightCategory = await dbContext.VehicleCategories
        .FirstAsync(c => !c.IsDeleted && c.Name == "Light");

    var mediumCategory = await dbContext.VehicleCategories
        .FirstAsync(c => !c.IsDeleted && c.Name == "Medium");

    var heavyCategory = await dbContext.VehicleCategories
        .FirstAsync(c => !c.IsDeleted && c.Name == "Heavy");

    // Act & Assert - 500 belongs to Medium.
    var mediumModel = new VehicleFormViewModel
    {
        OwnerName = "Boundary Medium Test",
        ManufacturerId = manufacturer.Id,
        YearOfManufacture = 2020,
        Weight = 500
    };

    var mediumResult = service.Create(mediumModel);

    Assert.True(mediumResult.Success);

    var mediumVehicle = await dbContext.Vehicles
        .AsNoTracking()
        .FirstAsync(v => v.OwnerName == "Boundary Medium Test");

    Assert.Equal(mediumCategory.Id, mediumVehicle.CategoryId);

    // Act & Assert - 2500 belongs to Heavy.
    var heavyModel = new VehicleFormViewModel
    {
        OwnerName = "Boundary Heavy Test",
        ManufacturerId = manufacturer.Id,
        YearOfManufacture = 2020,
        Weight = 2500
    };

    var heavyResult = service.Create(heavyModel);

    Assert.True(heavyResult.Success);

    var heavyVehicle = await dbContext.Vehicles
        .AsNoTracking()
        .FirstAsync(v => v.OwnerName == "Boundary Heavy Test");

    Assert.Equal(heavyCategory.Id, heavyVehicle.CategoryId);
}
}