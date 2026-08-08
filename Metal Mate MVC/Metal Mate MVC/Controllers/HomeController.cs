using Metal_Mate_MVC.DTOs;
using Metal_Mate_MVC.Exceptions;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using System.Diagnostics;


namespace Metal_Mate_MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IApiService _apiService;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<ApplicationUser> userManager, 
                              IApiService apiService)
        {
            _logger = logger;
            _userManager = userManager;
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                SilverSpotPrice = null,
                PlatinumSpotPrice = null,
                SpotPrice = null,
                Metals = null,
                SelectedMetal = null,
                Currencies = null,
                SelectedCurrency = null,
                ErrorMessage = null
            };

            try
            {
                string[] currencies = ["EUR", "AUD", "BRL", "CAD", "CHF", "CNY", "DKK", "GBP", "HKD", "INR", "JPY", "KRW", "MXN", "NOK", "NZD", "SEK", "SGD", "USD", "ZAR"];
                model.Currencies = currencies.Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c
                });
                model.SelectedCurrency = "EUR";

                var metals = await _apiService.GetAPIDataAsync<List<Metal>>("symbols");
                model.Metals = metals.Select(x => new SelectListItem
                {
                    Value = x.Symbol.ToString(),
                    Text = x.Name.ToString()
                });
                model.SelectedMetal = "XAU";

                await SetUserPreferencesAsync(model);

                var spotPrice = await _apiService.GetAPIDataAsync<SpotPrice>($"price/{model.SelectedMetal}/{model.SelectedCurrency}");
                model.SpotPrice = spotPrice;
                model.GoldSpotPrice = spotPrice;

                if (model.SelectedMetal != "XAU")
                {
                    var gold = "XAU";
                    var goldSpotPrice = await _apiService.GetAPIDataAsync<SpotPrice>($"price/{gold}/{model.SelectedCurrency}");
                    model.GoldSpotPrice = goldSpotPrice;
                }

                var silver = "XAG";
                var silverSpotPrice = await _apiService.GetAPIDataAsync<SpotPrice>($"price/{silver}/{model.SelectedCurrency}");
                model.SilverSpotPrice = silverSpotPrice;

                var platinum = "XPT";
                var platinumSpotPrice = await _apiService.GetAPIDataAsync<SpotPrice>($"price/{platinum}/{model.SelectedCurrency}");
                model.PlatinumSpotPrice = platinumSpotPrice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching data from the API." + ex.Message);    
                model.ErrorMessage = "The price site is unavailable at the moment. Please try again later.";
            }
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /* 
         * Called from JavaScript on change of the metal selection dropdown and calls the API passing in the 
         * new selection. 
         */
        [HttpGet]
        public async Task<IActionResult> GetSpotPriceAsync(string metal, string currency)
        {
            try
            {
                var spotPrice = await _apiService.GetAPIDataAsync<SpotPrice>($"price/{metal}/{currency}");

                return Json(new SpotPriceResponse
                {
                    Price = spotPrice.Price,
                    ExchangeRate = spotPrice.ExchangeRate,
                    CurrencySymbol = spotPrice.CurrencySymbol,
                    UpdatedAt = spotPrice.UpdatedAt.ToString("dd/MM/yyyy HH:mm:ss")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the spot price from the API.");
                return StatusCode(500, new { message = "The price site is unavailable at the moment. Please try again later." });
            }
        }

        private async Task SetUserPreferencesAsync(HomeViewModel model)
        {
            if (User?.Identity?.IsAuthenticated != true)
                return;

            try
            {
                var user = await _userManager.GetUserAsync(User);

                if (user is null)
                    throw new UserProfileErrorException("User profile not found.");

                model.SelectedMetal = user.FavouriteMetal;
                model.SelectedCurrency = user.FavouriteCurrency;
            }
            catch (UserProfileErrorException ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the user profile.");

                model.ErrorMessage =
                    "Apologies, your profile information is currently unavailable so your favourite selections cannot be defaulted. Please use the dropdowns above.";
            }
        }
    }
}
