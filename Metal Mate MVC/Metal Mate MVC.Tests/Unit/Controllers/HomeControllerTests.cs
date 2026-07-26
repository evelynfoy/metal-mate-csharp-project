using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;

using Metal_Mate_MVC.Controllers;
using Metal_Mate_MVC.DTOs;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;

namespace Metal_Mate_MVC.Tests
{
    public class HomeControllerTests
    {

        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly Mock<IApiService> _apiServiceMock;

        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _apiServiceMock = new Mock<IApiService>();

            _controller = new HomeController(
                _loggerMock.Object,
                _apiServiceMock.Object);
        }

        // Mocked response - happy path
        [Fact]
        public async Task Index_ReturnsAViewResult()
        {
            // Arrange
            // Mock the API service to return a valid SpotPrice object
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/EUR"))
                .ReturnsAsync(new SpotPrice
                {
                    Name = "Gold",
                    UpdatedAtReadable = "a few minutes ago",
                    UpdatedAt = DateTime.UtcNow,
                    CurrencySymbol = "$",
                    ExchangeRate = 1.00f,
                    Symbol = "XAU",
                    Currency = "EUR",
                    Price = 3500.00f
                });

            // Mock the API service to return a valid list of metals
            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" }
             });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HomeViewModel>(viewResult.Model);
            Assert.NotNull(model.SpotPrice);
            Assert.Equal("Gold", model.SpotPrice.Name);
            Assert.NotNull(model.Metals);
            Assert.Equal("Gold", model.Metals.ElementAt(1).Text);
        }

        // Mocked response - Exception thrown from the service
        [Fact]
        public async Task Index_ReturnsAnErrorResult()
        {
            // Arrange

            // Mock the API service to return an exception when trying to get the SpotPrice
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/USD"))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Mock the API service to return an exception when trying to get the list of metals
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HomeViewModel>(viewResult.Model);

            Assert.Null(model.SpotPrice);
            Assert.Null(model.Metals);
            Assert.Equal("The price site is unavailable at the moment. Please try again later.", model.ErrorMessage);
        }

        // Mocked response - Happy path for the GetSpotPriceAsync Method 
        [Fact]
        public async Task GetSpotPriceAsync_ReturnsOkWithSpotPrice()
        {
            // Arrange

            // Mock the API service to return a valid SpotPrice object
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/EUR"))
                .ReturnsAsync(new SpotPrice
                {
                    Name = "Gold",
                    UpdatedAtReadable = "a few minutes ago",
                    UpdatedAt = DateTime.UtcNow,
                    CurrencySymbol = "$",
                    ExchangeRate = 1.00f,
                    Symbol = "XAU",
                    Currency = "USD",
                    Price = 3500.00f
                });

            // Mock the API service to return a valid list of metals
            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" }
             });

            // Act
            var result = await _controller.GetSpotPriceAsync("XAU","EUR");

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<SpotPriceResponse>(json.Value);
            Assert.NotNull(response);
            Assert.Equal(3500.00f,response.Price);
            Assert.Equal("$", response.CurrencySymbol);

        }

        // Mocked response - Exception thrown from the service while calling the GetSpotPriceAsync Method       
        [Fact]
        public async Task GetSpotPriceAsync_ReturnsInternalServerError()
        {
            // Arrange

            // Mock the API service to return an exception when trying to get the SpotPrice
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/USD"))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Mock the API service to return an exception when trying to get the list of metals
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Act
            var result = await _controller.GetSpotPriceAsync("XAU", "EUR");

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(objectResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);

            var message = objectResult.Value
                .GetType()
                .GetProperty("message")?
                .GetValue(objectResult.Value); 

            Assert.Equal("The price site is unavailable at the moment. Please try again later.", message);

        }
    }
}
