using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.PollingStation.ToggleActivatePollingStation;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.PollingStation
{
    public class ToggleActivatePollingStationTest : TestBase
    {
        [Fact]
        public async Task ToggleActivatePollingStationTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var command = new ToggleActivatePollingStationCommand(default);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task ToggleActivatePollingStationTest_ShouldReturnValidationException_WhenIdMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.UpdatePollingStation]
            );

            var command = new ToggleActivatePollingStationCommand(default);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ToggleActivatePollingStationCommand.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task ToggleActivatePollingStationTest_ShouldReturnValidationException_WhenPollingStationDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.UpdatePollingStation]
            );

            var command = new ToggleActivatePollingStationCommand(999999);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ToggleActivatePollingStationCommand.Id),
                ValidationErrorCode.PollingStationMustExist
            );
        }

        [Fact]
        public async Task ToggleActivatePollingStationTest_ShouldDisableStation_WhenStationIsActive()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.UpdatePollingStation]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var pollingStation = new PollingStationDao
            {
                StationNumber = "01",
                Wording = "Station",
                DistrictId = district.Id,
                DisabledDate = null,
            };
            context.PollingStations.Add(pollingStation);
            await context.SaveChangesAsync();

            var command = new ToggleActivatePollingStationCommand(pollingStation.Id);

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var updated = await context.PollingStations.FirstAsync(p => p.Id == pollingStation.Id);
            updated.DisabledDate.Should().Be(timeProvider.GetUtcNow());
        }

        [Fact]
        public async Task ToggleActivatePollingStationTest_ShouldEnableStation_WhenStationIsDisabled()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.UpdatePollingStation]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var pollingStation = new PollingStationDao
            {
                StationNumber = "01",
                Wording = "Station",
                DistrictId = district.Id,
                DisabledDate = timeProvider.GetUtcNow(),
            };
            context.PollingStations.Add(pollingStation);
            await context.SaveChangesAsync();

            var command = new ToggleActivatePollingStationCommand(pollingStation.Id);

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var updated = await context.PollingStations.FirstAsync(p => p.Id == pollingStation.Id);
            updated.DisabledDate.Should().BeNull();
        }
    }
}
