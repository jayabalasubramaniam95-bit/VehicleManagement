using Microsoft.AspNetCore.Mvc;
using VehicleManagement.Services;

namespace VehicleManagement.Controllers;

public class VehicleCategoryController : Controller
{
    private readonly IVehicleCategoryService _categoryService;

    public VehicleCategoryController(
        IVehicleCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();

        return View(categories);
    }
}