using Application.Exceptions;
using Application.Features.Users.RegisterUser;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tools.Constants;

namespace Application.UnitTests.Features.Users
{
    public class RegisterUserTest : TestBase
    {
        private const string ValidPassword = "Secret12";

        private static RegisterUserCommand BuildValidCommand(
            long districtId,
            string email = "new.user@yopmail.com",
            string? password = ValidPassword
        )
        {
            return new RegisterUserCommand
            {
                FirstName = "John",
                LastName = "Doe",
                Email = email,
                Phone = "+33 1 23 45 67 89",
                DistrictId = districtId,
                Password = password,
            };
        }

        [Fact]
        public async Task RegisterUserTest_ShouldCreateUserAndReturnId_WhenCommandIsValid()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var district = await CreateDistrictAsync(context, isEnabled: true);

            var command = BuildValidCommand(district.Id, email: "john.doe@yopmail.com");

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBe(Guid.Empty);

            var created = await context
                .Users.Include(u => u.UserDistricts)
                .FirstAsync(u => u.Id == result.Data);
            created.Email.Should().Be("john.doe@yopmail.com");
            created.UserName.Should().Be("john.doe@yopmail.com");
            created.FirstName.Should().Be("John");
            created.LastName.Should().Be("Doe");
            created.PhoneNumber.Should().Be("+33 1 23 45 67 89");
            created.UserDistricts.Should().ContainSingle(ud => ud.DistrictId == district.Id);
        }

        [Fact]
        public async Task RegisterUserTest_ShouldHashPassword_WhenPasswordIsProvided()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<UserDao>>();
            var district = await CreateDistrictAsync(context, isEnabled: true);

            var command = BuildValidCommand(district.Id, email: "secured@yopmail.com");

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert — the provided password must be hashed and usable for authentication
            var created = await context.Users.FirstAsync(u => u.Id == result.Data);
            created.PasswordHash.Should().NotBeNullOrEmpty();

            var passwordValid = await userManager.CheckPasswordAsync(created, ValidPassword);
            passwordValid.Should().BeTrue();

            var wrongPassword = await userManager.CheckPasswordAsync(created, "WrongPassword1");
            wrongPassword.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterUserTest_ShouldAssignElectorRole_WhenUserIsRegistered()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var district = await CreateDistrictAsync(context, isEnabled: true);

            var electorRoleId = await context
                .Roles.Where(r => r.Name == AppConstants.ElectorRole)
                .Select(r => r.Id)
                .FirstAsync();

            var command = BuildValidCommand(district.Id, email: "elector@yopmail.com");

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            var created = await context
                .Users.Include(u => u.UserRoles)
                .FirstAsync(u => u.Id == result.Data);
            created.UserRoles.Should().ContainSingle(ur => ur.RoleId == electorRoleId);
        }

        [Fact]
        public async Task RegisterUserTest_ShouldThrowValidationException_WhenRequiredFieldIsMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var district = await CreateDistrictAsync(context, isEnabled: true);

            var command = BuildValidCommand(district.Id);
            command.FirstName = null;

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegisterUserCommand.FirstName),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task RegisterUserTest_ShouldThrowValidationException_WhenEmailIsInvalid()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var district = await CreateDistrictAsync(context, isEnabled: true);

            var command = BuildValidCommand(district.Id, email: "not-an-email");

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegisterUserCommand.Email),
                ValidationErrorCode.InvalidEmail
            );
        }

        [Fact]
        public async Task RegisterUserTest_ShouldThrowValidationException_WhenEmailAlreadyExists()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var district = await CreateDistrictAsync(context, isEnabled: true);

            await CreateUserAsync(serviceProvider, email: "taken@yopmail.com");

            var command = BuildValidCommand(district.Id, email: "taken@yopmail.com");

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegisterUserCommand.Email),
                ValidationErrorCode.Unique
            );
        }

        [Fact]
        public async Task RegisterUserTest_ShouldThrowValidationException_WhenPasswordIsMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var district = await CreateDistrictAsync(context, isEnabled: true);

            var command = BuildValidCommand(district.Id, password: null);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegisterUserCommand.Password),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task RegisterUserTest_ShouldThrowValidationException_WhenPasswordIsTooShort()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var district = await CreateDistrictAsync(context, isEnabled: true);

            var command = BuildValidCommand(district.Id, password: "Short1");

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegisterUserCommand.Password),
                ValidationErrorCode.MinLength
            );
        }

        [Fact]
        public async Task RegisterUserTest_ShouldThrowValidationException_WhenDistrictDoesNotExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var command = BuildValidCommand(districtId: 999_999, email: "nodistrict@yopmail.com");

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegisterUserCommand.DistrictId),
                ValidationErrorCode.DistrictMustExist
            );
        }
    }
}
