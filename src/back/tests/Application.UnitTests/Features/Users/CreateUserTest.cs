using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Common.District;
using Application.Features.Users.CreateUser;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tools.Constants;

namespace Application.UnitTests.Features.Users
{
    public class CreateUserTest : TestBase
    {
        [Fact]
        public async Task CreateUserTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var command = new CreateUserCommand();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task CreateUserTest_ShouldReturnValidationException_WhenEmailAlreadyExists()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateUser]);

            var user = await CreateUserAsync(serviceProvider, email: "test@email", authProvider: AuthProvider.Email);

            var command = new CreateUserCommand() 
            {
                Email = "test@email", 
                Password = "Password123!",
                FirstName = "firstnamea",
                LastName = "lastname",
                Phone = "+225 01 02 03 04 05",
                DistrictId = 1
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>("Email", ValidationErrorCode.Unique)
            );
        }

        [Fact]
        public async Task CreateUserTest_ShouldReturnValidationException_WhenDistrictDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateUser]);

            var command = new CreateUserCommand() 
            { 
                Email = "test@email",
                Password = "Password123!",
                Phone = "+225 01 02 03 04 05",
                FirstName = "firstname", 
                LastName = "lastname", 
                DistrictId = 1000 
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>(
                    "DistrictId",
                    ValidationErrorCode.DistrictMustExist
                )
            );
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CreateUserTest_ShouldSucceed(bool isUserActive)
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateUser]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var role = await context.Roles.FirstAsync(r => r.Name == AppConstants.SuperAdminRole);

            var district = await CreateDistrictAsync(context, isEnabled: true);

            var command = new CreateUserCommand()
            {
                Email = "test@email",
                Password = "1P@ssword!",
                FirstName = "firstname",
                LastName = "lastname",
                Phone = "+33 1 02 03 04 05",
                IsActive = isUserActive,
                Roles = [role.Id],
                DistrictId = district.Id,
            };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var user = await context.Users.FirstOrDefaultAsync(s => s.UserName == command.Email);
            user.Should().NotBeNull();
            result.Data.Should().Be(user!.Id);
            user.FirstName.Should().Be(command.FirstName);
            user.LastName.Should().Be(command.LastName);
            user.Email.Should().Be(command.Email);
            user.UserRoles.Should().ContainSingle();
            user.UserRoles.ElementAt(0).RoleId.Should().Be(role.Id);
            user.UserDistricts.Should().ContainSingle();
            user.UserDistricts.ElementAt(0).DistrictId.Should().Be(district.Id);
            
            if (isUserActive)
            {
                user.DisabledDate.Should().BeNull();
            }
            else
            {
                user.DisabledDate.Should().NotBeNull();
            }
        }


        [Fact]
        public async Task CreateUserTest_ShouldSucceedWithNewDistrict()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateUser]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var role = await context.Roles.FirstAsync(r => r.Name == AppConstants.SuperAdminRole);

            var command = new CreateUserCommand()
            {
                Email = "test@email",
                Password = "1P@ssword!",
                FirstName = "firstname",
                LastName = "lastname",
                Phone = "+33 1 02 03 04 05",
                IsActive = false,
                Roles = [role.Id],
                NewDistrict = new DistrictModel()
                {
                    Code = "code",
                    Wording = "Test district",
                    Level = ElectoralDistrictLevel.VotingLocation,
                    ParentId = 151,
                    
                },
            };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var user = await context
                .Users.Include(u => u.UserDistricts)
                .ThenInclude(us => us.District)
                .FirstOrDefaultAsync(s => s.UserName == command.Email);
            user.Should().NotBeNull();
            result.Data.Should().Be(user!.Id);
            user.FirstName.Should().Be(command.FirstName);
            user.LastName.Should().Be(command.LastName);
            user.Email.Should().Be(command.Email);
            user.UserRoles.Should().ContainSingle();
            user.UserRoles.ElementAt(0).RoleId.Should().Be(role.Id);
            user.UserDistricts.Should().ContainSingle();
            var district = user.UserDistricts.ElementAt(0).District;
            district.Should().NotBeNull();
            district.Code.Should().Be(command.NewDistrict.Code);
            district.Wording.Should().Be(command.NewDistrict.Wording);
            district.Level.Should().Be(command.NewDistrict.Level);
            district.ParentId.Should().Be(command.NewDistrict.ParentId);


            
        }

    }
}
