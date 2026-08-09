using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.RegistrationRequests.GetRegistrationRequestsForAdmin;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.RegistrationRequests
{
    public class GetRegistrationRequestsForAdminTest : TestBase
    {
        [Fact]
        public async Task GetRegistrationRequestsForAdminTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var query = new GetRegistrationRequestsForAdminQuery();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetRegistrationRequestsForAdminTest_ShouldReturnValidationException_WhenTakeOutOfRange()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForAdmin]
            );

            var query = new GetRegistrationRequestsForAdminQuery { Take = 0 };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(GetRegistrationRequestsForAdminQuery.Take),
                ValidationErrorCode.PositiveNumber
            );
        }

        [Fact]
        public async Task GetRegistrationRequestsForAdminTest_ShouldReturnEmptyList_WhenNoRequestsExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForAdmin]
            );

            var query = new GetRegistrationRequestsForAdminQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data!.Data.Should().BeEmpty();
            result.Data!.Total.Should().Be(0);
        }

        [Fact]
        public async Task GetRegistrationRequestsForAdminTest_ShouldReturnRequestsFromAllUsers()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForAdmin]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var otherUser = await CreateUserAsync(serviceProvider, email: "other@yopmail.com");

            await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                reference: "DE-2026-0000001"
            );
            await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                otherUser.Id,
                reference: "DE-2026-0000002"
            );

            var query = new GetRegistrationRequestsForAdminQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data!.Total.Should().Be(2);
        }

        [Fact]
        public async Task GetRegistrationRequestsForAdminTest_ShouldMapFields_WhenRequestExists()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForAdmin],
                firstName: "Harvey",
                lastName: "Author"
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(
                context,
                code: "DIS1",
                name: "DistrictName",
                level: ElectoralDistrictLevel.VotingLocation
            );

            var request = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: district.Id,
                reasonForRejection: "Dossier incomplet"
            );

            var query = new GetRegistrationRequestsForAdminQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.Id.Should().Be(request.Id);
            row.RequestReference.Should().Be(request.Reference);
            row.RequestDate.Should().Be(request.SubmissionDate);
            row.RequestType.Should().Be(request.RequestType);
            row.Status.Should().Be(request.Status);
            row.DistrictId.Should().Be(district.Id);
            row.DistrictName.Should().Be(district.Wording);
            row.Comment.Should().Be("Dossier incomplet");
            row.Citizen.FirstName.Should().Be(request.Citizen.FirstName);
            row.CreatedAt.Should().Be(request.SubmissionDate);
            row.AuthorName.Should().Be("Harvey Author");
        }

        [Fact]
        public async Task GetRegistrationRequestsForAdminTest_ShouldMapEmptyComment_WhenReasonForRejectionIsNull()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForAdmin]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                reasonForRejection: null
            );

            var query = new GetRegistrationRequestsForAdminQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.Comment.Should().Be(string.Empty);
        }

        [Theory]
        [InlineData(RegistrationStatus.Draft)]
        [InlineData(RegistrationStatus.ToBeProcessed)]
        [InlineData(RegistrationStatus.Approved)]
        [InlineData(RegistrationStatus.Rejected)]
        public async Task GetRegistrationRequestsForAdminTest_ShouldAlwaysAllowDeletion_RegardlessOfStatus(
            RegistrationStatus status
        )
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForAdmin]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            await CreateRegistrationRequestAsync(context, timeProvider, currentUser.Id, status: status);

            var query = new GetRegistrationRequestsForAdminQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.CanBeDeleted.Should().BeTrue();
        }
    }
}
