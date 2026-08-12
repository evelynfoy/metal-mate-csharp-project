using Microsoft.AspNetCore.Identity;

namespace Metal_Mate_MVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FavouriteMetal { get; set; } = "XAU";
        public string FavouriteCurrency { get; set; } = "EUR";
    }
}
