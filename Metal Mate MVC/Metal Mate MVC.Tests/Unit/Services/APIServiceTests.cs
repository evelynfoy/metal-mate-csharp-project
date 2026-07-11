using System.Net;
using Moq;
using Moq.Protected;
using Metal_Mate_MVC.Services;
using Metal_Mate_MVC.Exceptions;
using System.Text;

namespace Metal_Mate_MVC.Tests
{
    public class APIServiceTests
    {
        // Mocked response - happy path
        [Fact]
        public async Task GetSpotPriceAsync_ValidResponse_ReturnsSpotPrice()
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

            var service = new APIService(httpClient);

            // Act
            var result = await service.GetSpotPriceAsync("XAU","USD");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("USD", result!.Currency);
            Assert.Equal("Gold", result.Name);
            Assert.Equal(4086.199951f, result.Price);

        }

        // Mocked response - StatusCode OK but null content returned
        [Fact]
        public async Task GetSpotPriceAsync_NullResponse_ThrowsException()
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

            var service = new APIService(httpClient);

            // Act and Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetSpotPriceAsync("XAU", "USD"));

        }

        // Mocked response - Invalid parameter passed to API, returns 404 Not Found
        [Fact]
        public async Task GetSpotPriceAsync_ClientError_ThrowsApiClientErrorException()
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

            var service = new APIService(httpClient);

            // Act and Assert
            await Assert.ThrowsAsync<ApiClientErrorException>(() =>
                service.GetSpotPriceAsync("XAU", "USE"));

        }

        /* 
           Mocked Response - Set up an HttpClient so it throws an HttpRequestException every time the API is called.
           This simulates a network error. The test verifies that the GetSpotPriceAsync method retries the request three times before
           throwing a final exception. 
        */
        [Fact]
        public async Task GetSpotPriceAsync_NetworkError_ThrowsExceptionAfterThreeRetries()
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

            var service = new APIService(httpClient);

            // Act and Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                service.GetSpotPriceAsync("BTC", "USD"));

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