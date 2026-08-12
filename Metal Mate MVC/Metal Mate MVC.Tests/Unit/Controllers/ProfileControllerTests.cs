using Metal_Mate_MVC.Controllers;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;
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
        private readonly Mock<IApiService> _apiServiceMock;

        private readonly ProfileController _controller;

        // Constructor to set up the mocks and the controller
        public ProfileControllerTests()
        {
            // Set up a mock user store for the UserManager
            var store = new Mock<IUserStore<ApplicationUser>>();

            _loggerMock = new Mock<ILogger<ProfileController>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                               Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _apiServiceMock = new Mock<IApiService>();

            // Set up a mock user for the controller context
            var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "123"),
                new Claim(ClaimTypes.Name, "Alice"),
                new Claim(ClaimTypes.Role, "Admin")
            },
            authenticationType: "TestAuth"));

            _controller = new ProfileController(
                _loggerMock.Object,
                _userManagerMock.Object,
                _apiServiceMock.Object);

            _controller.ControllerContext = new ControllerContext
            { 
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // Mocked response - happy path
        [Fact]
        public async Task Edit_ReturnsAViewResult()
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

            // Mock the API service to return a valid list of metals
            _apiServiceMock
             .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
             .ReturnsAsync(new List<Metal>
             {
                 new Metal { Name = "Silver", Symbol = "XAG" },
                 new Metal { Name = "Gold", Symbol = "XAU" }
             });

            // Act
            var result = await _controller.Edit();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditProfileViewModel>(viewResult.Model);
            Assert.NotNull(model.FirstName);
            Assert.Equal("John", model.FirstName);
            Assert.NotNull(model.Metals);
            Assert.Equal("Gold", model.Metals.ElementAt(1).Text);
            Assert.NotNull(model.Currencies);
            Assert.Equal("AUD", model.Currencies.ElementAt(1).Text);
        }

        // Mocked response - Exception thrown from the service when calling the GetAPIDataAsync Method for metals
        [Fact]
        public async Task Edit_ReturnsAnErrorResult()
        {
            // Arrange

            // Mock the API service to return an exception when trying to get the list of metals
            _apiServiceMock
                .Setup(s => s.GetAPIDataAsync<List<Metal>>("symbols"))
                .ThrowsAsync(new Exception("An error ocurred"));

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

            // Act
            var result = await _controller.Edit();

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EditProfileViewModel>(viewResult.Model);

            Assert.NotNull(model.ErrorMessage);
            Assert.Null(model.Metals);
            Assert.Equal("Your profile information is temporarily unavailable. Please try again later.", model.ErrorMessage);

        }

        // Mocked response - Happy path for the Post Edit Method 
        [Fact]
        public async Task Edit_Post_ReturnsRedirectAndSetsTempData()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe",
                FavouriteMetal = "XAU",
                FavouriteCurrency = "USD",
                UserName = "jdoe",
                Id = "user-1"
            };

            // Mock for GetUserAsync to return the test user
            _userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            // Mock for UpdateAsync to return IdentityResult.Success
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new ProfileController(_loggerMock.Object, _userManagerMock.Object, _apiServiceMock.Object);

            // Give controller a HttpContext with an authenticated user
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, testUser.Id) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Initialize TempData so TempData["Success"] won't be null
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var model = new EditProfileViewModel
            {
                FirstName = "Jane",
                LastName = "Doe",
                FavouriteMetal = "XAG",
                FavouriteCurrency = "USD",
                Currencies = new List<SelectListItem> { new SelectListItem { Value = "USD", Text = "USD" } },
                Metals = new List<SelectListItem> { new SelectListItem { Value = "XAU", Text = "Gold" } }
            };

            // Act
            var result = await controller.Edit(model);

            // Assert
            Assert.NotNull(result);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Edit), redirect.ActionName);

            // TempData contains the success key
            Assert.Equal("Profile updated successfully.", controller.TempData["Success"] as string);
        }


        // Mocked response - Exception thrown when calling the UpdateAsync method of the UserManager in the Post Edit Method
        [Fact]
        public async Task Edit_Post_ReturnsError()
        {
            // Arrange
            var testUser = new ApplicationUser
            {
                FirstName = "John",
                LastName = "Doe",
                FavouriteMetal = "XAU",
                FavouriteCurrency = "USD",
                UserName = "jdoe",
                Id = "user-1"
            };

            // Mock for GetUserAsync to return the test user
            _userManagerMock
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(testUser);

            // Mock for UpdateAsync to throw an exception
            _userManagerMock
                .Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ThrowsAsync(new Exception("An error ocurred"));

            var controller = new ProfileController(_loggerMock.Object, _userManagerMock.Object, _apiServiceMock.Object);

            // Give controller a HttpContext with an authenticated user
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, testUser.Id) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = principal };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Prepare a model with changed data to simulate a user editing their profile
            var model = new EditProfileViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                FavouriteMetal = "XAU",
                FavouriteCurrency = "USD",
                Currencies = new List<SelectListItem> { new SelectListItem { Value = "USD", Text = "USD" } },
                Metals = new List<SelectListItem> { new SelectListItem { Value = "XAU", Text = "Gold" } }
            };

            // Act
            var result = await controller.Edit(model);

            // Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result);
            model = Assert.IsType<EditProfileViewModel>(viewResult.Model);
            Assert.Equal("An error occurred while updating your profile. Please try again later.", model.ErrorMessage);
        }

    }
}
