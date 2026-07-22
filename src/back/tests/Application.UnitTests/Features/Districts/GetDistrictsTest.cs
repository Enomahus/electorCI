using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.Common.GridData;
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

        //    [Fact]
        //    public async Task GetDistrictsTest_ShouldReturnOnlyRootDistricts_WithChildrenAndPollingStations()
        //    {
        //        // Arrange
        //        var serviceProvider = await SetupDistrictsAsync();

        //        // On isole le tout premier root grâce à un libellé unique.
        //        var query = new GetDistrictsQuery { Search = $"{Token} Alpha" };

        //        // Act
        //        var result = await serviceProvider.SendAsync(query);

        //        // Assert
        //        result.Data!.Should().ContainSingle();
        //        var root = result.Data.Single();
        //        root.Wording.Should().Be($"{Token} Alpha");
        //        root.Level.Should().Be(ElectoralDistrictLevel.Region);
        //        root.ParentId.Should().BeNull();

        //        root.Children.Should().ContainSingle();
        //        root.Children.Single().Wording.Should().Be($"{Token} Alpha Child");

        //        root.PollingStations.Should().ContainSingle();
        //        root.PollingStations.Single().Wording.Should().Be("PS-One");
        //    }

        //    [Fact]
        //    public async Task GetDistrictsTest_ShouldSearchGlobally()
        //    {
        //        // Arrange
        //        var serviceProvider = await SetupDistrictsAsync();

        //        var query = new GetDistrictsQuery { Search = Token };

        //        // Act
        //        var result = await serviceProvider.SendAsync(query);

        //        // Assert
        //        //result.Data!.Should().Be(3);
        //        result.Data.Should().OnlyContain(d => d.Wording.Contains(Token));
        //    }

        //    [Fact]
        //    public async Task GetDistrictsTest_ShouldFilterByField()
        //    {
        //        // Arrange
        //        var serviceProvider = await SetupDistrictsAsync();

        //        var query = new GetDistrictsQuery
        //        {
        //            Filters =
        //            [
        //                new GridFilter
        //                {
        //                    Field = nameof(GetDistrictsResponse.Wording),
        //                    Operator = GridFilterOperator.Contains,
        //                    Value = $"{Token} Bravo",
        //                },
        //            ],
        //        };

        //        // Act
        //        var result = await serviceProvider.SendAsync(query);

        //        // Assert
        //        result.Data!.Should().ContainSingle();
        //        result.Data.Single().Wording.Should().Be($"{Token} Bravo");
        //    }

        //    [Fact]
        //    public async Task GetDistrictsTest_ShouldSortDescending()
        //    {
        //        // Arrange
        //        var serviceProvider = await SetupDistrictsAsync();

        //        var query = new GetDistrictsQuery
        //        {
        //            Search = Token,
        //            Sorts =
        //            [
        //                new GridSort
        //                {
        //                    Field = nameof(GetDistrictsResponse.Code),
        //                    Direction = GridSortDirection.Descending,
        //                },
        //            ],
        //        };

        //        // Act
        //        var result = await serviceProvider.SendAsync(query);

        //        // Assert
        //        result
        //            .Data!.Select(d => d.Wording)
        //            .Should()
        //            .ContainInOrder($"{Token} Charlie", $"{Token} Bravo", $"{Token} Alpha");
        //    }

        //    [Fact]
        //    public async Task GetDistrictsTest_ShouldPaginate()
        //    {
        //        // Arrange
        //        var serviceProvider = await SetupDistrictsAsync();

        //        var query = new GetDistrictsQuery
        //        {
        //            Search = Token,
        //            Sorts = [new GridSort { Field = nameof(GetDistrictsResponse.Code) }],
        //            Skip = 1,
        //            Take = 1,
        //        };

        //        // Act
        //        var result = await serviceProvider.SendAsync(query);

        //        // Assert
        //        //result.Data!.Total.Should().Be(3); // total avant pagination
        //        result.Data.Should().ContainSingle();
        //        result.Data.Single().Wording.Should().Be($"{Token} Bravo"); // 2e après tri croissant
        //    }

        //    private static async Task<IServiceProvider> SetupDistrictsAsync()
        //    {
        //        var serviceProvider = CreateServiceCollection().BuildServiceProvider();
        //        await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetDistricts]);

        //        var context = serviceProvider.GetRequiredService<WritableDbContext>();

        //        var root1 = new DistrictDao
        //        {
        //            Code = "ZT001",
        //            Wording = $"{Token} Alpha",
        //            Level = ElectoralDistrictLevel.Region,
        //            Subconstituency =
        //            [
        //                new DistrictDao
        //                {
        //                    Code = "ZT001001",
        //                    Wording = $"{Token} Alpha Child",
        //                    Level = ElectoralDistrictLevel.Department,
        //                },
        //            ],
        //            PollingStations =
        //            [
        //                new PollingStationDao { StationNumber = "01", Wording = "PS-One" },
        //            ],
        //        };
        //        var root2 = new DistrictDao
        //        {
        //            Code = "ZT002",
        //            Wording = $"{Token} Bravo",
        //            Level = ElectoralDistrictLevel.Region,
        //        };
        //        var root3 = new DistrictDao
        //        {
        //            Code = "ZT003",
        //            Wording = $"{Token} Charlie",
        //            Level = ElectoralDistrictLevel.Region,
        //        };

        //        await context.Districts.AddRangeAsync(root1, root2, root3);
        //        await context.SaveChangesAsync();

        //        return serviceProvider;
        //    }
    }
}
