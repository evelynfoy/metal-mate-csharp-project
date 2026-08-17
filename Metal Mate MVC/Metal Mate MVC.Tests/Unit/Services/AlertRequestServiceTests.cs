using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;

namespace Metal_Mate_MVC.Tests
{
    public class AlertRequestServiceTests
    {

        // Mocked response - happy path - Checks only alert requests for the specified user are returned
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
    }
}