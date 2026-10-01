using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleManagement.Services;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Controllers;

public class ManufacturersController : Controller
{
    #region Fields and Constructor

    private const int PageSize = 10;
    private const string DuplicateNameMessage = "A manufacturer with this name already exists.";

    private readonly IManufacturerService _manufacturerservice;

    public ManufacturersController(IManufacturerService manufacturerservice) => _manufacturerservice = manufacturerservice;

    #endregion

    #region List and Details

    public ActionResult Index(
        string? search, int page = 1) =>
        View(_manufacturerservice.GetPageWiseManufacturer(search, page, PageSize));

    public ActionResult Details(int id)
    {
        var model = _manufacturerservice.GetManufacturerDetails(id);
        return model is null ? NotFound() : View(model);
    }

    #endregion

    #region Create

    public ActionResult Create() => View("Form", new ManufacturerFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public ActionResult Create(ManufacturerFormViewModel model)
    {
        ValidateUniqueName(model);
        if (!ModelState.IsValid) return View("Form", model);

        _manufacturerservice.Create(model);
        TempData["SuccessMessage"] = $"Manufacturer '{model.Name}' was added.";
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Edit

    public ActionResult Edit(int id)
    {
        var model = _manufacturerservice.GetManufacturerForEdit(id);
        return model is null ? NotFound() : View("Form", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public ActionResult Edit(ManufacturerFormViewModel model)
    {
        ValidateUniqueName(model);
        if (!ModelState.IsValid) return View("Form", model);

        if (!_manufacturerservice.Update(model)) return NotFound();

        TempData["SuccessMessage"] = $"Manufacturer '{model.Name}' was updated.";
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Delete (confirmed via popup on the Index page, POST only)

    [HttpPost, ValidateAntiForgeryToken]
    public ActionResult Delete(int id)
    {
        var (key, message) = _manufacturerservice.Delete(id) switch
        {
            DeleteResult.Deleted     => ("SuccessMessage", "Manufacturer deleted."),
            DeleteResult.IsDefault   => ("ErrorMessage", "Default manufacturers cannot be deleted."),
            DeleteResult.HasVehicles => ("ErrorMessage", "This manufacturer is used by vehicles and cannot be deleted."),
            _                        => ("ErrorMessage", "Manufacturer was not found.")
        };

        TempData[key] = message;
        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Remote (client-side) Validation

    [AcceptVerbs("GET", "POST")]
    public ActionResult IsNameAvailable(string name, int id) =>
        Json(string.IsNullOrWhiteSpace(name) || !_manufacturerservice.IsManufacturersNameExists(name, id == 0 ? null : id));

    #endregion

    #region Helpers

    /// <summary>Server-side duplicate check: the Remote attribute alone can be bypassed.</summary>
    private void ValidateUniqueName(ManufacturerFormViewModel model)
    {
        if (!ModelState.IsValid) return;

        if (_manufacturerservice.IsManufacturersNameExists(model.Name, model.IsEdit ? model.Id : null))
            ModelState.AddModelError(nameof(model.Name), DuplicateNameMessage);
    }

    #endregion
}