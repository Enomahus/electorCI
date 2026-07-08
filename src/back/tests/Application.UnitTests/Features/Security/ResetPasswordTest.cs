using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Security.ResetPassword;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Security
{
    public class ResetPasswordTest : TestBase
    {
        [Fact]
        public async Task ResetPassword_ShouldError_WhenMissingFields()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var command = new ResetPasswordCommand()
            {
                Password = null,
                ResetToken = null,
                UserEmail = null,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert

            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(ResetPasswordCommand.Password),
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(ResetPasswordCommand.ResetToken),
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(ResetPasswordCommand.UserEmail),
                    ValidationErrorCode.Required
                )
            );
        }

        [Fact]
        public async Task ResetPassword_ShouldError_WhenWrongEmailFormat()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await CreateUserAsync(serviceProvider, email: "hello@test.fr");

            var command = new ResetPasswordCommand()
            {
                Password = "Secret123",
                ResetToken = "azeazeaze",
                UserEmail = "azeazeaze",
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ResetPasswordCommand.UserEmail),
                ValidationErrorCode.InvalidEmail
            );
        }

        [Fact]
        public async Task ResetPassword_ShouldError_WhenEmailDoesNotExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await CreateUserAsync(serviceProvider, email: "hello@test.fr");

            var command = new ResetPasswordCommand()
            {
                Password = "Secret123",
                ResetToken = "azeazeaze",
                UserEmail = "hello2@test.fr",
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ResetPasswordCommand.UserEmail),
                ValidationErrorCode.UserMustExist
            );
        }

        [Fact]
        public async Task ResetPassword_ShouldError_WhenPasswordTooShort()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await CreateUserAsync(serviceProvider, email: "hello@test.fr");
            var command = new ResetPasswordCommand()
            {
                Password = "Aze123",
                ResetToken = "azeazeaze",
                UserEmail = "hello@test.fr",
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ResetPasswordCommand.Password),
                ValidationErrorCode.InvalidPassword
            );
        }

        [Fact]
        public async Task ResetPassword_ShouldError_WhenPasswordMissingUppercase()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await CreateUserAsync(serviceProvider, email: "hello@test.fr");
            var command = new ResetPasswordCommand()
            {
                Password = "aze123aze",
                ResetToken = "azeazeaze",
                UserEmail = "hello@test.fr",
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ResetPasswordCommand.Password),
                ValidationErrorCode.InvalidPassword
            );
        }

        [Fact]
        public async Task ResetPassword_ShouldError_WhenPasswordMissingDigit()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await CreateUserAsync(serviceProvider, email: "hello@test.fr");
            var command = new ResetPasswordCommand()
            {
                Password = "AZEazeaze",
                ResetToken = "azeazeaze",
                UserEmail = "hello@test.fr",
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ResetPasswordCommand.Password),
                ValidationErrorCode.InvalidPassword
            );
        }

        [Fact]
        public async Task ResetPassword_ShouldError_WhenPasswordIsTooLong()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await CreateUserAsync(serviceProvider, email: "hello@test.fr");
            var command = new ResetPasswordCommand()
            {
                Password = "AZEazeaze123AZEazeaze123A",
                ResetToken = "azeazeaze",
                UserEmail = "hello@test.fr",
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ResetPasswordCommand.Password),
                ValidationErrorCode.InvalidPassword
            );
        }

        [Fact]
        public async Task ResetPassword_ShouldNotThrow_WhenPasswordIsValid()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await CreateUserAsync(serviceProvider, email: "hello@test.fr");
            var command = new ResetPasswordCommand()
            {
                Password = "Secret123",
                ResetToken = "azeazeaze",
                UserEmail = "hello@test.fr",
            };

            // Act / Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .NotThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task ResetPassword_ShouldError_WhenInvalidToken()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await CreateUserAsync(serviceProvider, email: "hello@test.fr");
            var command = new ResetPasswordCommand()
            {
                Password = "Secret123",
                ResetToken = "azeazeaze",
                UserEmail = "hello@test.fr",
            };

            // Act / Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ResetTokenException>();
        }

        [Fact]
        public async Task ResetPassword_ShouldError_WhenUsingOtherUserToken()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var userManager = serviceProvider.GetRequiredService<UserManager<UserDao>>();
            var context = serviceProvider.GetRequiredService<ReadOnlyDbContext>();

            await CreateUserAsync(serviceProvider, email: "hello@test.fr");
            await CreateUserAsync(serviceProvider, email: "hello2@test.fr");
            var user1 = await context.Users.FirstAsync(u => u.Email == "hello@test.fr");
            var token1 = await userManager.GeneratePasswordResetTokenAsync(user1);
            var user2 = await context.Users.FirstAsync(u => u.Email == "hello2@test.fr");
            var token2 = await userManager.GeneratePasswordResetTokenAsync(user1);

            var command = new ResetPasswordCommand()
            {
                Password = "Secret123",
                ResetToken = token1,
                UserEmail = user2.Email,
            };

            // Act / Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ResetTokenException>();
        }

        [Fact]
        public async Task ResetPassword_ShouldError_WhenUsingSameTokenTwice()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var userManager = serviceProvider.GetRequiredService<UserManager<UserDao>>();
            var context = serviceProvider.GetRequiredService<ReadOnlyDbContext>();

            await CreateUserAsync(serviceProvider, email: "hello@test.fr");
            var user = await context.Users.FirstAsync(u => u.Email == "hello@test.fr");
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var command = new ResetPasswordCommand()
            {
                Password = "Secret123",
                ResetToken = token,
                UserEmail = user.Email,
            };

            // Act / Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .NotThrowAsync<ResetTokenException>();

            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ResetTokenException>();
        }

        [Fact]
        public async Task ResetPassword_ShouldSucceed()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var userManager = serviceProvider.GetRequiredService<UserManager<UserDao>>();
            var context = serviceProvider.GetRequiredService<ReadOnlyDbContext>();

            await CreateUserAsync(serviceProvider, email: "hello@test.fr", password: "Secret321");
            var user = await context.Users.FirstAsync(u => u.Email == "hello@test.fr");

            var loginResult = await userManager.CheckPasswordAsync(user, "Secret321");
            loginResult.Should().BeTrue();
            loginResult = await userManager.CheckPasswordAsync(user, "Secret123");
            loginResult.Should().BeFalse();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var command = new ResetPasswordCommand()
            {
                Password = "Secret123",
                ResetToken = token,
                UserEmail = "hello@test.fr",
            };

            // Act
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .NotThrowAsync<ResetTokenException>();

            user = await context.Users.FirstAsync(u => u.Email == "hello@test.fr");
            loginResult = await userManager.CheckPasswordAsync(user, "Secret321");
            loginResult.Should().BeFalse();
            loginResult = await userManager.CheckPasswordAsync(user, "Secret123");
            loginResult.Should().BeTrue();
        }
    }
}
