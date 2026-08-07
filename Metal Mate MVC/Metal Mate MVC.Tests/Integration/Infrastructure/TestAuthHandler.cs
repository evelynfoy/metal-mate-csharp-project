using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Metal_Mate_MVC.Tests.Integration.Infrastructure
{
    // Sets up a test authentication handler that can be used in integration tests to simulate authenticated users.
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string AuthenticationScheme = "Test";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-UserId", out var userId))
            {
                return Task.FromResult(
                    AuthenticateResult.Fail("Missing X-Test-UserId header."));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId!),
                new Claim(ClaimTypes.Name, "test@test.com"),
                new Claim(ClaimTypes.Email, "test@test.com")
            };

            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}