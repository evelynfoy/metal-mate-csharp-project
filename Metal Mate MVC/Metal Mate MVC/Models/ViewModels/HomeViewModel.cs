using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Metal_Mate_MVC.DTOs;

namespace Metal_Mate_MVC.Models.ViewModels
{
    public class HomeViewModel : ICommonSelectLists
    {
        public SpotPrice? GoldSpotPrice { get; set; } = null;
        public SpotPrice? SilverSpotPrice { get; set; } = null;
        public SpotPrice? PlatinumSpotPrice { get; set; } = null;

        public SpotPrice? SelectedSpotPrice { get; set; } = null;

        [Display(Name = "Metal")]
        public string SelectedMetal { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> Metals { get; set; } = Enumerable.Empty<SelectListItem>();

        [Display(Name = "Currencies")]
        public string SelectedCurrency { get; set; } = string.Empty;
        public IEnumerable<SelectListItem> Currencies { get; set; } = Enumerable.Empty<SelectListItem>();

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
