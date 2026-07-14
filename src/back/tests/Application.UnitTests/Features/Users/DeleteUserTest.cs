using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Users.DeleteUser;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Users
{
    public class DeleteUserTest : TestBase
    {
        [Fact]
        public async Task DeleteUserTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var command = new DeleteUserCommand() { Id = default };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task DeleteUserTest_ShouldReturnValidationException_WhenIdInvalid()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.DeleteUser]);

            var command = new DeleteUserCommand() { Id = Guid.Empty };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, string>(
                    nameof(DeleteUserCommand.Id),
                    ValidationErrorCode.Required.ToString()
                )
            );
        }

        [Fact]
        public async Task DeleteUserTest_ShouldReturnValidationException_WhenUserNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.DeleteUser]);

            var wrongId = Guid.NewGuid();
            var command = new DeleteUserCommand() { Id = wrongId };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteUserCommand.Id),
                ValidationErrorCode.UserMustExist
            );
        }

        [Fact]
        public async Task DeleteUserTest_ShouldReturnValidationException_WhenUserIsLinked()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.DeleteUser]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context);
            var user = await CreateUserAsync(
                serviceProvider,
                email: "todelete@yopmail.com",
                districtId: district.Id
            );

            await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                authorId: user.Id
            );

            var command = new DeleteUserCommand() { Id = user.Id };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteUserCommand.Id),
                ValidationErrorCode.UserLinked
            );
        }


        [Fact]
        public async Task DeleteUserTest_ShouldSucceed()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.DeleteUser]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context);
            var user = await CreateUserAsync(
                serviceProvider,
                email: "todelete@yopmail.com",
                districtId: district.Id
            );

            var command = new DeleteUserCommand() { Id = user.Id };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            context.Users.FirstOrDefault(s => s.Id == user.Id).Should().BeNull();
        }
    }
}
