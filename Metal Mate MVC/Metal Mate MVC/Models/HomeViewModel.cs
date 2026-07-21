using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.Rendering;

/* 
 * Takes the list of metals returned from the API and converts it into 
 * a list of SelectListItem objects for the dropdown in the view.
 */
namespace Metal_Mate_MVC.Models
{
    public class HomeViewModel
    {
        public SpotPrice? SpotPrice { get; set; }
        [Display(Name = "Metal")]
        public string? SelectedMetal { get; set; }
        public IEnumerable<SelectListItem>? Metals { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
