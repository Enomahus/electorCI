using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.RegistrationRequests.TriggerActionOnRegistrationRequest;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.RegistrationRequests
{
    public class TriggerActionOnRegistrationRequestTest : TestBase
    {
        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var command = new TriggerActionOnRegistrationRequestCommand();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldReturnValidationException_WhenRequestIdMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = default,
                NewStatus = RegistrationStatus.Approved,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(TriggerActionOnRegistrationRequestCommand.RequestId),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldReturnValidationException_WhenNewStatusIsInvalid()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = Guid.NewGuid(),
                NewStatus = (RegistrationStatus)999,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            result.Subject.Single()
                .AdditionalData.Should()
                .ContainKey(nameof(TriggerActionOnRegistrationRequestCommand.NewStatus));
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldReturnValidationException_WhenCommentMissingOnRejection()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = Guid.NewGuid(),
                NewStatus = RegistrationStatus.Rejected,
                Comment = null,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(TriggerActionOnRegistrationRequestCommand.Comment),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldReturnValidationException_WhenCommentExceedsMaxLength()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = Guid.NewGuid(),
                NewStatus = RegistrationStatus.Approved,
                Comment = new string('a', 501),
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            result.Subject.Single()
                .AdditionalData.Should()
                .ContainKey(nameof(TriggerActionOnRegistrationRequestCommand.Comment));
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldThrowNotFoundException_WhenRegistrationRequestDoesNotExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = Guid.NewGuid(),
                NewStatus = RegistrationStatus.Approved,
            };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<Application.Exceptions.NotFoundException>();
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldRejectRequest_WhenNewStatusIsRejected()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: district.Id
            );

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = registrationRequest.Id,
                NewStatus = RegistrationStatus.Rejected,
                Comment = "Dossier incomplet",
            };
            context.ChangeTracker.Clear();

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Data.Should().Be(registrationRequest.Id);
            var updated = await context.RegistrationRequests.FirstAsync(r => r.Id == registrationRequest.Id);
            updated.Status.Should().Be(RegistrationStatus.Rejected);
            updated.ReasonForRejection.Should().Be("Dossier incomplet");
            updated.LastUpdaterId.Should().Be(currentUser.Id);
            context.Electors.FirstOrDefault(e => e.Id == registrationRequest.CitizenId).Should().BeNull();
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldApproveRequestAndCreateNewPollingStation_WhenNoStationExists()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(
                context,
                code: "101",
                level: ElectoralDistrictLevel.VotingLocation
            );
            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: district.Id
            );

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = registrationRequest.Id,
                NewStatus = RegistrationStatus.Approved,
            };
            context.ChangeTracker.Clear();

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Data.Should().Be(registrationRequest.Id);
            var updated = await context.RegistrationRequests.FirstAsync(r => r.Id == registrationRequest.Id);
            updated.Status.Should().Be(RegistrationStatus.Approved);

            var pollingStation = await context.PollingStations.SingleAsync(p => p.DistrictId == district.Id);
            pollingStation.StationNumber.Should().Be("01");

            var elector = await context.Electors.SingleAsync(e => e.Id == registrationRequest.CitizenId);
            elector.Status.Should().Be(ElectorStatus.Active);
            elector.PollingStationId.Should().Be(pollingStation.Id);
            elector.VoterRegistrationNumber.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldReuseExistingPollingStation_WhenCapacityAvailable()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(
                context,
                code: "102",
                level: ElectoralDistrictLevel.VotingLocation
            );
            var existingStation = new PollingStationDao
            {
                StationNumber = "05",
                Wording = "Existing station",
                DistrictId = district.Id,
            };
            context.PollingStations.Add(existingStation);
            await context.SaveChangesAsync();

            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: district.Id
            );

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = registrationRequest.Id,
                NewStatus = RegistrationStatus.Approved,
            };
            context.ChangeTracker.Clear();

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Data.Should().Be(registrationRequest.Id);
            context.PollingStations.Count(p => p.DistrictId == district.Id).Should().Be(1);

            var elector = await context.Electors.SingleAsync(e => e.Id == registrationRequest.CitizenId);
            elector.PollingStationId.Should().Be(existingStation.Id);
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldThrowException_WhenCitizenAlreadyRegisteredAsElector()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: district.Id
            );

            var pollingStation = new PollingStationDao
            {
                StationNumber = "01",
                Wording = "Station",
                DistrictId = district.Id,
            };
            context.PollingStations.Add(pollingStation);
            await context.SaveChangesAsync();

            var now = timeProvider.GetUtcNow();
            context.Electors.Add(
                new ElectorDao
                {
                    Id = registrationRequest.CitizenId,
                    VoterRegistrationNumber = "V 00000 000001 01",
                    RegistrationDate = now,
                    Status = ElectorStatus.Active,
                    PollingStationId = pollingStation.Id,
                    CreatedAt = now,
                    ModifiedAt = now,
                }
            );
            await context.SaveChangesAsync();

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = registrationRequest.Id,
                NewStatus = RegistrationStatus.Approved,
            };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<Exception>()
                .WithMessage("*already registered as an elector*");
        }

        [Fact]
        public async Task TriggerActionOnRegistrationRequestTest_ShouldThrowInvalidOperationException_WhenDistrictIsNotVotingLocation()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.TriggerActionOnRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Region);
            var registrationRequest = await CreateRegistrationRequestAsync(
                context,
                timeProvider,
                currentUser.Id,
                districtId: district.Id
            );

            var command = new TriggerActionOnRegistrationRequestCommand
            {
                RequestId = registrationRequest.Id,
                NewStatus = RegistrationStatus.Approved,
            };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("*not a voting location*");
        }
    }
}
