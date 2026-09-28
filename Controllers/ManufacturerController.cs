using Microsoft.AspNetCore.Mvc;
using VehicleManagement.Services;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Controllers;

public class ManufacturerController : Controller
{
    #region Constants

    private const int PageSize = 10;

    private const string SuccessKey = "SuccessMessage";
    private const string ErrorKey = "ErrorMessage";

    private const string DuplicateNameError = "A manufacturer with this name already exists.";
    private const string InUseError = "This manufacturer cannot be deleted because it is being used by a vehicle.";

    #endregion

    #region Dependencies & Constructor

    private readonly IManufacturerService _service;

    public ManufacturerController(IManufacturerService service)
    {
        _service = service;
    }

    #endregion

    #region List (Index)

    [HttpGet]
    public ActionResult Index(string? search, int page = 1) =>
        View(_service.GetManufacturers(search, page, PageSize));

    #endregion

    #region Create

    [HttpGet]
    public ActionResult Create() => View();

    [HttpPost]
    public ActionResult Create(ManufacturerCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!_service.Create(model))
        {
            ModelState.AddModelError(nameof(model.Name), DuplicateNameError);
            return View(model);
        }

        return RedirectWithSuccess("Manufacturer created successfully.");
    }

    #endregion

    #region Details

    [HttpGet]
    public ActionResult Details(int id) =>
        ViewOrNotFound(_service.GetDetailsById(id));

    #endregion

    #region Edit

    [HttpGet]
    public ActionResult Edit(int id) =>
        ViewOrNotFound(_service.GetEdit(id));

    [HttpPost]
    public ActionResult Edit(ManufacturerEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!_service.Update(model))
        {
            ModelState.AddModelError(nameof(model.Name), DuplicateNameError);
            return View(model);
        }

        return RedirectWithSuccess("Manufacturer updated successfully.");
    }

    #endregion

    #region Delete

    [HttpGet]
    public ActionResult Delete(int id) =>
        ViewOrNotFound(_service.GetDetailsById(id));

    [HttpPost]
    [ActionName("Delete")]
    public ActionResult DeleteConfirmed(int id)
    {
        if (!_service.Delete(id))
        {
            return RedirectWithError(InUseError);
        }

        return RedirectWithSuccess("Manufacturer deleted successfully.");
    }

    #endregion

    #region Helpers

    private ActionResult ViewOrNotFound<TModel>(TModel? model) where TModel : class =>
        model is null ? NotFound() : View(model);

    private ActionResult RedirectWithSuccess(string message)
    {
        TempData[SuccessKey] = message;
        return RedirectToAction(nameof(Index));
    }

    private ActionResult RedirectWithError(string message)
    {
        TempData[ErrorKey] = message;
        return RedirectToAction(nameof(Index));
    }

    #endregion
}