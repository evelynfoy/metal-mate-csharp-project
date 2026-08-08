using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

/* 
 * Takes the list of metals returned from the API and converts it into 
 * a list of SelectListItem objects for the dropdown in the view.
 * Similarily, the list of currencies is also converted into a list of SelectListItem objects.
 */
namespace Metal_Mate_MVC.Models
{
    public class HomeViewModel
    {
        public SpotPrice? GoldSpotPrice { get; set; }
        public SpotPrice? SilverSpotPrice { get; set; }
        public SpotPrice? PlatinumSpotPrice { get; set; }

        public SpotPrice? SpotPrice { get; set; }

        [Display(Name = "Metal")]
        public string? SelectedMetal { get; set; }
        public IEnumerable<SelectListItem>? Metals { get; set; }

        [Display(Name = "Currencies")]
        public string? SelectedCurrency { get; set; }
        public IEnumerable<SelectListItem>? Currencies { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
