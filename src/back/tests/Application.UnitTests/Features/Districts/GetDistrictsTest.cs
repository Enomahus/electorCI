using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.Districts.GetDistricts;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Districts
{
    public class GetDistrictsTest : TestBase
    {
        private const string Token = "ZZTEST";

        [Fact]
        public async Task GetDistrictsTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var query = new GetDistrictsQuery();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetDistrictsTest_ShouldReturnRootDistrictsWithHierarchy_WhenDataExists()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetDistricts]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var region = new DistrictDao()
            {
                Code = "REG1",
                Wording = "Region1",
                Level = ElectoralDistrictLevel.Region,
                ParentId = null,
            };
            await context.Districts.AddAsync(region);
            await context.SaveChangesAsync();

            var department = new DistrictDao()
            {
                Code = "DEP1",
                Wording = "Department1",
                Level = ElectoralDistrictLevel.Department,
                ParentId = region.Id,
            };
            await context.Districts.AddAsync(department);
            await context.SaveChangesAsync();

            var votingLocation = new DistrictDao()
            {
                Code = "VL1",
                Wording = "VotingLocation1",
                Level = ElectoralDistrictLevel.VotingLocation,
                ParentId = department.Id,
            };
            await context.Districts.AddAsync(votingLocation);
            await context.SaveChangesAsync();

            var query = new GetDistrictsQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            var districts = result.Data!.ToList();

            var regionResponse = districts.Should().ContainSingle(d => d.Id == region.Id).Subject;
            regionResponse.Code.Should().Be(region.Code);
            regionResponse.Wording.Should().Be(region.Wording);
            regionResponse.Level.Should().Be(ElectoralDistrictLevel.Region);
            regionResponse.ParentId.Should().BeNull();

            districts.Should().NotContain(d => d.Id == department.Id);
            districts.Should().NotContain(d => d.Id == votingLocation.Id);

            var departmentResponse = regionResponse
                .Children.Should()
                .ContainSingle(c => c.Id == department.Id)
                .Subject;
            departmentResponse.Code.Should().Be(department.Code);
            departmentResponse.ParentId.Should().Be(region.Id);
            departmentResponse.PollingStations.Should().BeEmpty();

            var votingLocationResponse = departmentResponse
                .Children.Should()
                .ContainSingle(c => c.Id == votingLocation.Id)
                .Subject;
            votingLocationResponse.Code.Should().Be(votingLocation.Code);
            votingLocationResponse.Level.Should().Be(ElectoralDistrictLevel.VotingLocation);
            votingLocationResponse.ParentId.Should().Be(department.Id);
            votingLocationResponse.Children.Should().BeEmpty();
            votingLocationResponse.PollingStations.Should().BeEmpty();
        }

        [Fact]
        public async Task GetDistrictsTest_ShouldReturnEmptyList_WhenNoDistrictsExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetDistricts]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            // Removing only the root districts would leave their children dangling,
            // and the InMemory provider nulls out the orphaned ParentId on save
            // (turning them into new roots), so every district must be removed instead.
            context.Districts.RemoveRange(context.Districts.ToList());
            await context.SaveChangesAsync();

            var query = new GetDistrictsQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }
    }
}
