using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VehicleManagement.Data;
using VehicleManagement.Models;
using VehicleManagement.Services;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Controllers;

public class VehiclesController : Controller
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

        [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        string sortBy = "OwnerName",
        string sortDirection = "Ascending",
        int page = 1)
    {
        const int pageSize = 10;

        var model = await _vehicleService.GetVehiclesAsync(search, sortBy, sortDirection, page, pageSize);

        return View(model);
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await _vehicleService.GetCreateViewModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await ReloadDropdowns(model);
            return View(model);
        }

        var result = await _vehicleService.CreateAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            await ReloadDropdowns(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Vehicle was created successfully.";
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _vehicleService.GetDetailsAsync(id);
        return model == null ? NotFound() : View(model);
    }


    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _vehicleService.GetEditViewModelAsync(id);
        return model == null ? NotFound() : View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VehicleFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await ReloadDropdowns(model);
            return View(model);
        }

        var result = await _vehicleService.UpdateAsync(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            await ReloadDropdowns(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Vehicle was updated successfully.";
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _vehicleService.GetDeleteViewModelAsync(id);
        return model == null ? NotFound() : View(model);
    }


    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _vehicleService.DeleteAsync(id);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Vehicle was deleted successfully.";
        return RedirectToAction(nameof(Index));
    }


    private async Task ReloadDropdowns(VehicleFormViewModel model)
    {
        var refreshed = await _vehicleService.GetCreateViewModelAsync();
        model.Manufacturers = refreshed.Manufacturers;
        model.Categories = refreshed.Categories;
    }

   
}