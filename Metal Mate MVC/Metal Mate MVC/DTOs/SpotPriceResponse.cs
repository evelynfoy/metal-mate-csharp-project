/* Used to represent the response from the GetSpotPriceAsync function in the Home Controller */

namespace Metal_Mate_MVC.DTOs
{
    public class SpotPriceResponse
    {
        public float Price { get; init; }
        public float ExchangeRate { get; init; }
        public string CurrencySymbol { get; init; } = string.Empty;
        public string UpdatedAt { get; init; } = string.Empty;
    }
}
