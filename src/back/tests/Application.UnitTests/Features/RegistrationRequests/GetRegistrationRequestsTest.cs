using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.RegistrationRequests.GetRegistrationRequests;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.RegistrationRequests
{
    public class GetRegistrationRequestsTest : TestBase
    {
        [Fact]
        public async Task GetRegistrationRequestsTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var query = new GetRegistrationRequestsQuery();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetRegistrationRequestsTest_ShouldReturnValidationException_WhenTakeOutOfRange()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequests]
            );

            var query = new GetRegistrationRequestsQuery { Take = 0 };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(GetRegistrationRequestsQuery.Take),
                ValidationErrorCode.PositiveNumber
            );
        }

        [Fact]
        public async Task GetRegistrationRequestsTest_ShouldReturnEmptyList_WhenNoRequestsExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequests]
            );

            var query = new GetRegistrationRequestsQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data!.Data.Should().BeEmpty();
            result.Data!.Total.Should().Be(0);
        }

        [Fact]
        public async Task GetRegistrationRequestsTest_ShouldReturnOnlyCurrentUserRequests_WhenOtherUsersHaveRequests()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequests]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var otherUser = await CreateUserAsync(serviceProvider, email: "other@yopmail.com");

            var ownRequest = await CreateRegistrationRequestAsync(
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

            var query = new GetRegistrationRequestsQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data!.Total.Should().Be(1);
            var row = result.Data.Data.Should().ContainSingle().Subject;
            row.Id.Should().Be(ownRequest.Id);
        }

        [Fact]
        public async Task GetRegistrationRequestsTest_ShouldMapFields_WhenRequestExists()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequests]
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

            var query = new GetRegistrationRequestsQuery();

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
            row.Citizen.LastName.Should().Be(request.Citizen.LastName);
            row.CreatedAt.Should().Be(request.SubmissionDate);
        }

        [Fact]
        public async Task GetRegistrationRequestsTest_ShouldMapEmptyComment_WhenReasonForRejectionIsNull()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequests]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                reasonForRejection: null
            );

            var query = new GetRegistrationRequestsQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.Comment.Should().Be(string.Empty);
        }

        [Theory]
        [InlineData(RegistrationStatus.Draft, true)]
        [InlineData(RegistrationStatus.ToBeProcessed, true)]
        [InlineData(RegistrationStatus.Approved, false)]
        [InlineData(RegistrationStatus.Rejected, false)]
        public async Task GetRegistrationRequestsTest_ShouldComputeCanBeDeleted_BasedOnStatus(
            RegistrationStatus status,
            bool expectedCanBeDeleted
        )
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequests]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            await CreateRegistrationRequestAsync(context, timeProvider, currentUser.Id, status: status);

            var query = new GetRegistrationRequestsQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.CanBeDeleted.Should().Be(expectedCanBeDeleted);
        }
    }
}
