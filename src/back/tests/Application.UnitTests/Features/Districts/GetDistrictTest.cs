using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Districts.GetDistrict;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Districts
{
    public class GetDistrictTest : TestBase
    {
        [Fact]
        public async Task GetDistrictTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var query = new GetDistrictQuery(default);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetDistrictTest_ShouldReturnValidationException_WhenIdMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetDistrict]);

            var query = new GetDistrictQuery(default);

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(GetDistrictQuery.Id),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task GetDistrictTest_ShouldThrowNotFoundException_WhenDistrictDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetDistrict]);

            var query = new GetDistrictQuery(999999);

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetDistrictTest_ShouldSucceed_WhenDistrictIsActive()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var parent = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Region);
            var district = await CreateDistrictAsync(
                context,
                code: "CODE1",
                name: "DistrictName",
                level: ElectoralDistrictLevel.Department,
                parentId: parent.Id,
                isEnabled: true
            );

            var query = new GetDistrictQuery(district.Id);

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            var response = result.Data;
            response.Should().NotBeNull();
            response!.Id.Should().Be(district.Id);
            response.Code.Should().Be(district.Code);
            response.Wording.Should().Be(district.Wording);
            response.Level.Should().Be(district.Level);
            response.ParentId.Should().Be(district.ParentId);
            response.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetDistrictTest_ShouldSucceed_WhenDistrictIsDisabled()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(
                context,
                isEnabled: false,
                dateNow: timeProvider.GetUtcNow()
            );

            var query = new GetDistrictQuery(district.Id);

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data!.IsActive.Should().BeFalse();
        }
    }
}
