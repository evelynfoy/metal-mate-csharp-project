using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Metal_Mate_MVC.Tests
{
    public class AlertRequestServiceTests
    {
        // All tests are using an in-memory SQLite database. 
        // Happy path - Returns only user's alerts
        [Fact]
        public async Task GetForUserAsync_ValidResponse_ReturnsOnlyRequestsForSpecifiedUser()
        {

            // Arrange
            // Create an in-memory SQLite database with two users and two alert requests for the first user and one for the second user
            // The boolean parameter tells the method to create the alert requests for the first user.
            await using var testDb =
                await SetUpTestDB.CreateAsync(TestContext.Current.CancellationToken, true);

            var context = testDb.Context;

            var service = new AlertRequestService(context);
            var user = context.Users.First();

            // Act
            var result = await service.GetForUserAsync(user.Id);

            // Assert
            Assert.NotNull(result);

            var requests = Assert.IsType<List<AlertRequest>>(result);
            Assert.Equal(2, requests.Count);

            Assert.All(
                requests,
                request => Assert.Equal(user.Id, request.UserId));

            // Test that result includes the User navigation property
            Assert.All(result, request =>
                Assert.NotNull(request.User));

        }

        // Happy path - Returns empty list when none
        [Fact]
        public async Task GetForUserAsync_ValidResponse_ReturnsEmptyListForSpecifiedUser()
        {

            // Arrange
            // Create an in-memory SQLite database with two users, no alert requests for the first user and one for the second user
            await using var testDb =
                await SetUpTestDB.CreateAsync(TestContext.Current.CancellationToken, false);

            var context = testDb.Context;

            var service = new AlertRequestService(context);

            // Act
            var result = await service.GetForUserAsync("user1");

            // Assert
            Assert.NotNull(result);

            var requests = Assert.IsType<List<AlertRequest>>(result);

            Assert.Empty(requests);

        }

        // Happy path - Returns user's alert
        [Fact]
        public async Task GetByIdAsync_ValidResponse_ReturnsValidResult()
        {

            // Arrange
            // Create an in-memory SQLite database with two users and two alert requests for the first user and one for the second user
            // The boolean parameter tells the method to create the alert requests for the first user.
            await using var testDb =
                await SetUpTestDB.CreateAsync(TestContext.Current.CancellationToken, true);

            var context = testDb.Context;

            var service = new AlertRequestService(context);
            var user = context.Users.First();

            // Act
            var alertRequestRetrieved = await service.GetByIdAsync(1, user.Id);

            // Assert

            Assert.NotNull(alertRequestRetrieved);
            Assert.Equal(context.AlertRequests.First().Id, alertRequestRetrieved.Id);
            Assert.Equal(context.AlertRequests.First().Metal, alertRequestRetrieved.Metal);
            Assert.Equal(context.AlertRequests.First().Currency, alertRequestRetrieved.Currency);
            Assert.Equal(context.AlertRequests.First().Value, alertRequestRetrieved.Value);

        }

        // User requests anothers entry - Doesn't return another user's alert
        [Fact]
        public async Task GetByIdAsync_AnotherUsersAlert_ReturnsNull()
        {

            // Arrange
            // Creates two users with alert requests belonging to each user.
            await using var testDb =
                await SetUpTestDB.CreateAsync(TestContext.Current.CancellationToken, true);

            var context = testDb.Context;

            var service = new AlertRequestService(context);
            var user = context.Users.First();

            // Act
            // The alert request with id 3 belongs to another user.
            var alertRequestRetrieved = await service.GetByIdAsync(3, user.Id);

            // Assert
            Assert.Null(alertRequestRetrieved);

        }

        // Happy path - Persists alert
        [Fact]
        public async Task AddAsync_ValidResponse_ReturnsValidResult()
        {

            // Arrange
            // Create an in-memory SQLite database with two users and two alert requests for the first user and one for the second user
            // The boolean parameter tells the method to create the alert requests for the first user.
            await using var testDb =
                await SetUpTestDB.CreateAsync(TestContext.Current.CancellationToken, true);

            var context = testDb.Context;

            var service = new AlertRequestService(context);
            var user = context.Users.First();
            var alertRequest = SetUpTestDB.CreateUserAlertRequest(user);

            // Act
            await service.AddAsync(alertRequest);

            // Assert
            var result = await context.AlertRequests
                .SingleOrDefaultAsync(s => s.Id == alertRequest.Id,
                TestContext.Current.CancellationToken);

            Assert.NotNull(result);

        }

        // Happy path - Updates alert
        [Fact]
        public async Task SaveAsync_ValidResponse_ReturnsValidResult()
        {

            // Arrange
            // Create an in-memory SQLite database with two users and two alert requests for the first user and one for the second user
            // The boolean parameter tells the method to create the alert requests for the first user.
            await using var testDb =
                await SetUpTestDB.CreateAsync(TestContext.Current.CancellationToken, true);

            var context = testDb.Context;

            var service = new AlertRequestService(context);
            var user = context.Users.First();
            var alertRequest = SetUpTestDB.CreateUserAlertRequest(user);
            await service.AddAsync(alertRequest);
            alertRequest.Metal = "XAG"; //Silver


            // Act
            await service.SaveAsync(alertRequest);

            // Assert
            var result = await context.AlertRequests
                .SingleOrDefaultAsync(x => x.Id == alertRequest.Id,
                TestContext.Current.CancellationToken);

            Assert.NotNull(result);
            Assert.Equal(alertRequest.Metal, result.Metal);

        }

        // Happy path - Deletes user's alert
        [Fact]
        public async Task DeleteAsync_ValidResponse_ReturnsValidResult()
        {

            // Arrange
            // Create an in-memory SQLite database with two users and two alert requests for the first user and one for the second user
            // The boolean parameter tells the method to create the alert requests for the first user.
            await using var testDb =
                await SetUpTestDB.CreateAsync(TestContext.Current.CancellationToken, true);

            var context = testDb.Context;

            var service = new AlertRequestService(context);
            var user = context.Users.First();
            var alertRequest = context.AlertRequests.First();


            // Act
            var isDeleted = await service.DeleteAsync(alertRequest.Id, user.Id);

            // Assert
            Assert.True(isDeleted);

            var result = await context.AlertRequests
                .SingleOrDefaultAsync(x => x.Id == alertRequest.Id,
                TestContext.Current.CancellationToken);

            Assert.Null(result);

        }

        // User tries to delete another users request - Returns false and does not delete the entry
        [Fact]
        public async Task DeleteAsync_AnotherUsersAlert_DoesNotDeleteRequest()
        {

            // Arrange
            // Creates two users with alert requests belonging to each user.
            await using var testDb =
                await SetUpTestDB.CreateAsync(TestContext.Current.CancellationToken, true);

            var context = testDb.Context;

            var service = new AlertRequestService(context);
            var user = context.Users.First();

            // Act
            // The alert request with id 3 belongs to another user.
            var isDeleted = await service.DeleteAsync(3, user.Id);

            // Assert
            Assert.False(isDeleted);
        }

        // No alert request exists for Id - Returns false for nonexistent alert
        [Fact]
        public async Task DeleteAsync_NoAlertFound_ReturnsFalse()
        {

            // Arrange
            // Creates user with no alert request.
            await using var testDb =
                await SetUpTestDB.CreateAsync(TestContext.Current.CancellationToken, false);

            var context = testDb.Context;

            var service = new AlertRequestService(context);
            var user = context.Users.First();

            // Act
            // The alert request with id 1 does not exist.
            var isDeleted = await service.DeleteAsync(1, user.Id);

            // Assert
            Assert.False(isDeleted);
        }

    }
}