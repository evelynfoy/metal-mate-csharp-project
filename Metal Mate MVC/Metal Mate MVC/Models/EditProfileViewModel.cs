using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Metal_Mate_MVC.Models
{
    public class EditProfileViewModel
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Favourite Metal")]
        public string FavouriteMetal { get; set; } = string.Empty;

        [Display(Name = "Favourite Currency")]
        public string FavouriteCurrency { get; set; } = string.Empty;

        public IEnumerable<SelectListItem>? Metals { get; set; } = Enumerable.Empty<SelectListItem>();
        public IEnumerable<SelectListItem>? Currencies { get; set; } = Enumerable.Empty<SelectListItem>();

        public string? ErrorMessage { get; set; }
    }
}
