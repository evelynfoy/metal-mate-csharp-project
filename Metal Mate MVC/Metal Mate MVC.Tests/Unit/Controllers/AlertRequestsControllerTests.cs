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

namespace Metal_Mate_MVC.Tests
{
    public class AlertRequestsControllerTests
    {
        private readonly Mock<ILogger<AlertRequestsController>> _loggerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IAlertRequestService> _alertRequestServiceMock;
        private readonly ApplicationDbContext _context;

        private readonly AlertRequestsController _controller;

        public AlertRequestsControllerTests()
        {
            _loggerMock = new Mock<ILogger<AlertRequestsController>>();
            _userManagerMock = SetUpMocks.CreateUserManagerMock();
            _alertRequestServiceMock = new Mock<IAlertRequestService>();
            _context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().Options);

            _controller = CreateAlertRequestController(
                            _context,
                            _loggerMock.Object,
                            _userManagerMock.Object,
                            _alertRequestServiceMock.Object);
        }

        // Mocked response - happy path - Authenticated user with alert requests
        [Fact]
        public async Task Index_ReturnsOnlyCurrentUsersAlertRequests()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "user1"
            };

            _userManagerMock
                .Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var alertRequests = new List<AlertRequest>
            {
                new AlertRequest { UserId = "user1" },
                new AlertRequest { UserId = "user1" }
            };

            _alertRequestServiceMock
                .Setup(x => x.GetForUserAsync("user1"))
                .ReturnsAsync(alertRequests);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<AlertRequestIndexViewModel>(
                viewResult.Model);

            Assert.Equal(2, model.AlertRequests.Count);

            Assert.All(
                model.AlertRequests,
                request => Assert.Equal("user1", request.UserId));

            Assert.Empty(model.ErrorMessage);

            _alertRequestServiceMock.Verify(
                x => x.GetForUserAsync("user1"),
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


        private static AlertRequestsController CreateAlertRequestController(ApplicationDbContext context,
                                                                            ILogger<AlertRequestsController> logger,
                                                                            UserManager<ApplicationUser> userManager,
                                                                            IAlertRequestService alertRequestService)
        {
            var controller = new AlertRequestsController(
                            context,
                            logger,
                            userManager,
                            alertRequestService);

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

    
