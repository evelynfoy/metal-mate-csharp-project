using Metal_Mate_MVC.Controllers;
using Metal_Mate_MVC.DTOs;
using Metal_Mate_MVC.Exceptions;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Models.ViewModels;
using Metal_Mate_MVC.Services;
using Metal_Mate_MVC.Tests.Unit.SetUp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace Metal_Mate_MVC.Tests
{
    public class HomeControllerTests
    {

        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IApiService> _apiServiceMock;
        private readonly Mock<IDropdownOptionsService> _dropdownOptionsServiceMock;

        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _userManagerMock = SetUpMocks.CreateUserManagerMock();
            _apiServiceMock = new Mock<IApiService>();
            _dropdownOptionsServiceMock = new Mock<IDropdownOptionsService>();

            _controller = new HomeController(
                _loggerMock.Object,
                _userManagerMock.Object,
                _apiServiceMock.Object,
                _dropdownOptionsServiceMock.Object);
        }

        // Index - happy path - anonymous user - Displays spot prices for default values
        [Fact]
        public async Task Index_Anonymous_ReturnsAViewResult()
        {
            // Arrange
            // Mock the controller context to simulate an anonymous user
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext {User = new ClaimsPrincipal(new ClaimsIdentity())}
            };

            // Mock the API service to return a valid SpotPrice object
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/EUR"))
                .ReturnsAsync(new SpotPrice
                {
                    Name = "Gold",
                    UpdatedAtReadable = "a few minutes ago",
                    UpdatedAt = DateTime.UtcNow,
                    CurrencySymbol = "€",
                    ExchangeRate = 1.00f,
                    Symbol = "XAU",
                    Currency = "EUR",
                    Price = 3500.00f
                });

            // Mock the Dropdown Options service to return a successful run
            _dropdownOptionsServiceMock
                .Setup(s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Index();

            // Assert
            // Confirm unauthenticated user
            Assert.False(_controller.User.Identity?.IsAuthenticated);

            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<HomeViewModel>(viewResult.Model);

            Assert.Equal("XAU", model.SelectedMetal);
            Assert.Equal("EUR", model.SelectedCurrency);
            Assert.NotNull(model.SelectedSpotPrice);
            Assert.Equal("Gold", model.SelectedSpotPrice.Name);

            Assert.Empty(model.ErrorMessage);

            _dropdownOptionsServiceMock.Verify(
                s => s.PopulateAsync(It.IsAny<HomeViewModel>()),
                Times.Once());
        }

        // Index - happy path - authenticated user - Displays spot prices and defaults user's favourite values
        [Fact]
        public async Task Index_Authenticated_ReturnsAViewResult()
        {
            // Arrange
            // Set up a mock user for the controller context
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),
                new Claim(ClaimTypes.Name, "Alice"),
                new Claim(ClaimTypes.Role, "Admin")
            },
            authenticationType: "TestAuth"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Mock the user manager to return a valid application user when GetUserAsync is called. 
            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser
                {
                    FirstName = "Alice",
                    LastName = "Doe",
                    FavouriteMetal = "XAG",
                    FavouriteCurrency = "USD"
                });

            // Mock the API service to return a valid SpotPrice object
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAG/USD"))
                .ReturnsAsync(new SpotPrice
                {
                    Name = "Silver",
                    UpdatedAtReadable = "a few minutes ago",
                    UpdatedAt = DateTime.UtcNow,
                    CurrencySymbol = "$",
                    ExchangeRate = 1.00f,
                    Symbol = "XAG",
                    Currency = "USD",
                    Price = 60.00f
                });

            // Mock the Dropdown Options service to return a successful run
            _dropdownOptionsServiceMock
                .Setup(s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Index();

            // Assert
            // Confirm user is authenticated
            Assert.True(_controller.User.Identity?.IsAuthenticated);

            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);   
            var model = Assert.IsType<HomeViewModel>(viewResult.Model);

            Assert.Equal("XAG", model.SelectedMetal);
            Assert.Equal("USD", model.SelectedCurrency);
            Assert.NotNull(model.SelectedSpotPrice);
            Assert.Equal("Silver", model.SelectedSpotPrice.Name);
            Assert.Empty(model.ErrorMessage);

            _dropdownOptionsServiceMock.Verify(
                s => s.PopulateAsync(It.IsAny<HomeViewModel>()),
                Times.Once);
        }

        // Index - Exception - Failed to retrieve the user profile - reverts to standard defaults, displays message 
        [Fact]
        public async Task Index_Exception_ReturnsAViewResultAndException()
        {
            // Arrange
            // Mock the Dropdown Options service to complete successfully
            _dropdownOptionsServiceMock
                .Setup(s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()))
                .Returns(Task.CompletedTask);

            // Set up a mock user for the controller context
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),
                new Claim(ClaimTypes.Name, "Alice"),
                new Claim(ClaimTypes.Role, "Admin")
            },
            authenticationType: "TestAuth"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Mock the user manager to return an exception when GetUserAsync is called. 
            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ThrowsAsync(new UserProfileErrorException("An error occurred while fetching the user."));

            // Mock the API service to return a valid SpotPrice object
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/EUR"))
                .ReturnsAsync(new SpotPrice
                {
                    Name = "Gold",
                    UpdatedAtReadable = "a few minutes ago",
                    UpdatedAt = DateTime.UtcNow,
                    CurrencySymbol = "€",
                    ExchangeRate = 1.00f,
                    Symbol = "XAU",
                    Currency = "EUR",
                    Price = 3500.00f
                });

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<HomeViewModel>(viewResult.Model);

            // Confirm standard defaults displayed
            Assert.Equal("XAU", model.SelectedMetal);
            Assert.Equal("EUR", model.SelectedCurrency);
            Assert.NotNull(model.SelectedSpotPrice);
            Assert.Equal("Gold", model.SelectedSpotPrice.Name);

            Assert.Equal("Apologies, your profile information is currently unavailable so your favourite selections cannot be defaulted. " +
                "Please use the dropdowns above.", model.ErrorMessage);

            _dropdownOptionsServiceMock.Verify(
                s => s.PopulateAsync(It.IsAny<HomeViewModel>()),
                Times.Once);
        }

        // Index - Exception - Failed to retrieve spot price - Displays error message
        [Fact]
        public async Task Index_Exception_ReturnsAnErrorMessage()
        {
            // Arrange

            // Mock the API service to return an exception when trying to get the SpotPrice
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/EUR"))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
            var model = Assert.IsType<HomeViewModel>(viewResult.Model);

            Assert.Equal("The price site is unavailable at the moment. Please try again later.", model.ErrorMessage);
        }

        // Index - happy path - GetSpotPriceAsync Method - Returns SpotPriceResponse DTO
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
                    CurrencySymbol = "€",
                    ExchangeRate = 1.00f,
                    Symbol = "XAU",
                    Currency = "EUR",
                    Price = 3500.00f
                });

            // Act
            var result = await _controller.GetSpotPriceAsync("XAU","EUR");

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var response = Assert.IsType<SpotPriceResponse>(json.Value);
            Assert.NotNull(response);
            Assert.Equal(3500.00f,response.Price);
            Assert.Equal("€", response.CurrencySymbol);
        }

        // Index - Exception - GetSpotPriceAsync Method - Returns error message    
        [Fact]
        public async Task GetSpotPriceAsync_ReturnsAnErrorMessage()
        {
            // Arrange

            // Mock the API service to return an exception when trying to get the SpotPrice
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/USD"))
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

    
