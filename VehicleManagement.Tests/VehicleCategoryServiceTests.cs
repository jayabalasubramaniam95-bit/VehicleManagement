using Moq;
using VehicleManagement.Models;
using VehicleManagement.Repositories;
using VehicleManagement.Services;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Tests;

public class VehicleCategoryServiceTests
{
    private static VehicleCategoryService CreateService(
        List<VehicleCategory> categories)
    {
        var categoryRepository = new Mock<IVehicleCategoryRepository>();
        var vehicleRepository = new Mock<IVehicleRepository>();

        categoryRepository
            .Setup(r => r.GetAll())
            .Returns(categories);

        vehicleRepository
            .Setup(r => r.GetAll())
            .Returns(new List<Vehicle>());

        return new VehicleCategoryService(
            vehicleRepository.Object,
            categoryRepository.Object);
    }

    [Fact]
    public void GetByWeight_ReturnsCategoryFromRepository()
    {
        // Arrange
        var categoryRepository = new Mock<IVehicleCategoryRepository>();
        var vehicleRepository = new Mock<IVehicleRepository>();

        var medium = new VehicleCategory
        {
            Id = 2,
            Name = "Medium",
            MinWeight = 1000m,
            MaxWeight = 2000m
        };

        categoryRepository
            .Setup(r => r.GetByWeight(1500m))
            .Returns(medium);

        var service = new VehicleCategoryService(
            vehicleRepository.Object,
            categoryRepository.Object);

        // Act
        var result = service.GetByWeight(1500m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal("Medium", result.Name);
    }

    [Fact]
    public void Create_WithValidRange_ReturnsSuccess()
    {
        // Arrange
        var categories = new List<VehicleCategory>
        {
            new()
            {
                Id = 1,
                Name = "Light",
                MinWeight = 0m,
                MaxWeight = 2000m
            },
            new()
            {
                Id = 2,
                Name = "Heavy",
                MinWeight = 2000m,
                MaxWeight = null
            }
        };

        var service = CreateService(categories);

        var model = new VehicleCategoryFormViewModel
        {
            Name = "Medium",
            MinWeight = 0m,
            MaxWeight = 1000m
        };

        // Act
        var result = service.Create(model);

        // Assert
        Assert.Equal(CategorySaveStatus.Success, result.Status);
        Assert.Equal(1000m, categories[0].MaxWeight);
    }

    [Fact]
    public void Create_WithRangeStartingAboveZero_ReturnsInvalidRange()
    {
        // Arrange
        var categories = new List<VehicleCategory>
        {
            new()
            {
                Id = 1,
                Name = "Light",
                MinWeight = 0m,
                MaxWeight = null
            }
        };

        var service = CreateService(categories);

        var model = new VehicleCategoryFormViewModel
        {
            Name = "Medium",
            MinWeight = 1000m,
            MaxWeight = 2000m
        };

        // Act
        var result = service.Create(model);

        // Assert
        Assert.Equal(CategorySaveStatus.InvalidRange, result.Status);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Create_WithOverlappingRange_ReturnsInvalidRange()
    {
        // Arrange
        var categories = new List<VehicleCategory>
        {
            new()
            {
                Id = 1,
                Name = "Light",
                MinWeight = 0m,
                MaxWeight = 2000m
            },
            new()
            {
                Id = 2,
                Name = "Heavy",
                MinWeight = 2000m,
                MaxWeight = null
            }
        };

        var service = CreateService(categories);

        var model = new VehicleCategoryFormViewModel
        {
            Name = "Medium",
            MinWeight = 1000m,
            MaxWeight = 2500m
        };

        // Act
        var result = service.Create(model);

        // Assert
        Assert.Equal(CategorySaveStatus.InvalidRange, result.Status);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Create_WithGapBetweenCategories_ReturnsInvalidRange()
    {
        // Arrange
        var categories = new List<VehicleCategory>
        {
            new()
            {
                Id = 1,
                Name = "Light",
                MinWeight = 0m,
                MaxWeight = 1000m
            },
            new()
            {
                Id = 2,
                Name = "Heavy",
                MinWeight = 2000m,
                MaxWeight = null
            }
        };

        var service = CreateService(categories);

        var model = new VehicleCategoryFormViewModel
        {
            Name = "Medium",
            MinWeight = 1200m,
            MaxWeight = 1500m
        };

        // Act
        var result = service.Create(model);

        // Assert
        Assert.Equal(CategorySaveStatus.InvalidRange, result.Status);
        Assert.NotNull(result.Error);
    }
}
