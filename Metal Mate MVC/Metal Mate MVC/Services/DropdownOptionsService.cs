using Metal_Mate_MVC.Constants;
using Metal_Mate_MVC.DTOs;
using Metal_Mate_MVC.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Metal_Mate_MVC.Services
{
    public interface IDropdownOptionsService
    {
        Task PopulateAsync(ICommonSelectLists model);
    }

    public class DropdownOptionsService : IDropdownOptionsService
    {
        private readonly IApiService _apiService;

        public DropdownOptionsService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task PopulateAsync(ICommonSelectLists model)
        {
            var metals = await _apiService.GetAPIDataAsync<List<Metal>>("symbols");

            model.Metals = metals.Select(m => new SelectListItem
            {
                Value = m.Symbol,
                Text = m.Name
            });

            model.Currencies = SupportedCurrencies.All
                .Select(c => new SelectListItem
                {
                    Value = c,
                    Text = c
                });
        }
    }
}
