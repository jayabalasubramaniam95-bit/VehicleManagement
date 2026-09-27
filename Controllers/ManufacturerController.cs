using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VehicleManagement.Services;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Controllers
{
    public class ManufacturerController : Controller
    {
         private readonly IManufacturerService _service;

    public ManufacturerController(
        IManufacturerService service)
    {
        _service = service;
    }


    // GET: /Manufacturers

    public async Task<IActionResult> Index(
        string? search,
        string sortBy = "Name",
        string sortDirection = "Ascending",
        int page = 1)
    {
        const int pageSize = 10;

        var model =
            await _service.GetManufacturersAsync(
                search,
                sortBy,
                sortDirection,
                page,
                pageSize);

        return View(model);
    }


    // GET: /Manufacturers/Create

    public IActionResult Create()
    {
        return View();
    }


    // POST: /Manufacturers/Create

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ManufacturerCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }


        var created =
            await _service.CreateAsync(model);

        if (!created)
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "A manufacturer with this name already exists.");

            return View(model);
        }


        TempData["SuccessMessage"] =
            "Manufacturer created successfully.";

        return RedirectToAction(nameof(Index));
    }


    // GET: /Manufacturers/Details/5

    public async Task<IActionResult> Details(int id)
    {
        var model =
            await _service.GetDetailsAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }


    // GET: /Manufacturers/Edit/5

    public async Task<IActionResult> Edit(int id)
    {
        var model =
            await _service.GetEditAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }


    // POST: /Manufacturers/Edit/5

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        ManufacturerEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }


        var updated =
            await _service.UpdateAsync(model);

        if (!updated)
        {
            ModelState.AddModelError(
                nameof(model.Name),
                "A manufacturer with this name already exists.");

            return View(model);
        }


        TempData["SuccessMessage"] =
            "Manufacturer updated successfully.";

        return RedirectToAction(nameof(Index));
    }


    // GET: /Manufacturers/Delete/5

    public async Task<IActionResult> Delete(int id)
    {
        var model =
            await _service.GetDetailsAsync(id);

        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }


    // POST: /Manufacturers/Delete/5

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        var deleted =
            await _service.DeleteAsync(id);

        if (!deleted)
        {
            TempData["ErrorMessage"] =
                "This manufacturer cannot be deleted because it is being used by a vehicle.";

            return RedirectToAction(nameof(Index));
        }


        TempData["SuccessMessage"] =
            "Manufacturer deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
}