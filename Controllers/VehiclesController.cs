using Microsoft.AspNetCore.Mvc;
using VehicleManagement.Services;
using VehicleManagement.ViewModels;

namespace VehicleManagement.Controllers;

public class VehiclesController : Controller
{
    #region Constants

    private const int PageSize = 10;

    private const string SuccessKey = "SuccessMessage";
    private const string ErrorKey = "ErrorMessage";

    #endregion

    #region Dependencies & Constructor

    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    #endregion

    #region List (Index)

    [HttpGet]
    public ActionResult Index(string? search, int page = 1) =>
        View(_vehicleService.GetVehicles(search, page, PageSize));

    #endregion

    #region Create

    [HttpGet]
    public ActionResult Create() =>
        View(_vehicleService.GetCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Create(VehicleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ReloadDropdowns(model);
            return View(model);
        }

        var result = _vehicleService.Create(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            ReloadDropdowns(model);
            return View(model);
        }

        return RedirectWithSuccess("Vehicle was created successfully.");
    }

    #endregion

    #region Details

    [HttpGet]
    public ActionResult Details(int id) =>
        ViewOrNotFound(_vehicleService.GetDetails(id));

    #endregion

    #region Edit

    [HttpGet]
    public ActionResult Edit(int id) =>
        ViewOrNotFound(_vehicleService.GetEditViewModel(id));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit(int id, VehicleFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ReloadDropdowns(model);
            return View(model);
        }

        var result = _vehicleService.Update(model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage!);
            ReloadDropdowns(model);
            return View(model);
        }

        return RedirectWithSuccess("Vehicle was updated successfully.");
    }

    #endregion

    #region Delete

    [HttpGet]
    public ActionResult Delete(int id) =>
        ViewOrNotFound(_vehicleService.GetDeleteViewModel(id));

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        var result = _vehicleService.Delete(id);

        if (!result.Success)
        {
            return RedirectWithError(result.ErrorMessage!);
        }

        return RedirectWithSuccess("Vehicle was deleted successfully.");
    }

    #endregion

    #region Helpers

    private void ReloadDropdowns(VehicleFormViewModel model)
    {
        var refreshed = _vehicleService.GetCreateViewModel();

        model.Manufacturers = refreshed.Manufacturers;
        model.Categories = refreshed.Categories;
    }

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