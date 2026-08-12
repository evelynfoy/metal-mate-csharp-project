using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Metal_Mate_MVC.Tests.Integration.Controllers
{
    public class HomeControllerIntegrationTests
    {
        // Tests that the Index action of the HomeController correctly retrieves and displays
        // the authenticated user's favorite currency and metal in the dropdown selections.
        [Fact]
        public async Task Index_Defaults_Dropdown_Selections_To_Authenticated_Users_Favourites()
        {
            // Arrange
            // Setup the test server and create a test user
            using var factory = new CustomWebApplicationFactory();
            using var scope = factory.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();


            var user = new ApplicationUser
            {
                UserName = "test@test.com",
                Email = "test@test.com",
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Smith",
                FavouriteCurrency = "INR",
                FavouriteMetal = "BTC"
            };

            await userManager.CreateAsync(user, "Password123!");

            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-UserId",
                user.Id);

            // Act
            var response = await client.GetAsync(
                "/",
                TestContext.Current.CancellationToken);

            // Assert
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.Contains("<option selected=\"selected\" value=\"INR\">INR</option>", html);
            Assert.Contains("<option selected=\"selected\" value=\"BTC\">Bitcoin</option>", html);
            // Seeded spot price for favourites
            Assert.Contains("95.2421", html);
        }

        // Tests that the Index action of the HomeController correctly defaults to standard currency
        // and metal selections when there is no authenticated user, ensuring that the application
        // behaves as expected in the absence of user-specific preferences.
        [Fact]
        public async Task Index_Defaults_Dropdown_Selections_To_Standard_Defaults()
        {
            // Arrange
            // Setup the test server and create a test user
            using var factory = new CustomWebApplicationFactory();
            using var scope = factory.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync(
                "/",
                TestContext.Current.CancellationToken);

            // Assert
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.Contains("<option selected=\"selected\" value=\"EUR\">EUR</option>", html);
            Assert.Contains("<option selected=\"selected\" value=\"XAU\">Gold</option>", html);
            // Seeded defaulted spot price 
            Assert.Contains("3500", html);
        }

        // Tests that the Index action of the HomeController returns a user friendly error message
        // when the the Authenticated user doesn't exist in the databse.
        [Fact]
        public async Task Index_Error_User_Not_In_Database()
        {
            // Arrange
            // Setup the test server and create a test user
            using var factory = new CustomWebApplicationFactory();
            using var scope = factory.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var client = factory.CreateClient();

            var nonExistentUserId = Guid.NewGuid().ToString();

            client.DefaultRequestHeaders.Add(
                "X-Test-UserId",
                nonExistentUserId);

            // Act
            var response = await client.GetAsync(
                "/",
                TestContext.Current.CancellationToken);

            // Assert
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            // Assert that the error message is displayed in the Error section of the page
            Assert.Contains("Apologies, your profile information is currently unavailable so your favourite " +
                "selections cannot be defaulted. Please use the dropdowns above.", html);

            //Assert that the dropdowns default to standard values
            Assert.Contains("<option selected=\"selected\" value=\"EUR\">EUR</option>", html);
            Assert.Contains("<option selected=\"selected\" value=\"XAU\">Gold</option>", html);
            // Seeded defaulted spot price 
            Assert.Contains("3500", html);
        }

    }
}
