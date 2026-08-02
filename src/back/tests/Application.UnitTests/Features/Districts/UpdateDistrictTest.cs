using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Districts.UpdateDistrict;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Districts
{
    public class UpdateDistrictTest : TestBase
    {
        [Fact]
        public async Task UpdateDistrictTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var command = new UpdateDistrictCommand();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task UpdateDistrictTest_ShouldReturnValidationException_WhenRequiredPropertiesMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);

            var command = new UpdateDistrictCommand { Level = ElectoralDistrictLevel.Region };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(UpdateDistrictCommand.Id),
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(UpdateDistrictCommand.Wording),
                    ValidationErrorCode.Required
                )
            );
        }

        [Fact]
        public async Task UpdateDistrictTest_ShouldReturnValidationException_WhenWordingTooLong()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Region);

            var command = new UpdateDistrictCommand
            {
                Id = district.Id,
                Wording = new string('a', 51),
                Level = ElectoralDistrictLevel.Region,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(UpdateDistrictCommand.Wording),
                ValidationErrorCode.MaxLength
            );
        }

        [Fact]
        public async Task UpdateDistrictTest_ShouldReturnValidationException_WhenWordingAlreadyExistsInParent()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var region = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Region);
            var department1 = await CreateDistrictAsync(
                context,
                name: "Department1",
                level: ElectoralDistrictLevel.Department,
                parentId: region.Id
            );
            var department2 = await CreateDistrictAsync(
                context,
                name: "Department2",
                level: ElectoralDistrictLevel.Department,
                parentId: region.Id
            );

            var command = new UpdateDistrictCommand
            {
                Id = department2.Id,
                Wording = department1.Wording,
                Level = ElectoralDistrictLevel.Department,
                ParentId = region.Id,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(UpdateDistrictCommand.Wording),
                ValidationErrorCode.AlreadyExists
            );
        }

        [Fact]
        public async Task UpdateDistrictTest_ShouldReturnValidationException_WhenRegionHasParent()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var parent = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Region);
            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Region);

            var command = new UpdateDistrictCommand
            {
                Id = district.Id,
                Wording = "UpdatedRegion",
                Level = ElectoralDistrictLevel.Region,
                ParentId = parent.Id,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(UpdateDistrictCommand.ParentId),
                ValidationErrorCode.InvalidParent
            );
        }

        [Fact]
        public async Task UpdateDistrictTest_ShouldReturnValidationException_WhenNonRegionHasNoParent()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Department);

            var command = new UpdateDistrictCommand
            {
                Id = district.Id,
                Wording = "UpdatedDepartment",
                Level = ElectoralDistrictLevel.Department,
                ParentId = null,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(UpdateDistrictCommand.ParentId),
                ValidationErrorCode.DistrictMustHaveParent
            );
        }

        [Fact]
        public async Task UpdateDistrictTest_ShouldReturnValidationException_WhenParentLevelIncorrect()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Department);
            var wrongLevelParent = await CreateDistrictAsync(
                context,
                level: ElectoralDistrictLevel.Department
            );

            var command = new UpdateDistrictCommand
            {
                Id = district.Id,
                Wording = "UpdatedDepartment",
                Level = ElectoralDistrictLevel.Department,
                ParentId = wrongLevelParent.Id,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(result.Subject, string.Empty, ValidationErrorCode.InvalidLevel);
        }

        [Fact]
        public async Task UpdateDistrictTest_ShouldThrowNotFoundException_WhenDistrictDoesNotExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var region = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Region);

            var command = new UpdateDistrictCommand
            {
                Id = 999999,
                Wording = "SomeWording",
                Level = ElectoralDistrictLevel.Department,
                ParentId = region.Id,
            };

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateDistrictTest_ShouldSucceed()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.UpdateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var oldParent = await CreateDistrictAsync(
                context,
                name: "OldParent",
                level: ElectoralDistrictLevel.Region
            );
            var newParent = await CreateDistrictAsync(
                context,
                name: "NewParent",
                level: ElectoralDistrictLevel.Region
            );
            var district = await CreateDistrictAsync(
                context,
                name: "OriginalName",
                level: ElectoralDistrictLevel.Department,
                parentId: oldParent.Id
            );

            var command = new UpdateDistrictCommand
            {
                Id = district.Id,
                Wording = "UpdatedName",
                Level = ElectoralDistrictLevel.Department,
                ParentId = newParent.Id,
            };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().Be(district.Id);
            var updated = await context.Districts.FirstAsync(d => d.Id == district.Id);
            updated.Wording.Should().Be("UpdatedName");
            updated.Level.Should().Be(ElectoralDistrictLevel.Department);
            updated.ParentId.Should().Be(newParent.Id);
        }
    }
}
