using System.ComponentModel.DataAnnotations;

namespace Metal_Mate_MVC.Models
{
    public class SpotPrice
    {
        public required string Currency { get; set; }
        public required string CurrencySymbol { get; set; }
        [Display(Name = "Exchange Rate")]
        public required float ExchangeRate { get; set; }
        public required string Name { get; set; }
        [Display(Name = "Spot Price")]
        public required float Price { get; set; }
        public required string Symbol { get; set; }
        [Display(Name = "Updated At")]
        public required DateTime UpdatedAt { get; set; }
        public required string UpdatedAtReadable { get; set; }
    }
}
