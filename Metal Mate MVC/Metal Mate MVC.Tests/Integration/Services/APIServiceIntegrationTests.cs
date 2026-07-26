
using Metal_Mate_MVC.Exceptions;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;

namespace Metal_Mate_MVC.Tests.Integration.Services
{
    public class ApiServiceIntegrationTests
    {
        [Fact]
        public async Task GetAPIDataAsync_ValidResponse_ReturnsSpotPrice_FromRealAPI()
        {
            // Calls the real API - happy path
            // Arange
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new ApiService(client);

            //Act
            var result = await service.GetAPIDataAsync<SpotPrice>("price/XAU/USD");

            //Assert
            Assert.NotNull(result);
            Assert.Equal("XAU", result.Symbol);
        }

        [Fact]
        public async Task GetAPIDataAsync_ClientError_ThrowsApiException_FromRealAPI()
        {
            // Calls the real API - invalid currency code to test error handling

            // Arrange
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new ApiService(client);

            // Act & Assert
            await Assert.ThrowsAsync<ApiClientErrorException>(() =>
                service.GetAPIDataAsync<SpotPrice>("price/XAU/INVALID"));
        }
        
        [Fact]
        public async Task GetAPIDataAsync_ValidResponse_ReturnsSymbols_FromRealAPI()
        {
            // Calls the real API - happy path
            // Arange
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new ApiService(client);

            //Act
            var result = await service.GetAPIDataAsync<List<Metal>>("symbols");

            //Assert
            Assert.NotNull(result);
            Assert.IsType<List<Metal>>(result);
        }

        [Fact]
        public async Task GetAPIAsync_ThrowsException_FromRealSymbolsAPI()
        {
            // Calls the real API - invalid path to test error handling 

            // Arrange
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.gold-api.co/");

            var service = new ApiService(client);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                service.GetAPIDataAsync<Metal>("symbols"));
        }
    }
}
