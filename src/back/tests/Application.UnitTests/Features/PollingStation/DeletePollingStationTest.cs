using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.PollingStation.DeletePollingStation;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.PollingStation
{
    public class DeletePollingStationTest : TestBase
    {
        [Fact]
        public async Task DeletePollingStationTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var command = new DeletePollingStationCommand(default);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task DeletePollingStationTest_ShouldReturnValidationException_WhenIdMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeletePollingStation]
            );

            var command = new DeletePollingStationCommand(default);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeletePollingStationCommand.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task DeletePollingStationTest_ShouldReturnValidationException_WhenPollingStationDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeletePollingStation]
            );

            var command = new DeletePollingStationCommand(999999);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeletePollingStationCommand.Id),
                ValidationErrorCode.PollingStationMustExist
            );
        }

        [Fact]
        public async Task DeletePollingStationTest_ShouldReturnValidationException_WhenPollingStationHasElectors()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeletePollingStation]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var pollingStation = await CreatePollingStationAsync(context, district.Id);

            var now = timeProvider.GetUtcNow();
            context.Electors.Add(
                new ElectorDao
                {
                    VoterRegistrationNumber = "V001",
                    RegistrationDate = now,
                    PollingStationId = pollingStation.Id,
                    Citizen = new CitizenDao
                    {
                        Gender = Gender.Masculine,
                        LastName = "Doe",
                        FirstName = "John",
                        BirthDate = now.AddYears(-30),
                        BirthPlace = "Yamoussoukro",
                        Nationality = "Ivoirienne",
                        CreatedAt = now,
                        MaritalStatus = MaritalStatus.Single,
                        Email = "john.doe@yopmail.com",
                        ModifiedAt = now,
                    },
                }
            );
            await context.SaveChangesAsync();

            var command = new DeletePollingStationCommand(pollingStation.Id);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeletePollingStationCommand.Id),
                ValidationErrorCode.PollingStationLinked
            );
        }

        [Fact]
        public async Task DeletePollingStationTest_ShouldSucceed_WhenPollingStationHasNoElectors()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.DeletePollingStation]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var pollingStation = await CreatePollingStationAsync(context, district.Id);

            var command = new DeletePollingStationCommand(pollingStation.Id);

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            context.PollingStations.FirstOrDefault(p => p.Id == pollingStation.Id).Should().BeNull();
        }

        private static async Task<PollingStationDao> CreatePollingStationAsync(
            WritableDbContext context,
            long districtId,
            string stationNumber = "01",
            string wording = "Station"
        )
        {
            var pollingStation = new PollingStationDao
            {
                StationNumber = stationNumber,
                Wording = wording,
                DistrictId = districtId,
            };
            context.PollingStations.Add(pollingStation);
            await context.SaveChangesAsync();
            return pollingStation;
        }
    }
}
