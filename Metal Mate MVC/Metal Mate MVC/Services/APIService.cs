
using Metal_Mate_MVC.Exceptions;

namespace Metal_Mate_MVC.Services;

public interface IApiService
{
    Task<T?> GetAPIDataAsync<T>(string url) where T : class;
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /*
        Calls the API endpoint passed to the method to get either 
            1) the list of valid metals or
            2) the spot price of a metal in a specific currency 
        Implements retry logic for transient errors (network issues, server errors).
        Uses a generic type parameter T to allow for different return types (e.g., List<Metal> or SpotPrice).
    */
    public async Task<T?> GetAPIDataAsync<T>(string url) where T : class
    {
        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(2);
        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<T>();
                    if (result is null)
                    {
                        throw new InvalidOperationException("The API returned an empty response.");
                    }
                    return result;
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

