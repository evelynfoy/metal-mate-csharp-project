using System.ComponentModel.DataAnnotations;

namespace Metal_Mate_MVC.Models
{
    public class EditProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FavouriteMetal { get; set; } = string.Empty;

        public string? FavouriteCurrency { get; set; } = string.Empty;
    }
}
