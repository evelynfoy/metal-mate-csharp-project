using Metal_Mate_MVC.DTOs;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Models.ViewModels;
using Metal_Mate_MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace Metal_Mate_MVC.Controllers
{
    [Authorize]
    public class AlertRequestsController : Controller
    {
        private readonly ILogger<AlertRequestsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAlertRequestService _alertRequestService;
        private readonly IDropdownOptionsService _dropdownOptionsService;
        private readonly IApiService _ApiService;


        public AlertRequestsController(ILogger<AlertRequestsController> logger,
                                       UserManager<ApplicationUser> userManager,
                                       IAlertRequestService alertRequestService,
                                       IDropdownOptionsService dropdownOptionsService,
                                       IApiService apiService)
        {
            _logger = logger;
            _userManager = userManager;
            _alertRequestService = alertRequestService;
            _dropdownOptionsService = dropdownOptionsService;
            _ApiService = apiService;
        }

        // GET: AlertRequests
        public async Task<IActionResult> Index()
        {
            var model = new AlertRequestIndexViewModel();

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "Your alert requests are temporarily unavailable. Please try again later.";
                    return View(model);
                }

                model.AlertRequests =
                    await _alertRequestService.GetForUserAsync(user.Id);

                var metals = await _ApiService.GetAPIDataAsync<List<Metal>>("symbols");
                
                model.MetalNames = metals.ToDictionary(m => m.Symbol, m => m.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving alert requests.");
                model.ErrorMessage = "Your alert requests are temporarily unavailable. Please try again later.";
            }

            return View(model);
        }

        // GET: AlertRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var model = new AlertRequestViewModel();
            if (id == null)
            {
                _logger.LogError("The id is null. Id: {id}", id);
                model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                return View(model);
            }
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                    return View(model);
                }

                var alertRequest = await _alertRequestService.GetByIdAsync(id.Value,  user.Id);

                if (alertRequest == null)
                {
                    _logger.LogError("The alert request is null. Request: {id}", id);
                    model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                    return View(model);
                }

                model.Id = alertRequest.Id;
                model.Currency = alertRequest.Currency;
                model.Metal = alertRequest.Metal;
                model.Value = alertRequest.Value;
                model.Operator = alertRequest.Operator;
                model.IsEnabled = alertRequest.IsEnabled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data for the page." + ex.Message);
                model.ErrorMessage = "There was a problem retrieving the information for this page. Please try again later.";
            }

            return View(model);
        }

        // GET: AlertRequests/Create
        public async Task<IActionResult> Create()
        {
            var model = new AlertRequestViewModel();

            try
            {
                await _dropdownOptionsService.PopulateAsync(model);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while fetching data for the page." + ex.Message);
                model.ErrorMessage = "There was a problem retrieving the information for this page. Please try again later.";
            }

            return View(model);
        }

        // POST: AlertRequests/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AlertRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "There was a problem saving this entry. Please try again.";
                    return View(model);
                }

                var alertRequest = new AlertRequest
                {
                    Metal = model.Metal,
                    Currency = model.Currency,
                    Operator = model.Operator,
                    Value = model.Value,
                    IsEnabled = model.IsEnabled,
                    UserId = user.Id,
                    User = user
                };

                await _alertRequestService.AddAsync(alertRequest);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the alert request.");
                model.ErrorMessage = "Your alert request did not save successfully. Please try again later.";
            }
            return View(model);
        }

        // GET: AlertRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var model = new AlertRequestViewModel();

            if (id == null)
            {
                _logger.LogError("The id is null. Id: {id}", id);
                model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                    return View(model);
                }

                var alertRequest = await _alertRequestService.GetByIdAsync(id.Value, user.Id);

                if (alertRequest == null)
                {
                    _logger.LogError("The alert request is null. Request: {id}", id);
                    model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                    return View(model);
                }

                model.Id = alertRequest.Id;
                model.Currency = alertRequest.Currency;
                model.Metal = alertRequest.Metal;
                model.Value = alertRequest.Value;
                model.Operator = alertRequest.Operator;
                model.IsEnabled = alertRequest.IsEnabled;

                await _dropdownOptionsService.PopulateAsync(model);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data for the page." + ex.Message);
                model.ErrorMessage = "There was a problem retrieving the information for this page. Please try again later.";
            }

            return View(model);

        }

        // POST: AlertRequests/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AlertRequestViewModel model)
        {
            if (id != model.Id)
            {
                _logger.LogError(
                    "The id does not match the model id. Id: {id} Model Id: {model.Id}.",
                    id,
                    model.Id);
                model.ErrorMessage = "There was a problem saving this entry. Please try again.";
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                    return View(model);
                }

                var alertRequest = await _alertRequestService.GetByIdAsync(id, user.Id);

                if (alertRequest == null)
                {
                    _logger.LogError("The alert request is null. Id: {Id}", id);
                    model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                    return View(model);
                }

                alertRequest.Value = model.Value;
                alertRequest.Operator = model.Operator;
                alertRequest.Currency = model.Currency;
                alertRequest.Metal = model.Metal;
                alertRequest.IsEnabled = model.IsEnabled;

                await _alertRequestService.SaveAsync(alertRequest);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the data for the page." + ex.Message);
                model.ErrorMessage = "There was a problem saving the information for this page. Please try again later.";
            }
            
            return View(model);
        }

        // GET: AlertRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var model = new AlertRequestViewModel();
            if (id == null)
            {
                _logger.LogError("The id is null. Id: {id}", id);
                model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                return View(model);
            }
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                    return View(model);
                }

                var alertRequest = await _alertRequestService.GetByIdAsync(id.Value, user.Id);

                if (alertRequest == null)
                {
                    _logger.LogError("The alert request is null. Request: {id}", id);
                    model.ErrorMessage = "There was a problem loading this entry. Please try again.";
                    return View(model);
                }

                model.Id = alertRequest.Id;
                model.Currency = alertRequest.Currency;
                model.Metal = alertRequest.Metal;
                model.Value = alertRequest.Value;
                model.Operator = alertRequest.Operator;
                model.IsEnabled = alertRequest.IsEnabled;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data for the page." + ex.Message);
                model.ErrorMessage = "There was a problem retrieving the information for this page. Please try again later.";
            }

            return View(model);

        }

        // POST: AlertRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = new AlertRequestViewModel();
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "There was a problem deleting this entry. Please try again.";
                    return View(model);
                }
                if (!await _alertRequestService.DeleteAsync(id, user.Id))
                {
                    _logger.LogError("The delete failed for Request: {id}", id);
                    model.ErrorMessage = "There was a problem deleting this entry. Please try again.";
                    return View(model);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting data for the page.");
                model.ErrorMessage = "There was a problem deleting this entry. Please try again later.";
            }
            return View(model);

        }
    }
}
