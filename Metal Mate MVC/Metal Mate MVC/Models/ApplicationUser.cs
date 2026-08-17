using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace Metal_Mate_MVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FavouriteMetal { get; set; } = "XAU";
        public string FavouriteCurrency { get; set; } = "EUR";
        public ICollection<AlertRequest> AlertRequests { get; set; } = new List<AlertRequest>();
    }
}
