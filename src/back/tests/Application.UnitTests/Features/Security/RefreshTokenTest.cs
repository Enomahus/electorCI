using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Security.Authenticate;
using Application.Features.Security.RefreshToken;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Application.UnitTests.Features.Security
{
    public class RefreshTokenTest : TestBase
    {
        [Fact]
        public async Task RefreshTokenTest_ShouldReturnTokens_WhenAuthenticationIsSuccessful()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(setupDateService: t =>
                {
                    t.GetUtcNow().Returns(new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc));
                })
                .BuildServiceProvider();

            string password = "Password1@";
            await CreateUserAsync(serviceProvider, "Test", password);

            var authCommand = new AuthenticateCommand() { UserName = "Test", Password = password };
            var authResult = await serviceProvider.SendAsync(authCommand);

            var command = new RefreshTokenCommand()
            {
                UserName = "Test",
                RefreshToken = authResult.Data?.RefreshToken,
            };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().NotBeEmpty();
            result.Data.RefreshToken.Should().NotBeEmpty();
        }

        [Fact]
        public async Task RefreshTokenTest_ShouldReturnAuthenticationException_WhenUserNameIsIncorrect()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(setupDateService: t =>
                {
                    t.GetUtcNow().Returns(new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc));
                })
                .BuildServiceProvider();

            string password = "Password1@";
            await CreateUserAsync(serviceProvider, "Test", password);

            var authCommand = new AuthenticateCommand() { UserName = "Test", Password = password };
            var authResult = await serviceProvider.SendAsync(authCommand);

            var command = new RefreshTokenCommand()
            {
                UserName = "Michel",
                RefreshToken = authResult.Data?.RefreshToken,
            };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAuthenticationException>();
        }

        [Fact]
        public async Task RefreshTokenTest_ShouldReturnValidationException_WhenRefreshTokenIsIncorrect()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(setupDateService: t =>
                {
                    t.GetUtcNow().Returns(new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc));
                })
                .BuildServiceProvider();

            string password = "Password1@";
            await CreateUserAsync(serviceProvider, "Test", password);

            var command = new RefreshTokenCommand()
            {
                UserName = "Test",
                RefreshToken = Guid.NewGuid().ToString(),
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RefreshTokenCommand.RefreshToken),
                ValidationErrorCode.Base64Format
            );
        }

        [Fact]
        public async Task RefreshTokenTest_ShouldReturnAuthenticationException_WhenRefreshTokenIsExpired()
        {
            //Arrange
            var currentDate = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            var serviceProvider = CreateServiceCollection(setupDateService: t =>
                {
                    t.GetUtcNow().Returns(currentDate);
                })
                .BuildServiceProvider();

            string password = "Password1@";
            await CreateUserAsync(serviceProvider, "Test", password);

            var authCommand = new AuthenticateCommand() { UserName = "Test", Password = password };
            var authResult = await serviceProvider.SendAsync(authCommand);

            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var token = context.RefreshTokens.Single();
            token.Expiry = currentDate.AddHours(-1);
            await context.SaveChangesAsync();

            var command = new RefreshTokenCommand()
            {
                UserName = "Test",
                RefreshToken = authResult.Data?.RefreshToken,
            };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAuthenticationException>();
        }
    }
}
