using Metal_Mate_MVC.Controllers;
using Metal_Mate_MVC.Data;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Models.ViewModels;
using Metal_Mate_MVC.Services;
using Metal_Mate_MVC.Tests.Unit.SetUp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Metal_Mate_MVC.Tests
{
    public class AlertRequestsControllerTests
    {
        private readonly Mock<ILogger<AlertRequestsController>> _loggerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IAlertRequestService> _alertRequestServiceMock;
        private readonly ApplicationDbContext _context;
        private readonly Mock<IApiService> _apiServiceMock;

        private readonly AlertRequestsController _controller;

        public AlertRequestsControllerTests()
        {
            _loggerMock = new Mock<ILogger<AlertRequestsController>>();
            _userManagerMock = SetUpMocks.CreateUserManagerMock();
            _alertRequestServiceMock = new Mock<IAlertRequestService>();
            _context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
            _apiServiceMock = new Mock<IApiService>();

            _controller = CreateAlertRequestController(
                            _context,
                            _loggerMock.Object,
                            _userManagerMock.Object,
                            _alertRequestServiceMock.Object,
                            _apiServiceMock.Object);
        }

        // Mocked response - happy path - Authenticated user with alert requests
        [Fact]
        public async Task Index_ReturnsOnlyCurrentUsersAlertRequests()
        {
            // Arrange

            var alertRequest = new AlertRequest();

            // Mock the API service to return a valid list of metals
            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" }
             });

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<AlertRequestViewModel>(
                viewResult.Model);

            Assert.NotNull(model);
            Assert.NotNull(model.Metals);
            Assert.NotNull(model.Currencies);

            Assert.Empty(model.ErrorMessage);

            _apiServiceMock.Verify(
                x => x.GetAPIDataAsync<List<Metal>>("symbols"),
                Times.Once);
        }

        // Mocked response - happy path - Authenticated user with no requests   
        [Fact]
        public async Task Index_ReturnsEmptyView()
        {
            // Arrange

            var user = new ApplicationUser
            {
                Id = "user1"
            };

            _userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _alertRequestServiceMock
                .Setup(x => x.GetForUserAsync("user1"))
                .ReturnsAsync(new List<AlertRequest> { });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<AlertRequestIndexViewModel>(
                viewResult.Model);

            Assert.Empty(model.ErrorMessage);

            var requests = model.AlertRequests.ToList();

            Assert.Empty(requests);

        }

        // Mocked response - Error - database error while retrieving alert requests  - error message is displayed to the user 
        [Fact]
        public async Task Index_Exception_ReturnsEmptyViewWithErrorMessage()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user1"
            };

            _userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _alertRequestServiceMock
                .Setup(x => x.GetForUserAsync("user1"))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<AlertRequestIndexViewModel>(
                viewResult.Model);

            Assert.Empty(model.AlertRequests);
            Assert.Equal("Your alert requests are temporarily unavailable. Please try again later.", model.ErrorMessage);

            _alertRequestServiceMock.Verify(
                x => x.GetForUserAsync("user1"),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex.Message == "Database failure"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

        }

        // Mocked response - UserIsNull - error message is displayed to the user 
        [Fact]
        public async Task Index_UserIsNull_ReturnsEmptyViewWithErrorMessage()
        {
            // Arrange
            _userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<AlertRequestIndexViewModel>(
                viewResult.Model);

            Assert.Empty(model.AlertRequests);
            Assert.Equal(
                "Your alert requests are temporarily unavailable. Please try again later.",
                model.ErrorMessage);

            _loggerMock.Verify(
               x => x.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);

        }

        // Create - Get - Mocked response from apiService - happy path - Display blank new alert request page
        [Fact]
        public async Task Create_Get_ReturnsEmptyView()
        {
            // Arrange
            // Mock the API service to return a valid list of metals
            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" }
             });

            // Act
            var result = await _controller.Create();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<AlertRequestViewModel>(
                viewResult.Model);

            Assert.Equal(0,model.Id);
            Assert.NotNull(model.Metals);
            Assert.NotNull(model.Currencies);
            Assert.Empty(model.Metal);
            Assert.Empty(model.Currency);
            Assert.Equal(ComparisonOperator.LessThan, model.Operator);
            Assert.Equal(0, model.Value);
            Assert.False(model.IsEnabled);

            Assert.Empty(model.ErrorMessage);

            _apiServiceMock.Verify(
                s => s.GetAPIDataAsync<List<Metal>>("symbols"),
                Times.Once);
        }

        // Create - Get - Mocked response from API service to return an exception  
        [Fact]
        public async Task Create_Get_Exception_ReturnsError()
        {
            // Arrange

            // Mock the API service to return an exception when trying to get the list of metals
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Act
            var result = await _controller.Create();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<AlertRequestViewModel>(
                viewResult.Model);

            Assert.Equal(0, model.Id);
            Assert.Empty(model.Metals!);
            Assert.Empty(model.Currencies);
            Assert.Empty(model.Metal);
            Assert.Empty(model.Currency);
            Assert.Equal(ComparisonOperator.LessThan, model.Operator);
            Assert.Equal(0, model.Value);
            Assert.False(model.IsEnabled);

            Assert.Equal("There was a problem retrieving the informationfor this page. Please try again later.", model.ErrorMessage);

            _apiServiceMock.Verify(
                s => s.GetAPIDataAsync<List<Metal>>("symbols"),
                Times.Once);
        }

        // Create - Post - Mocked response from API and User Manager - happy path - User redirected to Index
        [Fact]
        public async Task Create_Post_RedirectsToIndex()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user1"
            };

            _userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" }
             });

            var model = new AlertRequestViewModel();
            model.Id = 1;
            model.Value = 1000;
            model.Currency = "USD";
            model.Metal = "XAU";

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.NotNull(result);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AlertRequestsController.Index), redirect.ActionName);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _apiServiceMock.Verify(
                 s => s.GetAPIDataAsync<List<Metal>>("symbols"),
                 Times.Once);

        }

        // Create - Post - Mocked response from user manager and services - Exception thrown - Returns error message
        [Fact]
        public async Task Create_Post_Error()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user1"
            };

            _userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" }
             });

            var alertRequest = new AlertRequest();

            _alertRequestServiceMock
                .Setup(x => x.AddAsync(It.IsAny<AlertRequest>()))
                .ThrowsAsync(new Exception("Database failure"));

            var model = new AlertRequestViewModel();
            model.Id = 1;
            model.Value = 1000;
            model.Currency = "USD";
            model.Metal = "XAU";

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);
            Assert.Equal("Your alert request did not save successfully. Please try again later.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _apiServiceMock.Verify(
                 s => s.GetAPIDataAsync<List<Metal>>("symbols"),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                 s => s.AddAsync(It.IsAny<AlertRequest>()),
                 Times.Once);

        }

        // Create - Post - Mocked response from user manager - Null User - Returns error message
        [Fact]
        public async Task Create_Post_NullUser()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user1"
            };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            var model = new AlertRequestViewModel();
            model.Id = 1;
            model.Value = 1000;
            model.Currency = "USD";
            model.Metal = "XAU";

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);
            Assert.Equal("There was a problem saving this entry. Please try again.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

        }




        private static AlertRequestsController CreateAlertRequestController(ApplicationDbContext context,
                                                                            ILogger<AlertRequestsController> logger,
                                                                            UserManager<ApplicationUser> userManager,
                                                                            IAlertRequestService alertRequestService,
                                                                            IApiService apiService)
        {
            var controller = new AlertRequestsController(
                            context,
                            logger,
                            userManager,
                            alertRequestService,
                            apiService);

            // Set up an authenticated user
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user1")
            };

            var identity = new ClaimsIdentity(
                claims,
                "TestAuthentication");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };

            return controller;
        }
        
    }
}

    
