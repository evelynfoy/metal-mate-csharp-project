using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metal_Mate_MVC.Controllers
{

    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApiService _apiService;

        public ProfileController(ILogger<ProfileController> logger, 
            UserManager<ApplicationUser> userManager,
            IApiService apiService)
        {
            _logger = logger;
            _userManager = userManager;
            _apiService = apiService;
        }

        // Display the profile and the populated dropdowns for metals and currencies
        [HttpGet]
        public async Task<IActionResult> Edit()
        {

            var model = new EditProfileViewModel
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                FavouriteMetal = string.Empty,
                FavouriteCurrency = string.Empty,
                Metals = null,
                Currencies = null,
            };

            // Get the current logged-in user
            try
            {
                var user = await _userManager.GetUserAsync(User);
                var metals = await _apiService.GetAPIDataAsync<List<Metal>>("symbols");
                // Map to a view model
                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "Your profile information is temporarily unavailable. Please try again later.";
                    return View(model);
                }
                model = new EditProfileViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FavouriteMetal = user.FavouriteMetal,
                    FavouriteCurrency = user.FavouriteCurrency,
                };
                model.Metals = metals.Select(x => new SelectListItem
                {
                    Value = x.Symbol.ToString(),
                    Text = x.Name.ToString()
                });
                string[] currencies = ["EUR", "AUD", "BRL", "CAD", "CHF", "CNY", "DKK", "GBP", "HKD", "INR", "JPY", "KRW", "MXN", "NOK", "NZD", "SEK", "SGD", "USD", "ZAR"];
                model.Currencies = currencies.Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data for the page." + ex.Message);
                model.ErrorMessage = "Your profile information is temporarily unavailable. Please try again later.";
            }
            return View(model);
        }

        // Save changes
        [HttpPost]
        public async Task<IActionResult> Edit( EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "Your profile information is temporarily unavailable. Please try again later.";
                    return View(model);
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.FavouriteCurrency = model.FavouriteCurrency;
                user.FavouriteMetal = model.FavouriteMetal;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View(model);
                }

                TempData["Success"] = "Profile updated successfully.";

                return RedirectToAction(nameof(Edit));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile. {error}", ex.Message);
                model.ErrorMessage = "An error occurred while updating your profile. Please try again later.";
                return View(model);
            }
        }
    }
}
