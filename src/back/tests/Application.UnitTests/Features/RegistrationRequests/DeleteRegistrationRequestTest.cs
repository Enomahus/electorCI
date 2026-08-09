using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.RegistrationRequests.DeleteRegistrationRequest;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.RegistrationRequests
{
    public class DeleteRegistrationRequestTest : TestBase
    {
        [Fact]
        public async Task DeleteRegistrationRequestTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var command = new DeleteRegistrationRequestCommand(default);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task DeleteRegistrationRequestTest_ShouldReturnValidationException_WhenIdMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.DeleteRegistrationRequest]);

            var command = new DeleteRegistrationRequestCommand(default);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteRegistrationRequestCommand.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task DeleteRegistrationRequestTest_ShouldReturnValidationException_WhenRegistrationRequestDoesNotExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.DeleteRegistrationRequest]);

            var command = new DeleteRegistrationRequestCommand(Guid.NewGuid());

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteRegistrationRequestCommand.Id),
                ValidationErrorCode.RegistrationRequestMustExist
            );
        }

        [Fact]
        public async Task DeleteRegistrationRequestTest_ShouldReturnValidationException_WhenNotOwnedByCurrentUser()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeleteRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var otherUser = await CreateUserAsync(serviceProvider, email: "other@yopmail.com");
            var registrationRequest = await CreateRegistrationRequestAsync(context, timeProvider, otherUser.Id);

            var command = new DeleteRegistrationRequestCommand(registrationRequest.Id);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteRegistrationRequestCommand.Id),
                ValidationErrorCode.RegistrationRequestMustBeOwnedByUser
            );
        }

        [Theory]
        [InlineData(RegistrationStatus.Approved)]
        [InlineData(RegistrationStatus.Rejected)]
        public async Task DeleteRegistrationRequestTest_ShouldReturnValidationException_WhenStatusNotDraftOrToBeProcessed(
            RegistrationStatus status
        )
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeleteRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                status: status
            );

            var command = new DeleteRegistrationRequestCommand(registrationRequest.Id);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteRegistrationRequestCommand.Id),
                ValidationErrorCode.RegistrationRequestMustBeDraftOrToBeProcessed
            );
        }

        [Theory]
        [InlineData(RegistrationStatus.Draft)]
        [InlineData(RegistrationStatus.ToBeProcessed)]
        public async Task DeleteRegistrationRequestTest_ShouldSucceed_WhenOwnedByUserAndStatusAllowsDeletion(
            RegistrationStatus status
        )
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeleteRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                status: status
            );

            var command = new DeleteRegistrationRequestCommand(registrationRequest.Id);
            context.ChangeTracker.Clear();

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            context.RegistrationRequests.FirstOrDefault(r => r.Id == registrationRequest.Id).Should().BeNull();
            context.Citizens.FirstOrDefault(c => c.Id == registrationRequest.CitizenId).Should().BeNull();
        }
    }
}
