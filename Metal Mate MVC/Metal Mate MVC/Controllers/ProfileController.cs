using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Models.ViewModels;
using Metal_Mate_MVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Metal_Mate_MVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IDropdownOptionsService _dropdownOptionsService;

        public ProfileController(ILogger<ProfileController> logger, 
            UserManager<ApplicationUser> userManager,
            IDropdownOptionsService dropdownOptionsService)
        {
            _logger = logger;
            _userManager = userManager;
            _dropdownOptionsService = dropdownOptionsService;
        }

        // Display the profile and the populated dropdowns for metals and currencies
        [HttpGet]
        public async Task<IActionResult> Edit()
        {

            var model = new EditProfileViewModel();

            try
            {
                // Get the current logged-in user
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogError("The user is null. User: {UserName}", User.Identity?.Name);
                    model.ErrorMessage = "Your profile information is temporarily unavailable. Please try again later.";
                    return View(model);
                }

                // Map to view model
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.FavouriteMetal = user.FavouriteMetal;
                model.FavouriteCurrency = user.FavouriteCurrency;

                await _dropdownOptionsService.PopulateAsync(model);
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
        [ValidateAntiForgeryToken]
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

                // Map from view model
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
