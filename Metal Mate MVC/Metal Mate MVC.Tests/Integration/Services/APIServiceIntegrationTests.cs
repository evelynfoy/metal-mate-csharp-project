
using Metal_Mate_MVC.Services;
using Metal_Mate_MVC.Exceptions;

namespace Metal_Mate_MVC.Tests.Integration.Services
{
    public class APIServiceIntegrationTests
    {
        [Fact]
        public async Task GetSpotPriceAsync_ValidResponse_ReturnsSpotPrice_FromRealAPI()
        {
            // Calls the real API - happy path
            // Arange
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new APIService(client);

            //Act
            var result = await service.GetSpotPriceAsync("XAU", "USD");

            //Assert
            Assert.NotNull(result);
            Assert.Equal("XAU", result.Symbol);
        }

        [Fact]
        public async Task GetSpotPriceAsync_ClientError_ThrowsApiException_FromRealAPI()
        {
            // Calls the real API - invalid currency code to test error handling

            // Arrange
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new APIService(client);

            // Act & Assert
            await Assert.ThrowsAsync<ApiClientErrorException>(() =>
                service.GetSpotPriceAsync("XAU", "INVALID"));
        }
    }
}
