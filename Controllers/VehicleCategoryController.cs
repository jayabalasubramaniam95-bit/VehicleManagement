using Microsoft.AspNetCore.Mvc;
using VehicleManagement.Services;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Controllers;

public class VehicleCategoryController : Controller
{
    #region Constants

    private const string SuccessKey = "SuccessMessage";
    private const string ErrorKey = "ErrorMessage";
    private const string DuplicateNameMessage = "A category with this name already exists.";

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

    #region Create

    [HttpGet]
    public ActionResult Create() =>
        View("Form", new VehicleCategoryFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public ActionResult Create(VehicleCategoryFormViewModel model)
    {
        ValidateUniqueName(model);
        if (!ModelState.IsValid) return View("Form", model);

        return CompleteSave(_categoryService.Create(model), model, "added");
    }

    #endregion

    #region Edit

    [HttpGet]
    public ActionResult Edit(int id)
    {
        var model = _categoryService.GetForEdit(id);
        return model is null ? NotFound() : View("Form", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public ActionResult Edit(VehicleCategoryFormViewModel model)
    {
        ValidateUniqueName(model);
        if (!ModelState.IsValid) return View("Form", model);

        return CompleteSave(_categoryService.Update(model), model, "updated");
    }

    #endregion

    #region Delete (confirmed via popup on the Index page, POST only)

    [HttpPost, ValidateAntiForgeryToken]
    public ActionResult Delete(int id)
    {
        var (key, message) = _categoryService.Delete(id) switch
        {
            CategoryDeleteResult.Deleted      => (SuccessKey, "Category deleted. The neighbouring range was extended to keep the ranges continuous."),
            CategoryDeleteResult.HasVehicles  => (ErrorKey, "This category is used by vehicles and cannot be deleted."),
            CategoryDeleteResult.LastCategory => (ErrorKey, "The last remaining category cannot be deleted."),
            _                                 => (ErrorKey, "Category was not found.")
        };

        TempData[key] = message;
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Remote (client-side) Validation

    [AcceptVerbs("GET", "POST")]
    public ActionResult IsNameAvailable(string name, int id) =>
        Json(string.IsNullOrWhiteSpace(name) || !_categoryService.NameExists(name, id == 0 ? null : id));

    #endregion

    #region Helpers

    /// <summary>Server-side duplicate check: the Remote attribute alone can be bypassed.</summary>
    private void ValidateUniqueName(VehicleCategoryFormViewModel model)
    {
        if (!ModelState.IsValid) return;

        if (_categoryService.NameExists(model.Name, model.IsEdit ? model.Id : null))
            ModelState.AddModelError(nameof(model.Name), DuplicateNameMessage);
    }

    /// <summary>Shared outcome handling for Create and Edit.</summary>
    private ActionResult CompleteSave(CategorySaveResult result, VehicleCategoryFormViewModel model, string action)
    {
        switch (result.Status)
        {
            case CategorySaveStatus.NotFound:
                return NotFound();

            case CategorySaveStatus.InvalidRange:
                ModelState.AddModelError(string.Empty, result.Error!);
                return View("Form", model);

            default:
                TempData[SuccessKey] = $"Vehicle category '{model.Name}' was {action}. Existing vehicles were re-categorised.";
                return RedirectToAction(nameof(Index));
        }
    }

    #endregion
}
