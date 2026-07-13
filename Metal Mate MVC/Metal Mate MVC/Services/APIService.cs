
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Exceptions;

namespace Metal_Mate_MVC.Services;

public interface IApiService
{
    Task<SpotPrice?> GetSpotPriceAsync(string symbol, string currencyCode);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /*
        Calls the Gold Price API to get the spot price of a metal in a specific currency.
        Implements retry logic for transient errors (network issues, server errors).
    */
    public async Task<SpotPrice?> GetSpotPriceAsync(string symbol, string currencyCode)
    {
        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(2);
        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync($"price/{symbol}/{currencyCode}");
                if (response.IsSuccessStatusCode)
                {
                    var price = await response.Content.ReadFromJsonAsync<SpotPrice>();
                    if (price is null)
                    {
                        throw new InvalidOperationException("The API returned an empty response.");
                    }
                    return price;
                }

                // Don't retry for client errors (400-499)
                if ((int)response.StatusCode >= 400 &&
                    (int)response.StatusCode < 500)
                {
                    var errorContent =
                        await response.Content.ReadAsStringAsync();

                    throw new ApiClientErrorException(
                        $"API returned {(int)response.StatusCode}: {errorContent}");
                }

                // Retry for server errors (500+)
                Console.WriteLine(
                    $"Attempt {attempt}: Server returned {(int)response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                Console.WriteLine(
                    $"Attempt {attempt}: Network error - {ex.Message}");
            }
            catch (TaskCanceledException ex)
            {
                lastException = ex;
                Console.WriteLine(
                    $"Attempt {attempt}: Request timeout - {ex.Message}");
            }
            catch (Exception)
            {
                // Unexpected errors - don't retry, just throw the exception
                throw;
            }

            if (attempt < maxRetries)
            {
                await Task.Delay(delay);

                // Exponential backoff
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
            }
        }

        throw new Exception(
            $"Failed to retrieve data after {maxRetries} attempts.",
            lastException);

    }

}

