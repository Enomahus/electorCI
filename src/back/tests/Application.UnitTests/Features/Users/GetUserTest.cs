using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Users.GetUser;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Tools.Constants;
using Microsoft.EntityFrameworkCore;

namespace Application.UnitTests.Features.Users
{
    public class GetUserTest : TestBase
    {
        [Fact]
        public async Task GetUserTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var command = new GetUserQuery();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task GetUserTest_ShouldReturnValidationException_WhenIdInvalid()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetUser]);

            var command = new GetUserQuery();

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, string>(
                    nameof(GetUserQuery.UserId),
                    ValidationErrorCode.Required.ToString()
                )
            );
        }

        [Fact]
        public async Task GetUserTest_ShouldSucceed()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.GetUser]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var role = await context.Roles.FirstAsync(r => r.Name == AppConstants.SuperAdminRole);

            var district = await CreateDistrictAsync(context);
            var existingUser = await CreateUserAsync(
                serviceProvider,
                "test@email",
                districtId: district.Id,
                firstName: "firstname",
                lastName: "lastname",
                phoneNumber: "+33 1 02 03 04 05",
                disabledDate: timeProvider.GetUtcNow(),
                roleId: role.Id
            );

            var command = new GetUserQuery() { UserId = existingUser.Id };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var user = result.Data;
            user.Should().NotBeNull();
            user!.FirstName.Should().Be(existingUser.FirstName);
            user.LastName.Should().Be(existingUser.LastName);
            user.Email.Should().Be(existingUser.Email);
            user.Roles.Should().ContainSingle();
            user.Roles.ElementAt(0).Should().Be(role.Id);
            user.DistrictId.Should().Be(district.Id);
            user.IsActive.Should().BeFalse();
            
        }


    }
}
