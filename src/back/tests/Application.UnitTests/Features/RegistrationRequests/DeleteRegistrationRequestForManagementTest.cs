using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.RegistrationRequests.DeleteRegistrationRequestForManagement;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.RegistrationRequests
{
    public class DeleteRegistrationRequestForManagementTest : TestBase
    {
        [Fact]
        public async Task DeleteRegistrationRequestForManagementTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var command = new DeleteRegistrationRequestForManagementCommand(default);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task DeleteRegistrationRequestForManagementTest_ShouldReturnValidationException_WhenIdMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeleteRegistrationRequestForManagement]
            );

            var command = new DeleteRegistrationRequestForManagementCommand(default);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteRegistrationRequestForManagementCommand.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task DeleteRegistrationRequestForManagementTest_ShouldReturnValidationException_WhenRegistrationRequestDoesNotExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeleteRegistrationRequestForManagement]
            );

            var command = new DeleteRegistrationRequestForManagementCommand(Guid.NewGuid());

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteRegistrationRequestForManagementCommand.Id),
                ValidationErrorCode.RegistrationRequestMustExist
            );
        }

        [Fact]
        public async Task DeleteRegistrationRequestForManagementTest_ShouldSucceed_RegardlessOfOwnerAndStatus()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeleteRegistrationRequestForManagement]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var otherUser = await CreateUserAsync(serviceProvider, email: "other@yopmail.com");
            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                otherUser.Id,
                status: RegistrationStatus.Rejected
            );

            var command = new DeleteRegistrationRequestForManagementCommand(registrationRequest.Id);
            context.ChangeTracker.Clear();

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            context.RegistrationRequests.FirstOrDefault(r => r.Id == registrationRequest.Id).Should().BeNull();
        }
    }
}
