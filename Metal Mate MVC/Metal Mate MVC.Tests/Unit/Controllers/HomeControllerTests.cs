using Metal_Mate_MVC.Controllers;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

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
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/USD"))
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
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HomeViewModel>(viewResult.Model);
            Assert.NotNull(model.SpotPrice);
            Assert.Equal("Gold", model.SpotPrice.Name);
            Assert.NotNull(model.metals);
            Assert.Equal("Gold", model.metals[1].Name);
        }

        // Mocked response - Exception thrown from the service
        [Fact]
        public async Task Index_ReturnsAnErrorResult()
        {
            // Arrange

            // Mock the API service to return a valid SpotPrice object
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/USD"))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Mock the API service to return a valid list of metals
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HomeViewModel>(viewResult.Model);

            Assert.Null(model.SpotPrice);
            Assert.Null(model.metals);
            Assert.Equal("An error ocurred", model.ErrorMessage);
        }
    }
}
