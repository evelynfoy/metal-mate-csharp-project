using Metal_Mate_MVC.Controllers;
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
    public class AlertRequestsControllerTests
    {
        private readonly Mock<ILogger<AlertRequestsController>> _loggerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IAlertRequestService> _alertRequestServiceMock;
        private readonly Mock<IApiService> _apiServiceMock;
        private readonly Mock<IDropdownOptionsService> _dropdownOptionsServiceMock;

        private readonly AlertRequestsController _controller;

        public AlertRequestsControllerTests()
        {
            _loggerMock = new Mock<ILogger<AlertRequestsController>>();
            _userManagerMock = SetUpMocks.CreateUserManagerMock();
            _alertRequestServiceMock = new Mock<IAlertRequestService>();
            _apiServiceMock = new Mock<IApiService>();
            _dropdownOptionsServiceMock = new Mock<IDropdownOptionsService>();

            _controller = CreateAlertRequestController(
                            _loggerMock.Object,
                            _userManagerMock.Object,
                            _alertRequestServiceMock.Object,
                            _dropdownOptionsServiceMock.Object,
                            _apiServiceMock.Object
                            );
        }

//---------------------------------------------------------------------------------------------------------------
// Index Tests
//---------------------------------------------------------------------------------------------------------------

        // Mocked response - happy path - Displays authenticated user's alert requests
        [Fact]
        public async Task Index_ReturnsOnlyCurrentUsersAlertRequests()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return a list with one alert request for the authenticated user
            var alertRequest = SetUpTestDB.CreateUserAlertRequest(user);
            var alertRequests = new List<AlertRequest> { alertRequest };

            _alertRequestServiceMock
                .Setup(s => s.GetForUserAsync("user1"))
                .ReturnsAsync(alertRequests);

            // Mock the API service to return a valid list of metals
            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" },
             });


            // Act
            var result = await _controller.Index();


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestIndexViewModel>(viewResult.Model);

            Assert.Single<AlertRequest>(model.AlertRequests);
            Assert.NotEmpty(model.MetalNames);
            Assert.Empty(model.ErrorMessage);

            _userManagerMock.Verify(
               s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
               Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetForUserAsync(It.IsAny<string>()),
                Times.Once);

            _apiServiceMock.Verify(
                s => s.GetAPIDataAsync<List<Metal>>("symbols"),
                Times.Once);
        }

        // Mocked response - happy path - Authenticated user with no requests   
        [Fact]
        public async Task Index_ReturnsEmptyView()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return an empty list 
            _alertRequestServiceMock
                .Setup(s => s.GetForUserAsync("user1"))
                .ReturnsAsync(new List<AlertRequest> { });

            // Mock the API service to return a valid list of metals
            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" },
             });

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestIndexViewModel>(viewResult.Model);

            Assert.Empty(model.AlertRequests);
            Assert.NotEmpty(model.MetalNames);
            Assert.Empty(model.ErrorMessage);

            _userManagerMock.Verify(
               s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
               Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetForUserAsync(It.IsAny<string>()),
                Times.Once);

            _apiServiceMock.Verify(
                s => s.GetAPIDataAsync<List<Metal>>("symbols"),
                Times.Once);
        }

        // Mocked response - Fails to retrieve user details - Error message is displayed to the user 
        [Fact]
        public async Task Index_UserIsNull_ReturnsEmptyViewWithErrorMessage()
        {
            // Arrange
            // Mock user manager to return a null application user instance
            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestIndexViewModel>(viewResult.Model);

            Assert.Empty(model.AlertRequests);
            Assert.Empty(model.MetalNames);
            Assert.Equal(
                "Your alert requests are temporarily unavailable. Please try again later.",
                model.ErrorMessage);

            _loggerMock.Verify(
               s => s.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                   Times.Once);
        }

        // Mocked response - Exception - Error message is displayed to the user 
        [Fact]
        public async Task Index_Exception_ReturnsEmptyViewWithErrorMessage()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return an exception
            _alertRequestServiceMock
                .Setup(s => s.GetForUserAsync("user1"))
                .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestIndexViewModel>(
                viewResult.Model);

            Assert.Empty(model.AlertRequests);
            Assert.Empty(model.MetalNames);
            Assert.Equal("Your alert requests are temporarily unavailable. Please try again later.", model.ErrorMessage);

            _userManagerMock.Verify(
               s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
               Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetForUserAsync("user1"),
                Times.Once);

            _loggerMock.Verify(
                     s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex.Message == "Database failure"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        //---------------------------------------------------------------------------------------------------------------
        // Create Tests
        //---------------------------------------------------------------------------------------------------------------

        // Get - happy path - Display blank new alert request page 
        [Fact]
        public async Task Create_Get_ReturnsEmptyView()
        {
            // Arrange
            // Mock the Dropdown Options service to return a successful run
            _dropdownOptionsServiceMock
                .Setup(s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal(0,model.Id);
            Assert.Empty(model.Metal);
            Assert.Empty(model.Currency);
            Assert.Equal(ComparisonOperator.LessThan, model.Operator);
            Assert.Equal(0, model.Value);
            Assert.True(model.IsEnabled);

            Assert.Empty(model.ErrorMessage);

            _dropdownOptionsServiceMock.Verify(
                s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()),
                Times.Once);
        }

        // Get - Dropdown options service returns an exception - Error message is displayed to the user 
        [Fact]
        public async Task Create_Get_Exception_ReturnViewWithError()
        {
            // Arrange
            // Mock the Dropdown Options service to return an exception
            _dropdownOptionsServiceMock
                .Setup(s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Act
            var result = await _controller.Create();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);  
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal(0, model.Id);
            Assert.Empty(model.Metal);
            Assert.Empty(model.Metals);
            Assert.Empty(model.Currency);
            Assert.Empty(model.Currencies);
            Assert.Equal(ComparisonOperator.LessThan, model.Operator);
            Assert.Equal(0, model.Value);
            Assert.True(model.IsEnabled);

            Assert.Equal("There was a problem retrieving the information for this page. Please try again later.", model.ErrorMessage);

            _dropdownOptionsServiceMock.Verify(
                s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()),
                Times.Once);

            _loggerMock.Verify(
               s => s.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                   It.IsAny<Exception>(),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                   Times.Once);
        }

        // Post - happy path - Request added and user redirected to Index
        [Fact]
        public async Task Create_Post_RedirectsToIndex()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var model = new AlertRequestViewModel();
            model.Id = 1;
            model.Value = 1000;
            model.Currency = "USD";
            model.Metal = "XAU";

            // Mock alert request service to complete successfully when AddAsync is called
            _alertRequestServiceMock.Setup(s => s.AddAsync(It.IsAny<AlertRequest>()))
                .Returns(Task.CompletedTask);


            // Act
            var result = await _controller.Create(model);


            // Assert
            Assert.NotNull(result);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AlertRequestsController.Index), redirect.ActionName);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                    s => s.AddAsync(It.IsAny<AlertRequest>()),
                    Times.Once);
        }

        // Post - Fails to retrieve user details - Fails to retrieve user details
        [Fact]
        public async Task Create_Post_NullUser()
        {
            // Arrange
            // Mock user manager to return a null application user instance
            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            var model = new AlertRequestViewModel();
            model.Id = 1;
            model.Value = 4000;
            model.Currency = "USD";
            model.Metal = "XAU";

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem saving this entry. Please try again.", model.ErrorMessage);

            _userManagerMock.Verify(
                s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Post - Exception thrown - Error message is displayed to the user
        [Fact]
        public async Task Create_Post_Error()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to throw an exception when AddAsync is called
            _alertRequestServiceMock
                .Setup(s => s.AddAsync(It.IsAny<AlertRequest>()))
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
            Assert.NotNull(viewResult.Model);
            model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("Your alert request did not save successfully. Please try again later.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                 s => s.AddAsync(It.IsAny<AlertRequest>()),
                 Times.Once);

            _loggerMock.Verify(
               s => s.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex.Message == "Database failure"),
                   It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                   Times.Once);
        }

//---------------------------------------------------------------------------------------------------------------
// Edit Tests
//---------------------------------------------------------------------------------------------------------------


        // Edit - Get - happy path - Displays selected alert request
        [Fact]
        public async Task Edit_Get_ReturnsPopulatedAlertRequestView()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return an alert request for the authenticated user
            var alertRequest = SetUpTestDB.CreateUserAlertRequest(user);
            alertRequest.Id = 1;

            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(1, user.Id))
                .ReturnsAsync(alertRequest);

            // Mock the Dropdown Options service to return a successful run
            _dropdownOptionsServiceMock
                .Setup(s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()))
                .Returns(Task.CompletedTask);


            // Act
            var result = await _controller.Edit(alertRequest.Id);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal(1, model.Id);
            Assert.Equal(alertRequest.Currency, model.Currency);
            Assert.Equal(alertRequest.Metal, model.Metal);
            Assert.Equal(alertRequest.Value, model.Value);
            Assert.Equal(alertRequest.Operator, model.Operator);
            Assert.Equal(alertRequest.IsEnabled, model.IsEnabled);

            Assert.Empty(model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _dropdownOptionsServiceMock.Verify(
                s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()),
                Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(alertRequest.Id, user.Id),
                Times.Once);
        }

        // Edit - Get - Null Id passed - Displays model showing error
        [Fact]
        public async Task Edit_Get_NullId_ReturnsModelViewWithError()
        {
            // Arrange
            int? id = null;

            // Act
            var result = await _controller.Edit(id);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Edit - Get - Fails to retrieve user details - Displays model showing error
        [Fact]
        public async Task Edit_Get_NullUser_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock user manager to return a null application user instance
            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            // Pass a random number to the Edit action
            var result = await _controller.Edit(1);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Edit - Get - Null alert request retrieved - Displays model showing error
        // Covers the case where the user is authenticated but the alert request id does not belong to that user.
        [Fact]
        public async Task Edit_Get_NullAlertRequest_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return null for the given id and user id
            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(1, user.Id))
                .ReturnsAsync((AlertRequest?)null);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);

            var model = Assert.IsType<AlertRequestViewModel>(
                viewResult.Model);

            Assert.Equal(0, model.Id);
            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(1, user.Id),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Edit - Get - Exception occurs - Displays model showing error
        [Fact]
        public async Task Edit_Get_Exception_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(1, user.Id))
                .ThrowsAsync(new Exception("The service failed to retrieve the alert request."));

            // Act
            var result = await _controller.Edit(1);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(
                viewResult.Model);

            Assert.Equal(0, model.Id);
            Assert.Equal("There was a problem retrieving the information for this page. Please try again later.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(1, user.Id),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex.Message == "The service failed to retrieve the alert request."),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }


//---------------------------------------------------------------------------------------------------------------
// Edit Post Tests
//---------------------------------------------------------------------------------------------------------------

        // Edit - Post - happy path - Saves changed alert request
        [Fact]
        public async Task Edit_Post_ReturnsRedirectToIndex()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Set up a valid AlertRequestViewModel to pass to the Edit action
            var model = new AlertRequestViewModel();
            model.Id = 1;
            model.Value = 1000;
            model.Currency = "USD";
            model.Metal = "XAU";
            model.IsEnabled = true;
            model.Operator = ComparisonOperator.GreaterThan;

            // Mock alertRequestService to return an alert request for the authenticated user
            // and to complete successfully when SaveAsync is called
            var alertRequest = SetUpTestDB.CreateUserAlertRequest(user);
            alertRequest.Id = 1;

            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(1, user.Id))
                .ReturnsAsync(alertRequest);

            _alertRequestServiceMock
                .Setup(s => s.SaveAsync(It.IsAny<AlertRequest>()))
                .Returns(Task.CompletedTask);


            // Act
            var result = await _controller.Edit(model.Id, model);


            // Assert
            Assert.NotNull(result);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AlertRequestsController.Index), redirect.ActionName);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(alertRequest.Id, user.Id),
                Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.SaveAsync(It.IsAny<AlertRequest>()),
                Times.Once);
        }

        // Edit - Post - different id - Displays model showing error
        [Fact]
        public async Task Edit_Post_IdNotMatchModel_ReturnsModelViewWithError()
        {
            // Arrange
            // Set up parameters for the Edit action
            int id = 3;

            var model = new AlertRequestViewModel();
            model.Id = 1;


            // Act
            var result = await _controller.Edit(id, model);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            model = Assert.IsType<AlertRequestViewModel>(
                viewResult.Model);

            Assert.Equal("There was a problem saving this entry. Please try again.", model.ErrorMessage);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Edit - Post - Fails to retrieve user details - Displays model showing error
        [Fact]
        public async Task Edit_Post_NullUser_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Set up parameters for the Edit action
            var alertRequest = SetUpTestDB.CreateUserAlertRequest(user);
            alertRequest.Id = 1;

            var model = new AlertRequestViewModel();
            model.Id = 1;
            model.Value = 1000;
            model.Currency = "USD";
            model.Metal = "XAU";
            model.IsEnabled = true;
            model.Operator = ComparisonOperator.GreaterThan;


            // Act
            var result = await _controller.Edit(alertRequest.Id, model);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Edit - Post - Null alert request retrieved - Displays model showing error
        // Covers the case where the user is authenticated but the alert request id does not belong to that user.
        [Fact]
        public async Task Edit_Post_NullAlertRequest_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return null for the given id and user id
            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<int>(), user.Id))
                .ReturnsAsync((AlertRequest?)null);

            // Act
            // Pass a random number to the Edit action
            var result = await _controller.Edit(1);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(1, user.Id),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Edit - Post - Exception - Displays model showing error
        [Fact]
        public async Task Edit_Post_Exception_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return an alert request for the authenticated user
            var alertRequest = SetUpTestDB.CreateUserAlertRequest(user);
            alertRequest.Id = 1;

            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(alertRequest.Id, user.Id))
                .ReturnsAsync(alertRequest);

            var model = new AlertRequestViewModel();
            model.Id = alertRequest.Id;

            // Mock the alert request service to throw an exception when SaveAsync is called
            _alertRequestServiceMock
                .Setup(s => s.SaveAsync(It.IsAny<AlertRequest>()))
                .ThrowsAsync(new Exception("The service failed to save the alert request."));
                

            // Act
            var result = await _controller.Edit(alertRequest.Id, model);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem saving the information for this page. Please try again later.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(alertRequest.Id, user.Id),
                Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.SaveAsync(It.IsAny<AlertRequest>()),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex.Message == "The service failed to save the alert request."),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);

        }

//---------------------------------------------------------------------------------------------------------------
// Details Get Tests
//---------------------------------------------------------------------------------------------------------------

        // Details - Get - happy path - Displays selected alert request
        [Fact]
        public async Task Details_Get_ReturnsPopulatedAlertRequestView()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return an alert request for the authenticated user
            var alertRequest = SetUpTestDB.CreateUserAlertRequest(user);
            alertRequest.Id = 1;

            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(1, user.Id))
                .ReturnsAsync(alertRequest);


            // Act
            var result = await _controller.Details(alertRequest.Id);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            // Check that the model properties match the alertRequest properties
            Assert.Equal(alertRequest.Id, model.Id);
            Assert.Equal(alertRequest.Metal, model.Metal);
            Assert.Equal(alertRequest.Operator, model.Operator);
            Assert.Equal(alertRequest.Value, model.Value);
            Assert.Equal(alertRequest.IsEnabled, model.IsEnabled);

            Assert.Empty(model.Metals);
            Assert.Empty(model.Currencies);
            Assert.Empty(model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(alertRequest.Id , user.Id),
                Times.Once);
        }

        // Details - Get - null id passed - Displays model showing error
        [Fact]
        public async Task Details_Get_NullId_ReturnsModelViewWithError()
        {
            // Arrange
            int? id = null;

            // Act
            var result = await _controller.Details(id);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Details - Get - Fails to retrieve user details - Displays model showing error
        [Fact]
        public async Task Details_Get_NullUser_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);


            // Act
            var result = await _controller.Details(1);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Details - Get - null alert request - Displays model showing error
        [Fact]
        public async Task Details_Get_NullAlertRequest_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(1, user.Id))
                .ReturnsAsync((AlertRequest?)null);


            // Act
            var result = await _controller.Details(1);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);


            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(1, user.Id),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Details - Get - Exception - Displays model showing error
        [Fact]
        public async Task Details_Get_Exception_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(1, user.Id))
                .ThrowsAsync(new Exception("The service failed to retrieve the alert request."));

            // Act
            var result = await _controller.Details(1);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem retrieving the information for this page. Please try again later.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(1, user.Id),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex.Message == "The service failed to retrieve the alert request."),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }


//---------------------------------------------------------------------------------------------------------------
// Delete Tests
//---------------------------------------------------------------------------------------------------------------


        // Delete - Get - happy path - Displays selected alert request
        [Fact]
        public async Task Delete_Get_ReturnsPopulatedAlertRequestView()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup( s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return an alert request for the authenticated user
            var alertRequest = SetUpTestDB.CreateUserAlertRequest(user);
            alertRequest.Id = 1;

            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(1, user.Id))
                .ReturnsAsync(alertRequest);

            // Act
            var result = await _controller.Delete(alertRequest.Id);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            // Check that the model properties match the alertRequest properties
            Assert.Equal(alertRequest.Id, model.Id);
            Assert.Equal(alertRequest.Metal, model.Metal);
            Assert.Equal(alertRequest.Operator, model.Operator);
            Assert.Equal(alertRequest.Value, model.Value);
            Assert.Equal(alertRequest.IsEnabled, model.IsEnabled);

            Assert.Empty(model.Metals);
            Assert.Empty(model.Currencies);
            Assert.Empty(model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(alertRequest.Id, user.Id),
                Times.Once);

        }

        // Delete - Get - null id passed- Displays model showing error
        [Fact]
        public async Task Delete_Get_NullId_ReturnsModelViewWithError()
        {
            // Arrange
            int? id = null;

            // Act
            var result = await _controller.Delete(id);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Delete - Get - Fails to retrieve user details - Displays model showing error
        [Fact]
        public async Task Delete_Get_NullUser_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);


            // Act
            var result = await _controller.Delete(1);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Delete - Get - null alert request - Displays model showing error
        [Fact]
        public async Task Delete_Get_NullAlertRequest_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(1, user.Id))
                .ReturnsAsync((AlertRequest?)null);


            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem loading this entry. Please try again.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(1, user.Id),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

        // Delete - Get - Exception - Displays model showing error
        [Fact]
        public async Task Delete_Get_Exception_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            //Mock the alert request service to throw an exception when GetByIdAsync is called
            _alertRequestServiceMock
                .Setup(s => s.GetByIdAsync(It.IsAny<int>(), user.Id))
                .ThrowsAsync(new Exception("The service failed to retrieve the alert request."));


            // Act
            // Call the Delete action with a random number
            var result = await _controller.Delete(1);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem retrieving the information for this page. Please try again later.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.GetByIdAsync(It.IsAny<int>(), user.Id),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex.Message == "The service failed to retrieve the alert request."),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }

//---------------------------------------------------------------------------------------------------------------
// Delete Post Tests
//---------------------------------------------------------------------------------------------------------------

        // Delete Confirmed - happy path - Deletes alert request
        [Fact]
        public async Task DeleteConfirmed_ReturnsRedirectToIndex()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup( s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to return true when DeleteAsync is called
            _alertRequestServiceMock
                .Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(true);


            // Act
            // Call the DeleteConfirmed action passing a random number
            var result = await _controller.DeleteConfirmed(1);


            // Assert
            Assert.NotNull(result);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AlertRequestsController.Index), redirect.ActionName);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<string>()),
                Times.Once);
        }

        // Delete Confirmed - Exception - Displays model showing error
        [Fact]
        public async Task DeleteConfirmed_Exception_ReturnsModelViewWithError()
        {
            // Arrange
            // Mock authenticated user - user1
            var user = new ApplicationUser { Id = "user1" };

            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Mock the alert request service to throw an exception when DeleteAsync is called
            _alertRequestServiceMock
                .Setup(s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("The service failed to delete the alert request."));


            // Act
            // Call the DeleteConfirmed action passing a random number
            var result = await _controller.DeleteConfirmed(1);


            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<AlertRequestViewModel>(viewResult.Model);

            Assert.Equal("There was a problem deleting this entry. Please try again later.", model.ErrorMessage);

            _userManagerMock.Verify(
                 s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
                 Times.Once);

            _alertRequestServiceMock.Verify(
                s => s.DeleteAsync(It.IsAny<int>(), It.IsAny<string>()),
                Times.Once);

            _loggerMock.Verify(
                s => s.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex.Message == "The service failed to delete the alert request."),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
        }


//---------------------------------------------------------------------------------------------------------------
//---------------------------------------------------------------------------------------------------------------



        private static AlertRequestsController CreateAlertRequestController(ILogger<AlertRequestsController> logger,
                                                                            UserManager<ApplicationUser> userManager,
                                                                            IAlertRequestService alertRequestService,
                                                                            IDropdownOptionsService dropdownOptionsService,
                                                                            IApiService apiService
                                                                            )
        {
            var controller = new AlertRequestsController(
                            logger,
                            userManager,
                            alertRequestService,
                            dropdownOptionsService,
                            apiService
                            );

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

    
