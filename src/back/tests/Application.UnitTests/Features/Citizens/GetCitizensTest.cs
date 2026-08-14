using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.Citizens.GetCitizens;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Citizens
{
    public class GetCitizensTest : TestBase
    {
        [Fact]
        public async Task GetCitizensTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var query = new GetCitizensQuery();

            // Act & Assert
            await FluentActions.Invoking(() => serviceProvider.SendAsync(query)).Should().ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetCitizensTest_ShouldReturnEmptyList_WhenNoCitizensExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetCitizens]);

            var query = new GetCitizensQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCitizensTest_ShouldReturnCitizen_WhenCitizenHasNoParents()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetCitizens]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var citizen = await CreateCitizenAsync(context, timeProvider, firstName: "Harvey", lastName: "Specter");

            var query = new GetCitizensQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            var response = result.Data.Should().ContainSingle(c => c.Id == citizen.Id).Subject;
            response.FirstName.Should().Be(citizen.FirstName);
            response.LastName.Should().Be(citizen.LastName);
            response.Gender.Should().Be(citizen.Gender);
            response.FatherId.Should().BeNull();
            response.Father.Should().BeNull();
            response.MotherId.Should().BeNull();
            response.Mother.Should().BeNull();
        }

        [Fact]
        public async Task GetCitizensTest_ShouldReturnCitizenWithParents_WhenFatherAndMotherExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetCitizens]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var father = await CreateCitizenAsync(context, timeProvider, firstName: "Robert", lastName: "Specter");
            var mother = await CreateCitizenAsync(context, timeProvider, firstName: "Lily", lastName: "Specter");
            var citizen = await CreateCitizenAsync(
                context,
                timeProvider,
                firstName: "Harvey",
                lastName: "Specter",
                fatherId: father.Id,
                motherId: mother.Id
            );

            var query = new GetCitizensQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            var response = result.Data.Should().ContainSingle(c => c.Id == citizen.Id).Subject;
            response.FatherId.Should().Be(father.Id);
            response.Father.Should().NotBeNull();
            response.Father!.FirstName.Should().Be(father.FirstName);
            response.MotherId.Should().Be(mother.Id);
            response.Mother.Should().NotBeNull();
            response.Mother!.FirstName.Should().Be(mother.FirstName);
        }

        [Fact]
        public async Task GetCitizensTest_ShouldReturnAllCitizens_WhenMultipleCitizensExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetCitizens]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var firstCitizen = await CreateCitizenAsync(context, timeProvider, firstName: "Harvey");
            var secondCitizen = await CreateCitizenAsync(context, timeProvider, firstName: "Mike");

            var query = new GetCitizensQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data.Should().HaveCount(2);
            result.Data.Should().Contain(c => c.Id == firstCitizen.Id);
            result.Data.Should().Contain(c => c.Id == secondCitizen.Id);
        }
    }
}
