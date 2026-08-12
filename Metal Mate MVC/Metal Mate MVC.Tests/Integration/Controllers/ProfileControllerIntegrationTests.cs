using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Metal_Mate_MVC.Tests.Integration.Controllers
{
    public class ProfileControllerIntegrationTests
    {

        [Fact]
        public async Task Edit_Get_Returns_Profile_For_Authenticated_User()
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
                FavouriteCurrency = "USD",
                FavouriteMetal = "XAU"
            };

            await userManager.CreateAsync(user, "Password123!");

            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-UserId",
                user.Id);

            // Act
            var response = await client.GetAsync(
                "/Profile/Edit", 
                TestContext.Current.CancellationToken);

            // Assert
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.Contains("John", html);
            Assert.Contains("Smith", html);
        }

        [Fact]
        public async Task Edit_Post_Updates_For_Authenticated_User()
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
                FavouriteCurrency = "EUR",
                FavouriteMetal = "XAU"
            };

            await userManager.CreateAsync(user, "Password123!");

            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-UserId",
                user.Id);

            // Prepare the form data for the POST request
            var model = new Dictionary<string, string>
            {
                ["FirstName"] = "Jane",
                ["LastName"] = "Doe",
                ["FavouriteCurrency"] = "EUR",
                ["FavouriteMetal"] = "XAU",
            };

            // Create the form content
            var content = new FormUrlEncodedContent(model);

            // Act
            var response = await client.PostAsync("/Profile/Edit",
                                                    content,
                                                    TestContext.Current.CancellationToken);

            // Assert
            var html = await response.Content.ReadAsStringAsync(
                        TestContext.Current.CancellationToken);
            Assert.Contains("Profile updated successfully.", html);

            // Verify that the user's profile was updated in the database
            using var verificationScope = factory.Services.CreateScope();

            var verificationUserManager = verificationScope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            var updatedUser = await verificationUserManager.FindByIdAsync(user.Id);

            Assert.NotNull(updatedUser);

            Assert.Equal("Jane", updatedUser!.FirstName);
            Assert.Equal("Doe", updatedUser.LastName);
            Assert.Equal("EUR", updatedUser.FavouriteCurrency);
            Assert.Equal("XAU", updatedUser.FavouriteMetal);
        }

        [Fact]
        public async Task Edit_Get_Redirects_Anonymous_User_To_Login()
        {
            // Arrange
            using var factory = new CustomWebApplicationFactory();

            var client = factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

            // Act
            var response = await client.GetAsync(
                "/Profile/Edit",
                TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);

        }

    }
}
