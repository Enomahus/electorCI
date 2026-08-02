using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Districts.ToggleActiveDistrict;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Districts
{
    public class ToggleActiveDistrictTest : TestBase
    {
        [Fact]
        public async Task ToggleActiveDistrictTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var command = new ToggleActiveDistrictCommand(default);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task ToggleActiveDistrictTest_ShouldReturnValidationException_WhenIdMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);

            var command = new ToggleActiveDistrictCommand(default);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ToggleActiveDistrictCommand.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task ToggleActiveDistrictTest_ShouldReturnValidationException_WhenDistrictDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);

            var command = new ToggleActiveDistrictCommand(999999);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(ToggleActiveDistrictCommand.Id),
                ValidationErrorCode.DistrictMustExist
            );
        }

        [Fact]
        public async Task ToggleActiveDistrictTest_ShouldDisableDistrict_WhenDistrictIsActive()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, isEnabled: true);

            var command = new ToggleActiveDistrictCommand(district.Id);

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var updated = await context.Districts.FirstAsync(d => d.Id == district.Id);
            updated.DisabledDate.Should().Be(timeProvider.GetUtcNow());
        }

        [Fact]
        public async Task ToggleActiveDistrictTest_ShouldEnableDistrict_WhenDistrictIsDisabled()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(
                context,
                isEnabled: false,
                dateNow: timeProvider.GetUtcNow()
            );

            var command = new ToggleActiveDistrictCommand(district.Id);

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var updated = await context.Districts.FirstAsync(d => d.Id == district.Id);
            updated.DisabledDate.Should().BeNull();
        }
    }
}
