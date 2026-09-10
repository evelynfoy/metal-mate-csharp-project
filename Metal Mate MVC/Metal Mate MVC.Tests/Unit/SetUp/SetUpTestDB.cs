using Metal_Mate_MVC.Data;
using Metal_Mate_MVC.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public sealed class SetUpTestDB : IAsyncDisposable
{
    public SqliteConnection Connection { get; }
    public ApplicationDbContext Context { get; }

    private SetUpTestDB(SqliteConnection connection, ApplicationDbContext context)
    {
        Connection = connection;
        Context = context;
    }

    public static async Task<SetUpTestDB> CreateAsync(CancellationToken cancellationToken, bool createAlertRequestsForCurrentUser)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync(cancellationToken);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);

        await context.Database.EnsureCreatedAsync(cancellationToken);

        // Create two users
        var currentUser = CreateUser(1);
        var otherUser = CreateUser(2);

        context.Users.AddRange(currentUser,otherUser);

        // If bool parameter is true two alert requests are created for the current user 
        // This allows testing for the return of no alert requests for the current user when the bool parameter is false.
        if (createAlertRequestsForCurrentUser)
        {
            var currentUserRequest1 = CreateUserAlertRequest(currentUser);
            var currentUserRequest2 = CreateUserAlertRequest(currentUser);

            context.AlertRequests.AddRange(
                currentUserRequest1,
                currentUserRequest2);
        }
        // An alert request is created for the other user regardless of the bool parameter.
        var otherUserRequest = CreateUserAlertRequest(otherUser);

        context.AlertRequests.AddRange(
            otherUserRequest);

        await context.SaveChangesAsync(
               cancellationToken);

        return new SetUpTestDB(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await Connection.DisposeAsync();
    }

    public static ApplicationUser CreateUser(int number)
    {
        return new ApplicationUser
        {
            Id = "user" + number,
            UserName = $"user{number}@test.com",
            Email = $"user{number}@test.com"
        };
    }

    public static AlertRequest CreateUserAlertRequest(ApplicationUser user)
    {
        return new AlertRequest
        {
            UserId = user.Id,
            User = user,
            Metal = "XAU",
            Currency = "USD",
            Operator = ComparisonOperator.GreaterThan,
            Value = 2000,
            IsEnabled = true
        };
    }
}