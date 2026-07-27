using Application.Common.Enum;
using Application.Exceptions;
using Application.Features.Users.UpdateCurrentUser;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Users
{
    public class UpdatCurrentUserTest : TestBase
    {
        [Fact]
        public async Task UpdateCurrentUserTest_ShouldReturnValidationException_WhenRequiredPropertiesMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var command = new UpdateCurrentUserCommand();

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>(
                    "FirstName",
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "LastName",
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "Email",
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "Phone",
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    "DistrictId",
                    ValidationErrorCode.Required
                )
            );
        }

        [Fact]
        public async Task UpdateCurrentUserTest_ShouldReturnValidationException_WhenEmailIsNotCurrentUser()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var user = await CreateUserAsync(serviceProvider, email: "test@email");

            var command = new UpdateCurrentUserCommand() { Email = "test@email" };

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
        public async Task UpdateCurrentUserTest_ShouldReturnValidationException_WhenDistrictDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();

            var command = new UpdateCurrentUserCommand() { DistrictId = 1000 };

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

        [Fact]
        public async Task UpdateCurrentUserTest_ShouldSucceed()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context);

            var currentUser = await SetupCurrentUserAsync(serviceProvider);

            var command = new UpdateCurrentUserCommand()
            {
                Title = PersonTitle.Mrs,
                Email = "test@email",
                FirstName = "firstname",
                LastName = "lastname",
                Phone = "+33 1 02 03 04 05",
                IsActive = false,
                EmployeeNumber = "12345M",
                DistrictId = district.Id,
            };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var user = await context.Users.FirstOrDefaultAsync(s => s.Id == currentUser.Id);
            user.Should().NotBeNull();
            result.Data.Should().Be(user!.Id);
            user.Civility.Should().Be(PersonTitle.Mrs);
            user.FirstName.Should().Be(command.FirstName);
            user.LastName.Should().Be(command.LastName);
            user.Email.Should().Be(command.Email);
            user.UserDistricts.Should().ContainSingle();
            user.UserDistricts.ElementAt(0).DistrictId.Should().Be(district.Id);
        }
    }
}
