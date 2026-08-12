using Metal_Mate_MVC.Data;
using Metal_Mate_MVC.Models;
using Metal_Mate_MVC.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Metal_Mate_MVC.Tests.Integration.Infrastructure
{ 

    // CustomWebApplicationFactory is a custom implementation of WebApplicationFactory<Program> that sets up
    // an in-memory SQLite database for integration testing.
    // It removes the application's SQL Server DbContext and replaces it with an in-memory SQLite DbContext.
    // It also mocks the IApiService to return predefined data for testing purposes.
    // Additionally, it configures authentication for testing scenarios.
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Configure the web host for integration testing
            builder.ConfigureServices(services =>
            {
                // Remove the application's SQL Server DbContext
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));

                // Create an in-memory SQLite database
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // Remove the application's IApiService and replace it with a mock implementation
                services.RemoveAll<IApiService>();

                var apiService = new Mock<IApiService>();

                apiService
                    .Setup(x => x.GetAPIDataAsync<List<Metal>>("symbols"))
                    .ReturnsAsync(new List<Metal>
                        {
                        new Metal
                        {
                            Symbol = "XAU",
                            Name = "Gold"
                        },
                        new Metal
                        {
                            Symbol = "XAG",
                            Name = "Silver"
                        },
                        new Metal
                        {
                            Symbol = "BTC",
                            Name = "Bitcoin"
                        }
                    });

                apiService
                    .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/XAU/EUR"))
                    .ReturnsAsync(new SpotPrice
                    {
                        Name = "Gold",
                        UpdatedAtReadable = "a few minutes ago",
                        UpdatedAt = DateTime.UtcNow,
                        CurrencySymbol = "€",
                        ExchangeRate = 1.00f,
                        Symbol = "XAU",
                        Currency = "EUR",
                        Price = 3500.00f
                    });

                apiService
                    .Setup(s => s.GetAPIDataAsync<SpotPrice>("price/BTC/INR"))
                    .ReturnsAsync(new SpotPrice
                    {
                        Name = "Bitcoin",
                        UpdatedAtReadable = "a few minutes ago",
                        UpdatedAt = DateTime.UtcNow,
                        CurrencySymbol = "₹",
                        ExchangeRate = 95.2421f,
                        Symbol = "BTC",
                        Currency = "INR",
                        Price = 6186641.00f
                    });


                services.AddSingleton(apiService.Object);

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context (ApplicationDbContext)
                using var scope = serviceProvider.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Create Identity tables
                db.Database.EnsureCreated();
            
            });

            // Configure authentication for testing
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme,
                    options => { });
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _connection?.Dispose();
        }
    
    }
}