using Application.Common.Enums;
using Application.Exceptions;
using Application.Exceptions.Auth;
using Application.Features.Districts.CreateDistrict;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Application.UnitTests.Features.Districts
{
    public class CreateDistrictTest : TestBase
    {
        [Fact]
        public async Task CreateDistrictTest_ShouldFail_WhenPermissionMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false)
                .BuildServiceProvider();

            var command = new CreateDistrictCommand();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task CreateDistrictTest_ShouldReturnValidationException_WhenWordingMissing()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateDistrict]);

            var command = new CreateDistrictCommand { Level = ElectoralDistrictLevel.Region };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(CreateDistrictCommand.Wording),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task CreateDistrictTest_ShouldReturnValidationException_WhenWordingTooLong()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateDistrict]);

            var command = new CreateDistrictCommand
            {
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
                nameof(CreateDistrictCommand.Wording),
                ValidationErrorCode.MaxLength
            );
        }

        [Fact]
        public async Task CreateDistrictTest_ShouldReturnValidationException_WhenWordingAlreadyExistsInParent()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var region = await CreateDistrictAsync(
                context,
                name: "ParentRegion",
                level: ElectoralDistrictLevel.Region,
                parentId: null
            );
            await CreateDistrictAsync(
                context,
                name: "ExistingDept",
                level: ElectoralDistrictLevel.Department,
                parentId: region.Id
            );

            var command = new CreateDistrictCommand
            {
                Wording = "ExistingDept",
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
                nameof(CreateDistrictCommand.Wording),
                ValidationErrorCode.AlreadyExists
            );
        }

        [Fact]
        public async Task CreateDistrictTest_ShouldReturnValidationException_WhenRegionHasParent()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var parent = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.Region);

            var command = new CreateDistrictCommand
            {
                Wording = "NewRegion",
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
                nameof(CreateDistrictCommand.ParentId),
                ValidationErrorCode.InvalidParent
            );
        }

        [Fact]
        public async Task CreateDistrictTest_ShouldReturnValidationException_WhenNonRegionHasNoParent()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateDistrict]);

            var command = new CreateDistrictCommand
            {
                Wording = "NewDepartment",
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
                nameof(CreateDistrictCommand.ParentId),
                ValidationErrorCode.DistrictMustHaveParent
            );
        }

        [Fact]
        public async Task CreateDistrictTest_ShouldReturnValidationException_WhenParentLevelIncorrect()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            // A Department's parent must be a Region; here the parent is itself a Department.
            var wrongLevelParent = await CreateDistrictAsync(
                context,
                level: ElectoralDistrictLevel.Department
            );

            var command = new CreateDistrictCommand
            {
                Wording = "NewDepartment",
                Level = ElectoralDistrictLevel.Department,
                ParentId = wrongLevelParent.Id,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                string.Empty,
                ValidationErrorCode.InvalidLevel
            );
        }

        [Fact]
        public async Task CreateDistrictTest_ShouldSucceed_WhenCreatingRegion()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var command = new CreateDistrictCommand
            {
                Wording = "BrandNewRegion",
                Level = ElectoralDistrictLevel.Region,
                ParentId = null,
            };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            var district = await context.Districts.FirstOrDefaultAsync(d => d.Id == result.Data);
            district.Should().NotBeNull();
            district!.Wording.Should().Be(command.Wording);
            district.Level.Should().Be(ElectoralDistrictLevel.Region);
            district.ParentId.Should().BeNull();
            district.Code.Should().MatchRegex("^\\d{3}$");
        }

        [Fact]
        public async Task CreateDistrictTest_ShouldSucceedAndIncrementSequence_WhenSiblingDistrictsExist()
        {
            //Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateDistrict]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var region = await CreateDistrictAsync(
                context,
                code: "REG1",
                name: "ParentRegion",
                level: ElectoralDistrictLevel.Region,
                parentId: null
            );

            var firstCommand = new CreateDistrictCommand
            {
                Wording = "DeptA",
                Level = ElectoralDistrictLevel.Department,
                ParentId = region.Id,
            };
            var secondCommand = new CreateDistrictCommand
            {
                Wording = "DeptB",
                Level = ElectoralDistrictLevel.Department,
                ParentId = region.Id,
            };

            // Act
            var firstResult = await serviceProvider.SendAsync(firstCommand);
            var secondResult = await serviceProvider.SendAsync(secondCommand);

            // Assert
            var firstDistrict = await context.Districts.FirstAsync(d => d.Id == firstResult.Data);
            var secondDistrict = await context.Districts.FirstAsync(d => d.Id == secondResult.Data);
            firstDistrict.Code.Should().Be("REG1001");
            secondDistrict.Code.Should().Be("REG1002");
        }
    }
}
