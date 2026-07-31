using Application.Common.Enums;
using Application.Exceptions;
using Application.Features.PollingStation.GetPollingStation;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.PollingStation
{
    public class GetPollingStationTest : TestBase
    {
        [Fact]
        public async Task GetPollingStationTest_ShouldReturnValidationException_WhenIdMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var query = new GetPollingStationQuery { Id = default };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(GetPollingStationQuery.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task GetPollingStationTest_ShouldThrowNotFoundException_WhenPollingStationDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var query = new GetPollingStationQuery { Id = 999999 };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetPollingStationTest_ShouldSucceed_WhenStationIsActive()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var pollingStation = new PollingStationDao
            {
                StationNumber = "04",
                Wording = "LYON",
                DistrictId = district.Id,
                DisabledDate = null,
            };
            context.PollingStations.Add(pollingStation);
            await context.SaveChangesAsync();

            var query = new GetPollingStationQuery { Id = pollingStation.Id };

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            var response = result.Data;
            response.Should().NotBeNull();
            response!.Id.Should().Be(pollingStation.Id);
            response.StationNumber.Should().Be(pollingStation.StationNumber);
            response.Wording.Should().Be(pollingStation.Wording);
            response.DistrictId.Should().Be(district.Id);
            response.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetPollingStationTest_ShouldSucceed_WhenStationIsDisabled()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var pollingStation = new PollingStationDao
            {
                StationNumber = "05",
                Wording = "PARIS",
                DistrictId = district.Id,
                DisabledDate = timeProvider.GetUtcNow(),
            };
            context.PollingStations.Add(pollingStation);
            await context.SaveChangesAsync();

            var query = new GetPollingStationQuery { Id = pollingStation.Id };

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data!.IsActive.Should().BeFalse();
        }
    }
}
