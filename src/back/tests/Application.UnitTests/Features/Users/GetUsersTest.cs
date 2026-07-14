using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.Common.GridData;
using Application.Features.Users.GetUsers;
using Application.UnitTests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Users
{
    public class GetUsersTest : TestBase
    {
        [Fact]
        public async Task GetUsersTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var query = new GetUsersQuery();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetUsersTest_ShouldSearchGlobally()
        {
            // Arrange
            var serviceProvider = await SetupUsersAsync();

            var query = new GetUsersQuery { Search = "zorro" };

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data!.Total.Should().Be(1);
            result.Data.Data.Should().ContainSingle();
            result.Data.Data.Single().LastName.Should().Be("Zorro");
        }

        [Fact]
        public async Task GetUsersTest_ShouldFilterByField()
        {
            // Arrange
            var serviceProvider = await SetupUsersAsync();

            var query = new GetUsersQuery
            {
                Filters =
                [
                    new GridFilter
                    {
                        Field = nameof(GetUsersResponse.FirstName),
                        Operator = GridFilterOperator.Contains,
                        Value = "Alice",
                    },
                ],
            };

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data!.Data.Should().OnlyContain(u => u.FirstName == "Alice");
        }

        [Fact]
        public async Task GetUsersTest_ShouldSortDescending()
        {
            // Arrange
            var serviceProvider = await SetupUsersAsync();

            var query = new GetUsersQuery
            {
                Search = "grid-test",
                Sorts =
                [
                    new GridSort
                    {
                        Field = nameof(GetUsersResponse.LastName),
                        Direction = GridSortDirection.Descending,
                    },
                ],
            };

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result
                .Data!.Data.Select(u => u.LastName)
                .Should()
                .ContainInOrder("Zorro", "Bernard", "Albert");
        }

        [Fact]
        public async Task GetUsersTest_ShouldPaginate()
        {
            // Arrange
            var serviceProvider = await SetupUsersAsync();

            var query = new GetUsersQuery
            {
                Search = "grid-test",
                Sorts = [new GridSort { Field = nameof(GetUsersResponse.LastName) }],
                Skip = 1,
                Take = 1,
            };

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data!.Total.Should().Be(3); // total avant pagination
            result.Data.Data.Should().ContainSingle();
            result.Data.Data.Single().LastName.Should().Be("Bernard"); // 2e après tri croissant
        }

        private static async Task<IServiceProvider> SetupUsersAsync()
        {
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetUsers]);

            await CreateUserAsync(serviceProvider, "albert@grid-test", firstName: "Alice", lastName: "Albert");
            await CreateUserAsync(serviceProvider, "bernard@grid-test", firstName: "Bob", lastName: "Bernard");
            await CreateUserAsync(serviceProvider, "zorro@grid-test", firstName: "Alice", lastName: "Zorro");

            return serviceProvider;
        }
    }
}
