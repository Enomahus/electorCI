using Application.Exceptions.Auth;
using Application.Features.Security.Authenticate;
using Application.UnitTests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Application.UnitTests.Features.Security
{
    public class AuthenticateTest : TestBase
    {
        [Fact]
        public async Task AuthenticateTest_ShouldReturnTokens_WhenAuthenticationIsSuccessful()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(setupDateService: t =>
                {
                    t.GetUtcNow().Returns(new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc));
                })
                .BuildServiceProvider();

            string password = "Password1@";
            await CreateUserAsync(serviceProvider, "Test", password);

            var command = new AuthenticateCommand() { UserName = "Test", Password = password };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
            result.Data!.AccessToken.Should().NotBeEmpty();
            result.Data.RefreshToken.Should().NotBeEmpty();
        }

        [Fact]
        public async Task AuthenticateTest_ShouldReturnUserAuthenticationException_WhenPasswordIsIncorrect()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(setupDateService: t =>
                {
                    t.GetUtcNow().Returns(new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc));
                })
                .BuildServiceProvider();

            string password = "Password1@";
            await CreateUserAsync(serviceProvider, "Test", password);

            var command = new AuthenticateCommand() { UserName = "Test", Password = "WrongPass1" };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAuthenticationException>();
        }

        [Fact]
        public async Task AuthenticateTest_ShouldReturnUserAuthenticationException_WhenUnactive()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(setupDateService: t =>
                {
                    t.GetUtcNow().Returns(new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc));
                })
                .BuildServiceProvider();

            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            string password = "Password1@";
            await CreateUserAsync(
                serviceProvider,
                "Test",
                password,
                disabledDate: timeProvider.GetUtcNow()
            );

            var command = new AuthenticateCommand() { UserName = "Test", Password = password };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAuthenticationException>();
        }

        [Fact]
        public async Task AuthenticateTest_ShouldReturnUserAuthenticationException_WhenUserIsNotFound()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(setupDateService: t =>
                {
                    t.GetUtcNow().Returns(new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc));
                })
                .BuildServiceProvider();

            var command = new AuthenticateCommand() { UserName = "Test", Password = "WrongPass1" };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAuthenticationException>();
        }
    }
}
