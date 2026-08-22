using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Metal_Mate_MVC.Tests
{
    public class AlertRequestServiceTests
    {

        // In-memory Database - happy path - Checks only alert requests for the specified user are returned
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

        // In-memory Database - happy path - Checks empty list is returned if no requests exist
        // for the specified user
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

        // In-memory Database - happy path - Save changes
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
                .SingleOrDefaultAsync(x => x.Id == alertRequest.Id,
                TestContext.Current.CancellationToken);

            Assert.NotNull(result);

        }

        // In-memory Database - happy path - Save changes
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

        // In-memory Database - happy path - Get alert request by id.
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
            var alertRequestRetrieved = await service.GetByIdAsync(1);

            // Assert

            Assert.NotNull(alertRequestRetrieved);
            Assert.Equal(context.AlertRequests.First().Id, alertRequestRetrieved.Id);
            Assert.Equal(context.AlertRequests.First().Metal, alertRequestRetrieved.Metal);
            Assert.Equal(context.AlertRequests.First().Currency, alertRequestRetrieved.Currency);
            Assert.Equal(context.AlertRequests.First().Value, alertRequestRetrieved.Value);

        }

        // In-memory Database - happy path - Save changes
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
            await service.DeleteAsync(alertRequest);

            // Assert
            var result = await context.AlertRequests
                .SingleOrDefaultAsync(x => x.Id == alertRequest.Id,
                TestContext.Current.CancellationToken);

            Assert.Null(result);

        }

    }
}