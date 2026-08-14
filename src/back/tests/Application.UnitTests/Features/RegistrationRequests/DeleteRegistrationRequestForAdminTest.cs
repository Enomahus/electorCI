using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.RegistrationRequests.DeleteRegistrationRequestForAdmin;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.RegistrationRequests
{
    public class DeleteRegistrationRequestForAdminTest : TestBase
    {
        [Fact]
        public async Task DeleteRegistrationRequestForAdminTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var command = new DeleteRegistrationRequestForAdminCommand(default);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task DeleteRegistrationRequestForAdminTest_ShouldReturnValidationException_WhenIdMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeleteRegistrationRequestForAdmin]
            );

            var command = new DeleteRegistrationRequestForAdminCommand(default);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteRegistrationRequestForAdminCommand.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task DeleteRegistrationRequestForAdminTest_ShouldReturnValidationException_WhenRegistrationRequestDoesNotExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeleteRegistrationRequestForAdmin]
            );

            var command = new DeleteRegistrationRequestForAdminCommand(Guid.NewGuid());

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteRegistrationRequestForAdminCommand.Id),
                ValidationErrorCode.RegistrationRequestMustExist
            );
        }

        [Fact]
        public async Task DeleteRegistrationRequestForAdminTest_ShouldSucceed_RegardlessOfOwnerAndStatus()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeleteRegistrationRequestForAdmin]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var otherUser = await CreateUserAsync(serviceProvider, email: "other@yopmail.com");
            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                otherUser.Id,
                status: RegistrationStatus.Approved
            );

            var command = new DeleteRegistrationRequestForAdminCommand(registrationRequest.Id);
            context.ChangeTracker.Clear();

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            context.RegistrationRequests.FirstOrDefault(r => r.Id == registrationRequest.Id).Should().BeNull();
        }
    }
}
