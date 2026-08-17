using Metal_Mate_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Metal_Mate_MVC.Tests.Unit.SetUp
{
    public class SetUpMocks
    {
        // Helper method to create a mock UserManager
        public static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();

            var options = Options.Create(new IdentityOptions());

            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var userValidators =
                Array.Empty<IUserValidator<ApplicationUser>>();

            var passwordValidators =
                Array.Empty<IPasswordValidator<ApplicationUser>>();

            var keyNormalizer =
                new UpperInvariantLookupNormalizer();

            var errors =
                new IdentityErrorDescriber();

            var services =
                new Mock<IServiceProvider>();

            var logger =
                new Mock<ILogger<UserManager<ApplicationUser>>>();

            return new Mock<UserManager<ApplicationUser>>(
                store.Object,
                options,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errors,
                services.Object,
                logger.Object);
        }
    }
}
