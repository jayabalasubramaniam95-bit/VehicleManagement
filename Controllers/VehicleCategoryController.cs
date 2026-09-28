using Microsoft.AspNetCore.Mvc;
using VehicleManagement.Models;
using VehicleManagement.Services;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Controllers;

public class VehicleCategoryController : Controller
{
    #region Constants

    private const string SuccessKey = "SuccessMessage";

    #endregion

    #region Dependencies & Constructor

    private readonly IVehicleCategoryService _categoryService;

    public VehicleCategoryController(IVehicleCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    #endregion

    #region List (Index)

    [HttpGet]
    public ActionResult Index() =>
        View(_categoryService.GetList());

    #endregion

    #region Edit

    [HttpGet]
    public ActionResult Edit(int id)
    {
        var Vehicle = _categoryService.GetById(id);

        if (Vehicle is null && Vehicle.Category is null)
        {
            return NotFound();
        }

        return View(new VehicleCategoryEditViewModel
        {
            Id = Vehicle.Category.Id,
            Name = Vehicle.Category.Name,
            MinWeight = Vehicle.Category.MinWeight,
            MaxWeight = (Vehicle.Category.MaxWeight ?? 0)
        });
    }

    [HttpPost]
    public ActionResult Edit(VehicleCategoryEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.MinWeight > model.MaxWeight)
        {
            ModelState.AddModelError(
                nameof(model.MaxWeight),
                "Maximum weight must be greater than or equal to minimum weight.");

            return View(model);
        }

        _categoryService.Update(new VehicleCategory
        {
            Id = model.Id,
            Name = model.Name.Trim(),
            MinWeight = model.MinWeight,
            MaxWeight = model.MaxWeight
        });

        TempData[SuccessKey] = "Vehicle category updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    #endregion
}