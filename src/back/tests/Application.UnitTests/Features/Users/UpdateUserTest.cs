using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Users.UpdateUser;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Users
{
    public class UpdateUserTest : TestBase
    {
        [Fact]
        public async Task UpdateUserTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var command = new UpdateUserCommand();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task UpdateUserTest_ShouldReturnValidationException_WhenRequiredPropertiesMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateUser]);

            var command = new UpdateUserCommand();

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(UpdateUserCommand.UserId),
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "FirstName",
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "LastName",
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "Email",
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "Phone",
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "DistrictId",
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "Roles",
                    ValidationErrorCode.Required
                )
            );
        }

        [Fact]
        public async Task UpdateUserTest_ShouldReturnValidationException_WhenEmailAlreadyExists()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateUser]);

            var user = await CreateUserAsync(serviceProvider, email: "test@email");

            var command = new UpdateUserCommand() { Email = "test@email" };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>("Email", ValidationErrorCode.Unique)
            );
        }

        [Fact]
        public async Task UpdateUserTest_ShouldReturnValidationException_WhenDistrictDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateUser]);

            var command = new UpdateUserCommand() { DistrictId = 1000 };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>(
                    "DistrictId",
                    ValidationErrorCode.DistrictMustExist
                )
            );
        }

        [Fact]
        public async Task UpdateUserTest_ShouldReturnValidationException_WhenRoleDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateUser]);

            var command = new UpdateUserCommand() { Roles = [Guid.NewGuid()] };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>(
                    "Roles",
                    ValidationErrorCode.RoleMustExist
                )
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task UpdateUserTest_ShouldSucceed(bool isUserActive)
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateUser]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var roles = await context.Roles.Take(2).ToListAsync();

            var district = await CreateDistrictAsync(context);

            DateTimeOffset? disabledDate = isUserActive ? null : timeProvider.GetUtcNow();

            var existingUser = await CreateUserAsync(serviceProvider, roleId: roles[0].Id, isActive: isUserActive, disabledDate: disabledDate);

            var command = new UpdateUserCommand()
            {
                UserId = existingUser.Id,
                Email = "test@email",
                FirstName = "firstname",
                LastName = "lastname",
                Phone = "+33 1 02 03 04 05",
                IsActive = !isUserActive,
                Roles = [roles[1].Id],
                DistrictId = district.Id,
            };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var user = await context.Users.FirstOrDefaultAsync(s => s.Id == existingUser.Id);
            user.Should().NotBeNull();
            result.Data.Should().Be(user!.Id);
            user.FirstName.Should().Be(command.FirstName);
            user.LastName.Should().Be(command.LastName);
            user.Email.Should().Be(command.Email);
            user.UserRoles.Should().ContainSingle();
            user.UserRoles.ElementAt(0).RoleId.Should().Be(roles[1].Id);
            user.UserDistricts.Should().ContainSingle();
            user.UserDistricts.ElementAt(0).DistrictId.Should().Be(district.Id);
            if (isUserActive)
            {
                user.DisabledDate.Should().NotBeNull();
            }
            else
            {
                user.DisabledDate.Should().BeNull();
            }

        }
    }
}
