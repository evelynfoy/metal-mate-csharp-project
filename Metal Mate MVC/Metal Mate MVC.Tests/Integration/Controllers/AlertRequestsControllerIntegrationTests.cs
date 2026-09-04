using Metal_Mate_MVC.Data;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Metal_Mate_MVC.Tests.Integration.Controllers
{
    public class AlertRequestsControllerIntegrationTests
    {
        // Tests that the Get action of the AlertRequestsController allows
        // a user to view their own alert request details.
        [Fact]
        public async Task GetDetails_UsersOwnRequest_DisplaysAlert()
        {
            // Arrange
            // Setup the test server and create 2 test users
            using var factory = new CustomWebApplicationFactory();
            using var scope = factory.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // Set up user 
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

            var userCreated = await userManager.CreateAsync(user, "Password123!");

            // Authenticate as user 1
            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-UserId",
                user.Id);

            // Create an alert request for user
            var alertRequest = new AlertRequest
            {
                UserId = user.Id,
                User = user,
                Metal = "XAU",
                Currency = "USD",
                Operator = ComparisonOperator.GreaterThan,
                Value = 4000,
                IsEnabled = true
            };

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            context.AlertRequests.AddRange(
                alertRequest);

            var alertRequestCreated = await context.SaveChangesAsync(
                TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/AlertRequests/Details/{alertRequest.Id}",
                TestContext.Current.CancellationToken);

            // Assert
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.True(userCreated.Succeeded);
            Assert.True(alertRequestCreated.Equals(1));

            // Error is NOT displayed
            Assert.DoesNotContain(html,"There was a problem loading this entry. Please try again.");

            // Definition List of data IS displayed
            Assert.Contains("<dl class=\"row\">", html);

        }


        // Tests that the Get action of the AlertRequestsController does NOT allow
        // a user to view another user's alert request details.
        [Fact]
        public async Task GetDetails_AnotherUsersAlert_DoesNotDisplayAlert()
        {
            // Arrange
            // Setup the test server and create 2 test users
            using var factory = new CustomWebApplicationFactory();
            using var scope = factory.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // Set up user 1
            var user1 = new ApplicationUser
            {
                UserName = "test@test.com",
                Email = "test@test.com",
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Smith",
                FavouriteCurrency = "INR",
                FavouriteMetal = "BTC"
            };

            var user1Created = await userManager.CreateAsync(user1, "Password123!");

            // Set up user 2
            var user2 = new ApplicationUser
            {
                UserName = "test2@test.com",
                Email = "test2@test.com",
                EmailConfirmed = true,
                FirstName = "Jane",
                LastName = "Smith",
                FavouriteCurrency = "USD",
                FavouriteMetal = "BTC"
            };

            var user2Created = await userManager.CreateAsync(user2, "Password456!");

            // Authenticate as user 1
            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-UserId",
                user1.Id);

            // Create an alert request for user 2
            var alertRequest = new AlertRequest
            {
                UserId = user2.Id,
                User = user2,
                Metal = "XAU",
                Currency = "USD",
                Operator = ComparisonOperator.GreaterThan,
                Value = 4000,
                IsEnabled = true
            };

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            context.AlertRequests.AddRange(
                alertRequest);

            var alertRequestCreated = await context.SaveChangesAsync(
                TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/AlertRequests/Details/{alertRequest.Id}",
                TestContext.Current.CancellationToken);

            // Assert
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.True(user1Created.Succeeded);
            Assert.True(user2Created.Succeeded);
            Assert.True(alertRequestCreated.Equals(1));

            // Error is displayed
            Assert.Contains("There was a problem loading this entry. Please try again.", html);

            // Definition List of data is not displayed
            Assert.DoesNotContain(html, "<dl class=\"row\">");
        }

        [Fact]
        public async Task Edit_AnotherUsersAlert_DoesNotChangeAlert()
        {
            // Arrange
            // Setup the test server and create 2 test users
            using var factory = new CustomWebApplicationFactory();
            using var scope = factory.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // Set up user 1
            var user1 = new ApplicationUser
            {
                UserName = "test@test.com",
                Email = "test@test.com",
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Smith",
                FavouriteCurrency = "INR",
                FavouriteMetal = "BTC"
            };

            var user1Created = await userManager.CreateAsync(user1, "Password123!");

            // Set up user 2
            var user2 = new ApplicationUser
            {
                UserName = "test2@test.com",
                Email = "test2@test.com",
                EmailConfirmed = true,
                FirstName = "Jane",
                LastName = "Smith",
                FavouriteCurrency = "USD",
                FavouriteMetal = "BTC"
            };

            var user2Created = await userManager.CreateAsync(user2, "Password456!");

            // Authenticate as user 1
            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-UserId",
                user1.Id);

            // Create an alert request for user 2
            var alertRequest = new AlertRequest
            {
                UserId = user2.Id,
                User = user2,
                Metal = "XAU",
                Currency = "USD",
                Operator = ComparisonOperator.GreaterThan,
                Value = 4000,
                IsEnabled = true
            };

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            context.AlertRequests.AddRange(
                alertRequest);

            var alertRequestCreated = await context.SaveChangesAsync(
                TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/AlertRequests/Edit/{alertRequest.Id}",
                TestContext.Current.CancellationToken);

            // Assert
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.True(user1Created.Succeeded);
            Assert.True(user2Created.Succeeded);
            Assert.True(alertRequestCreated.Equals(1));

            // Error is displayed
            Assert.Contains("There was a problem loading this entry. Please try again.", html);

            // Data from request is not
            Assert.DoesNotContain(html, "value=\"0\"");
        }

        [Fact]
        public async Task Delete_AnotherUsersAlert_DoesNotDeleteAlert()
        {
            // Arrange
            // Setup the test server and create 2 test users
            using var factory = new CustomWebApplicationFactory();
            using var scope = factory.Services.CreateScope();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // Set up user 1
            var user1 = new ApplicationUser
            {
                UserName = "test@test.com",
                Email = "test@test.com",
                EmailConfirmed = true,
                FirstName = "John",
                LastName = "Smith",
                FavouriteCurrency = "INR",
                FavouriteMetal = "BTC"
            };

            var user1Created = await userManager.CreateAsync(user1, "Password123!");

            // Set up user 2
            var user2 = new ApplicationUser
            {
                UserName = "test2@test.com",
                Email = "test2@test.com",
                EmailConfirmed = true,
                FirstName = "Jane",
                LastName = "Smith",
                FavouriteCurrency = "USD",
                FavouriteMetal = "BTC"
            };

            var user2Created = await userManager.CreateAsync(user2, "Password456!");

            // Authenticate as user 1
            var client = factory.CreateClient();

            client.DefaultRequestHeaders.Add(
                "X-Test-UserId",
                user1.Id);

            // Create an alert request for user 2
            var alertRequest = new AlertRequest
            {
                UserId = user2.Id,
                User = user2,
                Metal = "XAU",
                Currency = "USD",
                Operator = ComparisonOperator.GreaterThan,
                Value = 4000,
                IsEnabled = true
            };

            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            context.AlertRequests.AddRange(
                alertRequest);

            var alertRequestCreated = await context.SaveChangesAsync(
                TestContext.Current.CancellationToken);

            // Act
            var response = await client.GetAsync(
                $"/AlertRequests/Delete/{alertRequest.Id}",
                TestContext.Current.CancellationToken);

            // Assert
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.True(user1Created.Succeeded);
            Assert.True(user2Created.Succeeded);
            Assert.True(alertRequestCreated.Equals(1));

            // Error is displayed
            Assert.Contains("There was a problem loading this entry. Please try again.", html);

            // Data from request is not
            Assert.DoesNotContain(html, "<dl class=\"row mt-5\">");
        }

    }
}
