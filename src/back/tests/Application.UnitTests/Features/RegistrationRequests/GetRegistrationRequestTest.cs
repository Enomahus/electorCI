using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.RegistrationRequests.GetRegistrationRequest;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.RegistrationRequests
{
    public class GetRegistrationRequestTest : TestBase
    {
        [Fact]
        public async Task GetRegistrationRequestTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var query = new GetRegistrationRequestQuery(default);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetRegistrationRequestTest_ShouldReturnValidationException_WhenIdMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetRegistrationRequest]);

            var query = new GetRegistrationRequestQuery(default);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(GetRegistrationRequestQuery.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task GetRegistrationRequestTest_ShouldThrowUserAccessException_WhenCurrentUserRecordNotFound()
        {
            // Arrange - permission checks are bypassed (mockAuthorization defaults to true) but no
            // current user record is seeded, so ICurrentUserService.UserId resolves to an unmatched value.
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var query = new GetRegistrationRequestQuery(Guid.NewGuid());

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetRegistrationRequestTest_ShouldThrowNotFoundException_WhenRegistrationRequestDoesNotExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetRegistrationRequest]);

            var query = new GetRegistrationRequestQuery(Guid.NewGuid());

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetRegistrationRequestTest_ShouldSucceed_WhenRegistrationRequestExists()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequest],
                firstName: "Harvey",
                lastName: "Author"
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: district.Id,
                reasonForRejection: "Dossier incomplet"
            );

            var query = new GetRegistrationRequestQuery(registrationRequest.Id);

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            var response = result.Data;
            response.Should().NotBeNull();
            response!.Id.Should().Be(registrationRequest.Id);
            response.Reference.Should().Be(registrationRequest.Reference);
            response.Status.Should().Be(registrationRequest.Status);
            response.SubmittedAt.Should().Be(registrationRequest.SubmissionDate);
            response.DistrictId.Should().Be(district.Id);
            response.DistrictName.Should().Be(district.Wording);
            response.AuthorId.Should().Be(currentUser.Id);
            response.Comment.Should().Be("Dossier incomplet");
            response.Citizen.Should().NotBeNull();
            response.Citizen.FirstName.Should().Be(registrationRequest.Citizen.FirstName);
            response.RequestDocuments.Should().BeEmpty();
        }
    }
}
