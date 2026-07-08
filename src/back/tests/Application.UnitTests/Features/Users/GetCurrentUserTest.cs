using Application.Common.Enums;
using Application.Exceptions;
using Application.Features.Users.GetCurrentUser;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Users
{
    public class GetCurrentUserTest : TestBase
    {
        [Fact]
        public async Task GetCurrentUserTest_ShouldReturnCurrentUserInfo_WhenUserHasActiveStakeholder()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
            var dateNow = timeProvider.GetUtcNow();

            var firstName = "First name";
            var lastName = "Last name";
            var email = "test@email.email";
            var phone = "+33 1 23 45 67 89";

            var district = await CreateDistrictAsync(context, isEnabled: true);

            var user = await SetupCurrentUserAsync(
                serviceProvider,
                firstName: firstName,
                lastName: lastName,
                email: email,
                phoneNumber: phone,
                districtId: district.Id,
                permissions: [AppPermission.GetUsers, AppPermission.AccessUsersAdminPage]
            );

            var query = new GetCurrentUserQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(user.Id);
            result.Data.FirstName.Should().Be(firstName);
            result.Data.LastName.Should().Be(lastName);
            result.Data.Email.Should().Be(email);
            result.Data.Phone.Should().Be(phone);
            result.Data.IsActive.Should().BeTrue();
            result.Data.CreatedAt.Should().BeCloseTo(dateNow, TimeSpan.FromSeconds(5));
            result.Data.ActivationDate.Should().NotBeNull();
            result
                .Data.Permissions.Should()
                .BeEquivalentTo([AppPermission.GetUsers, AppPermission.AccessUsersAdminPage]);
            result.Data.CurrentUserDistrict.Should().NotBeNull();
            result.Data.CurrentUserDistrict!.Wording.Should().Be(district.Wording);
            result.Data.DistrictId.Should().Be(district.Id);
        }

        [Fact]
        public async Task GetCurrentUserTest_ShouldThrowNotFoundException_WhenUserDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var query = new GetCurrentUserQuery();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(query))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetCurrentUserTest_ShouldReturnNullStakeholder_WhenUserHasNoStakeholder()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var user = await SetupCurrentUserAsync(
                serviceProvider,
                districtId: null,
                permissions: [AppPermission.GetUsers]
            );

            var query = new GetCurrentUserQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(user.Id);
            result.Data.CurrentUserDistrict.Should().BeNull();
            result.Data.DistrictId.Should().BeNull();
        }

        [Fact]
        public async Task GetCurrentUserTest_ShouldReturnNullStakeholder_WhenDistrictIsDisabled()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
            var dateNow = timeProvider.GetUtcNow();

            var district = await CreateDistrictAsync(
                context,
                isEnabled: false,
                dateNow: dateNow.AddDays(-1)
            );

            var user = await SetupCurrentUserAsync(
                serviceProvider,
                districtId: district.Id,
                permissions: [AppPermission.GetUsers]
            );

            var query = new GetCurrentUserQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(user.Id);
            result.Data.CurrentUserDistrict.Should().BeNull();
            result.Data.DistrictId.Should().Be(district.Id);
        }

        [Fact]
        public async Task GetCurrentUserTest_ShouldReturnEmptyPermissions_WhenUserHasNoPermissions()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var user = await SetupCurrentUserAsync(serviceProvider, permissions: []);

            var query = new GetCurrentUserQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(user.Id);
            result.Data.Permissions.Should().BeEmpty();
        }

        [Fact]
        public async Task GetCurrentUserTest_ShouldReturnUserWithAuthProvider_WhenUserHasExternalAuth()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var user = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.GetUsers]
            );

            // Update user with auth provider
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            user.AuthProvider = AuthProvider.Google;
            await context.SaveChangesAsync();

            var query = new GetCurrentUserQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(user.Id);
            result.Data.AuthProvider.Should().Be(AuthProvider.Google);
        }

        [Fact]
        public async Task GetCurrentUserTest_ShouldReturnInactiveUser_WhenUserIsDisabled()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();
            var dateNow = timeProvider.GetUtcNow();

            var user = await SetupCurrentUserAsync(serviceProvider, permissions: []);

            // Disable user
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            user.DisabledDate = dateNow.AddDays(-1);
            await context.SaveChangesAsync();

            var query = new GetCurrentUserQuery();

            // Act
            var result = await serviceProvider.SendAsync(query);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data!.Id.Should().Be(user.Id);
            result.Data.IsActive.Should().BeFalse();
        }
    }
}
