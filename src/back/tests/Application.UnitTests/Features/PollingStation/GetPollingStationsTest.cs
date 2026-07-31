using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.PollingStation.GetPollingStations;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.PollingStation
{
    public class GetPollingStationsTest : TestBase
    {
        [Fact]
        public async Task GetPollingStationsTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var query = new GetPollingStationsQuery();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetPollingStationsTest_ShouldReturnValidationException_WhenTakeOutOfRange()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetPollingStations]
            );

            var query = new GetPollingStationsQuery { Take = 0 };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(GetPollingStationsQuery.Take),
                ValidationErrorCode.PositiveNumber
            );
        }

        [Fact]
        public async Task GetPollingStationsTest_ShouldReturnEmptyList_WhenNoPollingStationsExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetPollingStations]
            );

            var query = new GetPollingStationsQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data!.Data.Should().BeEmpty();
            result.Data!.Total.Should().Be(0);
        }

        [Fact]
        public async Task GetPollingStationsTest_ShouldReturnHierarchy_WhenPollingStationExists()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetPollingStations]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var region = await CreateDistrictAsync(
                context,
                code: "REG",
                name: "RegionName",
                level: ElectoralDistrictLevel.Region,
                parentId: null
            );
            var department = await CreateDistrictAsync(
                context,
                code: "DEP",
                name: "DepartmentName",
                level: ElectoralDistrictLevel.Department,
                parentId: region.Id
            );
            var subPrefecture = await CreateDistrictAsync(
                context,
                code: "SPF",
                name: "SubPrefectureName",
                level: ElectoralDistrictLevel.SubPrefecture,
                parentId: department.Id
            );
            var municipality = await CreateDistrictAsync(
                context,
                code: "MUN",
                name: "MunicipalityName",
                level: ElectoralDistrictLevel.Municipality,
                parentId: subPrefecture.Id
            );
            var votingLocation = await CreateDistrictAsync(
                context,
                code: "VOT",
                name: "VotingLocationName",
                level: ElectoralDistrictLevel.VotingLocation,
                parentId: municipality.Id
            );

            var pollingStation = new PollingStationDao
            {
                StationNumber = "04",
                Wording = "LYON",
                DistrictId = votingLocation.Id,
                DisabledDate = null,
            };
            context.PollingStations.Add(pollingStation);
            await context.SaveChangesAsync();

            var query = new GetPollingStationsQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data!.Total.Should().Be(1);
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.RegionId.Should().Be(region.Id);
            row.RegionCode.Should().Be(region.Code);
            row.RegionName.Should().Be(region.Wording);
            row.DepartmentId.Should().Be(department.Id);
            row.DepartmentCode.Should().Be(department.Code);
            row.SubPrefectureId.Should().Be(subPrefecture.Id);
            row.MunicipalityId.Should().Be(municipality.Id);
            row.VotingLocationId.Should().Be(votingLocation.Id);
            row.VotingLocationCode.Should().Be(votingLocation.Code);
            row.StationId.Should().Be(pollingStation.Id);
            row.StationNumber.Should().Be(pollingStation.StationNumber);
            row.IsDisabled.Should().BeFalse();
            row.DisabledDate.Should().BeNull();
        }

        [Fact]
        public async Task GetPollingStationsTest_ShouldReturnIsDisabledTrue_WhenStationIsDisabled()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetPollingStations]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var region = await CreateDistrictAsync(
                context,
                code: "REG2",
                name: "RegionName2",
                level: ElectoralDistrictLevel.Region,
                parentId: null
            );
            var department = await CreateDistrictAsync(
                context,
                code: "DEP2",
                name: "DepartmentName2",
                level: ElectoralDistrictLevel.Department,
                parentId: region.Id
            );
            var subPrefecture = await CreateDistrictAsync(
                context,
                code: "SPF2",
                name: "SubPrefectureName2",
                level: ElectoralDistrictLevel.SubPrefecture,
                parentId: department.Id
            );
            var municipality = await CreateDistrictAsync(
                context,
                code: "MUN2",
                name: "MunicipalityName2",
                level: ElectoralDistrictLevel.Municipality,
                parentId: subPrefecture.Id
            );
            var votingLocation = await CreateDistrictAsync(
                context,
                code: "VOT2",
                name: "VotingLocationName2",
                level: ElectoralDistrictLevel.VotingLocation,
                parentId: municipality.Id
            );

            var disabledDate = timeProvider.GetUtcNow();
            var pollingStation = new PollingStationDao
            {
                StationNumber = "06",
                Wording = "MARSEILLE",
                DistrictId = votingLocation.Id,
                DisabledDate = disabledDate,
            };
            context.PollingStations.Add(pollingStation);
            await context.SaveChangesAsync();

            var query = new GetPollingStationsQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            var row = result.Data!.Data.Should().ContainSingle().Subject;
            row.IsDisabled.Should().BeTrue();
            row.DisabledDate.Should().Be(disabledDate);
        }
    }
}
