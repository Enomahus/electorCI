using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.RegistrationRequests.GetRegistrationRequestsForManagement;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.RegistrationRequests
{
    public class GetRegistrationRequestsForManagementTest : TestBase
    {
        [Fact]
        public async Task GetRegistrationRequestsForManagementTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var query = new GetRegistrationRequestsForManagementQuery();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetRegistrationRequestsForManagementTest_ShouldReturnValidationException_WhenTakeOutOfRange()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForManagement]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            await AssignUserToDistrictAsync(context, currentUser.Id, await CreateRegionAsync(context));

            var query = new GetRegistrationRequestsForManagementQuery { Take = 0 };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(GetRegistrationRequestsForManagementQuery.Take),
                ValidationErrorCode.PositiveNumber
            );
        }

        [Fact]
        public async Task GetRegistrationRequestsForManagementTest_ShouldReturnEmptyList_WhenUserHasNoDistrict()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForManagement]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var region = await CreateRegionAsync(context);
            await CreateRegistrationRequestAsync(context, timeProvider, currentUser.Id, districtId: region.Id);

            var query = new GetRegistrationRequestsForManagementQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data!.Data.Should().BeEmpty();
            result.Data!.Total.Should().Be(0);
        }

        [Fact]
        public async Task GetRegistrationRequestsForManagementTest_ShouldReturnOnlyRequestsInUserRegion()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForManagement]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var (region, votingLocation) = await CreateFullHierarchyAsync(context, code: "IN");
            await AssignUserToDistrictAsync(context, currentUser.Id, votingLocation);

            var (otherRegion, otherVotingLocation) = await CreateFullHierarchyAsync(context, code: "OUT");

            var inRegionRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                reference: "DE-2026-0000001",
                districtId: votingLocation.Id
            );
            await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                reference: "DE-2026-0000002",
                districtId: otherVotingLocation.Id
            );

            var query = new GetRegistrationRequestsForManagementQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data!.Total.Should().Be(1);
            var row = result.Data.Data.Should().ContainSingle().Subject;
            row.Id.Should().Be(inRegionRequest.Id);
        }

        [Fact]
        public async Task GetRegistrationRequestsForManagementTest_ShouldIncludeRequestsAtRegionItself_WhenUserAssignedToRegion()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForManagement]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var region = await CreateRegionAsync(context, code: "REG-DIRECT");
            await AssignUserToDistrictAsync(context, currentUser.Id, region);

            var request = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: region.Id
            );

            var query = new GetRegistrationRequestsForManagementQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data!.Total.Should().Be(1);
            result.Data.Data.Should().ContainSingle(r => r.Id == request.Id);
        }

        [Fact]
        public async Task GetRegistrationRequestsForManagementTest_ShouldMapFields_WhenRequestExists()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForManagement],
                firstName: "Harvey",
                lastName: "Author"
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var (_, votingLocation) = await CreateFullHierarchyAsync(context, code: "MAP");
            await AssignUserToDistrictAsync(context, currentUser.Id, votingLocation);

            var request = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: votingLocation.Id,
                reasonForRejection: "Dossier incomplet"
            );

            var query = new GetRegistrationRequestsForManagementQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.Id.Should().Be(request.Id);
            row.RequestReference.Should().Be(request.Reference);
            row.RequestDate.Should().Be(request.SubmissionDate);
            row.RequestType.Should().Be(request.RequestType);
            row.Status.Should().Be(request.Status);
            row.DistrictId.Should().Be(votingLocation.Id);
            row.DistrictName.Should().Be(votingLocation.Wording);
            row.Comment.Should().Be("Dossier incomplet");
            row.Citizen.FirstName.Should().Be(request.Citizen.FirstName);
            row.CreatedAt.Should().Be(request.SubmissionDate);
            row.AuthorName.Should().Be("Harvey Author");
        }

        [Fact]
        public async Task GetRegistrationRequestsForManagementTest_ShouldMapEmptyComment_WhenReasonForRejectionIsNull()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForManagement]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var region = await CreateRegionAsync(context, code: "EMPTY-COMMENT");
            await AssignUserToDistrictAsync(context, currentUser.Id, region);

            await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: region.Id,
                reasonForRejection: null
            );

            var query = new GetRegistrationRequestsForManagementQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.Comment.Should().Be(string.Empty);
        }

        [Theory]
        [InlineData(RegistrationRequestType.RegistrationRequest, RegistrationStatus.Draft, true)]
        [InlineData(RegistrationRequestType.RegistrationRequest, RegistrationStatus.ToBeProcessed, true)]
        [InlineData(RegistrationRequestType.RegistrationRequest, RegistrationStatus.Approved, false)]
        [InlineData(RegistrationRequestType.RegistrationRequest, RegistrationStatus.Rejected, true)]
        [InlineData(RegistrationRequestType.RegistrationDataUpdate, RegistrationStatus.Draft, false)]
        [InlineData(RegistrationRequestType.RegistrationDataUpdate, RegistrationStatus.Approved, false)]
        public async Task GetRegistrationRequestsForManagementTest_ShouldComputeCanBeDeleted_BasedOnTypeAndStatus(
            RegistrationRequestType requestType,
            RegistrationStatus status,
            bool expectedCanBeDeleted
        )
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetRegistrationRequestForManagement]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var region = await CreateRegionAsync(context, code: $"CBD-{requestType}-{status}");
            await AssignUserToDistrictAsync(context, currentUser.Id, region);

            await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: region.Id,
                requestType: requestType,
                status: status
            );

            var query = new GetRegistrationRequestsForManagementQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.CanBeDeleted.Should().Be(expectedCanBeDeleted);
        }

        private static async Task<DistrictDao> CreateRegionAsync(WritableDbContext context, string code = "REG")
        {
            return await CreateDistrictAsync(
                context,
                code: code,
                name: $"Region-{code}",
                level: ElectoralDistrictLevel.Region,
                parentId: null
            );
        }

        private static async Task<(DistrictDao Region, DistrictDao VotingLocation)> CreateFullHierarchyAsync(
            WritableDbContext context,
            string code
        )
        {
            var region = await CreateRegionAsync(context, code: $"REG-{code}");
            var department = await CreateDistrictAsync(
                context,
                code: $"DEP-{code}",
                name: $"Department-{code}",
                level: ElectoralDistrictLevel.Department,
                parentId: region.Id
            );
            var subPrefecture = await CreateDistrictAsync(
                context,
                code: $"SPF-{code}",
                name: $"SubPrefecture-{code}",
                level: ElectoralDistrictLevel.SubPrefecture,
                parentId: department.Id
            );
            var municipality = await CreateDistrictAsync(
                context,
                code: $"MUN-{code}",
                name: $"Municipality-{code}",
                level: ElectoralDistrictLevel.Municipality,
                parentId: subPrefecture.Id
            );
            var votingLocation = await CreateDistrictAsync(
                context,
                code: $"VOT-{code}",
                name: $"VotingLocation-{code}",
                level: ElectoralDistrictLevel.VotingLocation,
                parentId: municipality.Id
            );

            return (region, votingLocation);
        }

        private static async Task AssignUserToDistrictAsync(
            WritableDbContext context,
            Guid userId,
            DistrictDao district
        )
        {
            await context.UserDistricts.AddAsync(new UserDistrictDao { UserId = userId, DistrictId = district.Id });
            await context.SaveChangesAsync();
        }
    }
}
