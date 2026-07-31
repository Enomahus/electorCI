using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.PollingStation.UpdatePollingStation;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.PollingStation
{
    public class UpdatePollingStationTest : TestBase
    {
        [Fact]
        public async Task UpdatePollingStationTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var command = new UpdatePollingStationCommand();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task UpdatePollingStationTest_ShouldReturnValidationException_WhenRequiredPropertiesMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.UpdatePollingStation]
            );

            var command = new UpdatePollingStationCommand();

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(UpdatePollingStationCommand.Wording),
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(UpdatePollingStationCommand.DistrictId),
                    ValidationErrorCode.Required
                )
            );
        }

        [Fact]
        public async Task UpdatePollingStationTest_ShouldReturnValidationException_WhenWordingTooLong()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.UpdatePollingStation]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);

            var command = new UpdatePollingStationCommand
            {
                Wording = new string('a', 101),
                DistrictId = district.Id,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(UpdatePollingStationCommand.Wording),
                ValidationErrorCode.MaxLength
            );
        }

        [Fact]
        public async Task UpdatePollingStationTest_ShouldReturnValidationException_WhenDistrictIsNotVotingLocation()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.UpdatePollingStation]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Department);

            var command = new UpdatePollingStationCommand { Wording = "Station", DistrictId = district.Id };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(result.Subject, string.Empty, ValidationErrorCode.InvalidLevel);
        }

        [Fact]
        public async Task UpdatePollingStationTest_ShouldThrowNotFoundException_WhenPollingStationDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.UpdatePollingStation]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);

            var command = new UpdatePollingStationCommand
            {
                Id = 999999,
                Wording = "Station",
                DistrictId = district.Id,
            };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdatePollingStationTest_ShouldSucceed()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.UpdatePollingStation]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var oldDistrict = await CreateDistrictAsync(
                context,
                name: "OldDistrict",
                level: ElectoralDistrictLevel.VotingLocation
            );
            var newDistrict = await CreateDistrictAsync(
                context,
                name: "NewDistrict",
                level: ElectoralDistrictLevel.VotingLocation
            );
            var pollingStation = new PollingStationDao
            {
                StationNumber = "01",
                Wording = "OriginalName",
                DistrictId = oldDistrict.Id,
            };
            context.PollingStations.Add(pollingStation);
            await context.SaveChangesAsync();

            var command = new UpdatePollingStationCommand
            {
                Id = pollingStation.Id,
                Wording = "UpdatedName",
                DistrictId = newDistrict.Id,
            };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().Be(pollingStation.Id);
            var updated = await context.PollingStations.FirstAsync(p => p.Id == pollingStation.Id);
            updated.Wording.Should().Be("UpdatedName");
            updated.DistrictId.Should().Be(newDistrict.Id);
        }
    }
}
