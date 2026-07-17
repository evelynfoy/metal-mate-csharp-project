using Metal_Mate_MVC.Exceptions;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace Metal_Mate_MVC.Tests
{
    public class ApiServiceTests
    {

        // Mocked response - happy path - Symbols
        [Fact]
        public async Task GetAPIDataAsync_ValidResponse_ReturnsSymbols()
        {

            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(new[]
                {
                  new JsonObject
                  {
                      ["name"] = "Silver",
                      ["symbol"] = "XAG"
                  },
                  new JsonObject
                  {
                      ["name"] = "Gold",
                      ["symbol"] = "XAU"
                  }
                })

            });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new ApiService(httpClient);

            // Act
            var result = await service.GetAPIDataAsync<List<Metal>>("symbols");

            // Assert
            Assert.NotNull(result);
            var metals = Assert.IsType<List<Metal>>(result);

            Assert.Equal("XAG", metals[0].Symbol);
            Assert.Equal("Silver", metals[0].Name);

        }

        // Mocked response - happy path - Spot Price
        [Fact]
        public async Task GetAPIDataAsync_ValidResponse_ReturnsSpotPrice()
        {

            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "{\"currency\": \"USD\"," +
                    "\"currencySymbol\": \"$\"," +
                    "\"exchangeRate\": 1," +
                    "\"name\": \"Gold\"," +
                    "\"price\": 4086.199951," +
                    "\"symbol\" :\"XAU \"," +
                    "\"updatedAt\": \"2026-07-08T19:42:50Z\"," +
                    "\"updatedAtReadable\": \"a few seconds ago\"}")
            });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new ApiService(httpClient);

            // Act
            var result = await service.GetAPIDataAsync<SpotPrice>("price/XAU/USD");

            Assert.NotNull(result);
            var spotPrice = Assert.IsType<SpotPrice>(result);
            Assert.Equal("USD", spotPrice.Currency);
            Assert.Equal("Gold", spotPrice.Name);
            Assert.Equal(4086.199951f, spotPrice.Price);

        }

        // Mocked response - StatusCode OK but null content returned
        [Fact]
        public async Task GetAPIDataAsync_NullResponse_ThrowsException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(
                    "null",
                    Encoding.UTF8,
                    "application/json")
            });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new ApiService(httpClient);

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetAPIDataAsync<SpotPrice>("price/XAU/USD"));

        }

        // Mocked response - Invalid parameter passed to API, returns 404 Not Found
        [Fact]
        public async Task GetAPIAsync_ClientError_ThrowsApiClientErrorException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            });

            var httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new ApiService(httpClient);

            // Act and Assert
            await Assert.ThrowsAsync<ApiClientErrorException>(() =>
                service.GetAPIDataAsync<SpotPrice>("price/XAU/INVALID"));

        }

        /* 
           Mocked Response - Set up an HttpClient so it throws an HttpRequestException every time the API is called.
           This simulates a network error. The test verifies that the GetSpotPriceAsync method retries the request three times before
           throwing a final exception. 
        */
        [Fact]
        public async Task GetAPIAsync_NetworkError_ThrowsExceptionAfterThreeRetries()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(handlerMock.Object);
            httpClient.BaseAddress = new Uri("https://api.gold-api.com/");

            var service = new ApiService(httpClient);

            // Act and Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetAPIDataAsync<SpotPrice>("price/BTC/USD"));

            Assert.Equal(
                "Failed to retrieve data after 3 attempts.",
                ex.Message);

            Assert.IsType<HttpRequestException>(ex.InnerException);

            handlerMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(3),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());

        }
    }
}