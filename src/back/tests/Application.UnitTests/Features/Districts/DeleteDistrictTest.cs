using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Districts.DeleteDistrict;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Districts
{
    public class DeleteDistrictTest : TestBase
    {
        [Fact]
        public async Task DeleteDistrictTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var command = new DeleteDistrictCommand(default);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task DeleteDistrictTest_ShouldReturnValidationException_WhenIdMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.DeleteDistrict]);

            var command = new DeleteDistrictCommand(default);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteDistrictCommand.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task DeleteDistrictTest_ShouldReturnValidationException_WhenDistrictDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.DeleteDistrict]);

            var command = new DeleteDistrictCommand(999999);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(DeleteDistrictCommand.Id),
                ValidationErrorCode.DistrictMustExist
            );
        }

        [Fact]
        public async Task DeleteDistrictTest_ShouldSucceed()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.DeleteDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context, code: "TOKILL");

            var command = new DeleteDistrictCommand(district.Id);

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            context.Districts.FirstOrDefault(d => d.Id == district.Id).Should().BeNull();
        }
    }
}
