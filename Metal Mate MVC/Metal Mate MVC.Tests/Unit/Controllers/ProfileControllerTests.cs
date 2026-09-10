using Metal_Mate_MVC.Controllers;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Models.ViewModels;
using Metal_Mate_MVC.Services;
using Metal_Mate_MVC.Tests.Unit.SetUp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;


namespace Metal_Mate_MVC.Tests
{
    public class ProfileControllerTests
    {

        private readonly Mock<ILogger<ProfileController>> _loggerMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IDropdownOptionsService> _dropdownOptionsServiceMock;

        private readonly ProfileController _controller;

        // Constructor to set up the mocks and the controller
        public ProfileControllerTests()
        {
            _loggerMock = new Mock<ILogger<ProfileController>>();
            _userManagerMock = SetUpMocks.CreateUserManagerMock();
            _dropdownOptionsServiceMock = new Mock<IDropdownOptionsService>();

            _controller = CreateProfileController(
                            _loggerMock.Object,
                            _userManagerMock.Object,
                            _dropdownOptionsServiceMock.Object
                            );
        }

        // Edit - Get - happy path - Displays authenticated user's profile and populates dropdowns for metals and currencies
        [Fact]
        public async Task Edit_Get_ReturnsAViewResult()
        {
            // Arrange
            // Mock the user manager to return a valid application user when GetUserAsync is called. 
            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(new ApplicationUser
                {
                    FirstName = "John",
                    LastName = "Doe",
                    FavouriteMetal = "XAU",
                    FavouriteCurrency = "USD"
                });

            // Mock the Dropdown options service to complete successfully when PopulateAsync is called.
            _dropdownOptionsServiceMock
                .Setup(s => s.PopulateAsync(It.IsAny<AlertRequestViewModel>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var model = Assert.IsType<EditProfileViewModel>(viewResult.Model);

            Assert.Equal("John", model.FirstName);
            Assert.Equal("Doe", model.LastName);
            Assert.Equal("XAU", model.FavouriteMetal);
            Assert.Equal("USD", model.FavouriteCurrency);

            _dropdownOptionsServiceMock.Verify(
                s => s.PopulateAsync(It.IsAny<EditProfileViewModel>()),
                Times.Once);
        }

        // Edit - Get - Exception thrown from the service when calling the GetAPIDataAsync Method for metals
        [Fact]
        public async Task Edit_Get_ReturnsAnErrorResult()
        {
            // Arrange
            var model = new EditProfileViewModel();

            // Mock the dropdown options service to return an exception
            _dropdownOptionsServiceMock
                .Setup(s => s.PopulateAsync(It.IsAny<EditProfileViewModel>()))
                .ThrowsAsync(new Exception("An error ocurred"));

            // Mock the user manager to return a valid application user when GetUserAsync is called. 
            _userManagerMock
                .Setup(s => s.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(SetUpTestDB.CreateUser(1));

            // Act
            var result = await _controller.Edit();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            model = Assert.IsType<EditProfileViewModel>(viewResult.Model);

            Assert.Equal("Your profile information is temporarily unavailable. Please try again later.", model.ErrorMessage);

            _userManagerMock.Verify(
               s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
               Times.Once);

            _dropdownOptionsServiceMock.Verify(
                s => s.PopulateAsync(It.IsAny<EditProfileViewModel>()),
                Times.Once);

        }

        // Edit - Post - happy path - Sets success message in TempData and redirects to the Edit page
        [Fact]
        public async Task Edit_Post_ReturnsRedirectAndSetsTempData()
        {
            // Arrange
            var testUser = SetUpTestDB.CreateUser(1);

            // Mock for GetUserAsync to return the test user
            _userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            // Mock for UpdateAsync to return IdentityResult.Success
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Initialize TempData so TempData["Success"] won't be null
            _controller.TempData = new TempDataDictionary(_controller.ControllerContext.HttpContext, Mock.Of<ITempDataProvider>());

            var model = new EditProfileViewModel();

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.NotNull(result);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(_controller.Edit), redirect.ActionName);

            // TempData contains the success key
            Assert.Equal("Profile updated successfully.", _controller.TempData["Success"] as string);
        }


        // Edit - Post - Exception thrown when calling the UpdateAsync method of the UserManager
        [Fact]
        public async Task Edit_Post_ReturnsError()
        {
            // Arrange
            var testUser = SetUpTestDB.CreateUser(1);

            // Mock for GetUserAsync to return the test user
            _userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            // Mock for UpdateAsync to throw an exception
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ThrowsAsync(new Exception("An error ocurred"));

            var model = new EditProfileViewModel();

            // Act
            var result = await _controller.Edit(model);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            model = Assert.IsType<EditProfileViewModel>(viewResult.Model);

            Assert.Equal("An error occurred while updating your profile. Please try again later.", model.ErrorMessage);

            // TempData is empty
            Assert.Null(_controller.TempData);

            _userManagerMock.Verify(
               s => s.GetUserAsync(It.IsAny<ClaimsPrincipal>()),
               Times.Once);

            _userManagerMock.Verify(
               s => s.UpdateAsync(It.IsAny<ApplicationUser>()),
               Times.Once);
        }

        //---------------------------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------------------------



        private static ProfileController CreateProfileController(ILogger<ProfileController> logger,
                                                                 UserManager<ApplicationUser> userManager,
                                                                 IDropdownOptionsService dropdownOptionsService
                                                                 )
        {
            var controller = new ProfileController(
                            logger,
                            userManager,
                            dropdownOptionsService
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
