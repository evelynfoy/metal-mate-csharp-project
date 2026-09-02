using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Metal_Mate_MVC.Constants;

namespace Metal_Mate_MVC.Services
{
    public interface IDropdownOptionsService
    {
        Task PopulateAsync(AlertRequestViewModel model);
    }

    public class DropdownOptionsService : IDropdownOptionsService
    {
        private readonly IApiService _apiService;

        public DropdownOptionsService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task PopulateAsync(AlertRequestViewModel model)
        {
            var metals = await _apiService.GetAPIDataAsync<List<Metal>>("symbols");

            model.Metals = metals.Select(x => new SelectListItem
            {
                Value = x.Name.ToString(),
                Text = x.Name.ToString()
            });

            model.Currencies = SupportedCurrencies.All.Select(c => new SelectListItem
            {
                Value = c,
                Text = c
            });
        }
    }
}
