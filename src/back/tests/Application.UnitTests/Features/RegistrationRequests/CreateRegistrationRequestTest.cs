using Application.Common.Enums;
using Application.Exceptions.Auth;
using Application.Features.Common.Citizen;
using Application.Features.RegistrationRequests.Common;
using Application.Features.RegistrationRequests.CreateRegistrationRequest;
using Application.Models.Errors;
using Application.UnitTests.Common;
using FluentAssertions;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ValidationException = Application.Exceptions.ValidationException;

namespace Application.UnitTests.Features.RegistrationRequests
{
    public class CreateRegistrationRequestTest : TestBase
    {
        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldFail_WhenPermissionMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection(mockAuthorization: false).BuildServiceProvider();

            var command = new CreateRegistrationRequestCommand();

            // Act & Assert
            await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<UserAccessException>();
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldReturnValidationException_WhenRegistrationRequestMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.CreateRegistrationRequest]
            );

            var command = new CreateRegistrationRequestCommand();

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegistrationRequestCommandBase.RegistrationRequest),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldReturnValidationException_WhenDistrictIdMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.CreateRegistrationRequest]
            );
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var registrationRequest = BuildValidRegistrationRequestModel(null, timeProvider);

            var command = new CreateRegistrationRequestCommand { RegistrationRequest = registrationRequest };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegistrationRequestModel.DistrictId),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldReturnValidationException_WhenDistrictDoesNotExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.CreateRegistrationRequest]
            );
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var registrationRequest = BuildValidRegistrationRequestModel(999999, timeProvider);

            var command = new CreateRegistrationRequestCommand { RegistrationRequest = registrationRequest };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegistrationRequestModel.DistrictId),
                ValidationErrorCode.DistrictMustExist
            );
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldReturnValidationException_WhenCitizenFirstNameMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.CreateRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var registrationRequest = BuildValidRegistrationRequestModel(district.Id, timeProvider);
            registrationRequest.Citizen!.FirstName = null;

            var command = new CreateRegistrationRequestCommand { RegistrationRequest = registrationRequest };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(result.Subject, nameof(CitizenModel.FirstName), ValidationErrorCode.Required);
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldReturnValidationException_WhenIdentityDocumentIsNull()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.CreateRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var command = new CreateRegistrationRequestCommand
            {
                RegistrationRequest = BuildValidRegistrationRequestModel(district.Id, timeProvider),
                IdentityDocument = null!,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegistrationRequestCommandBase.IdentityDocument),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldReturnValidationException_WhenPhotoIsNull()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.CreateRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var command = new CreateRegistrationRequestCommand
            {
                RegistrationRequest = BuildValidRegistrationRequestModel(district.Id, timeProvider),
                Photo = null!,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegistrationRequestCommandBase.Photo),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldReturnValidationException_WhenResidenceCertificateIsNull()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.CreateRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var command = new CreateRegistrationRequestCommand
            {
                RegistrationRequest = BuildValidRegistrationRequestModel(district.Id, timeProvider),
                ResidenceCertificate = null!,
            };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(RegistrationRequestCommandBase.ResidenceCertificate),
                ValidationErrorCode.Required
            );
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldSucceed_WhenDataIsValid()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            var currentUser = await SetupCurrentUserAsync(
                serviceProvider,
                permissions: [AppPermission.CreateRegistrationRequest]
            );
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var command = new CreateRegistrationRequestCommand
            {
                RegistrationRequest = BuildValidRegistrationRequestModel(district.Id, timeProvider),
            };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBe(Guid.Empty);

            var registrationRequest = await context
                .RegistrationRequests.Include(r => r.Citizen)
                .FirstOrDefaultAsync(r => r.Id == result.Data);
            registrationRequest.Should().NotBeNull();
            registrationRequest!.DistrictId.Should().Be(district.Id);
            registrationRequest.Status.Should().Be(RegistrationStatus.ToBeProcessed);
            registrationRequest.AuthorId.Should().Be(currentUser.Id);
            registrationRequest.LastUpdaterId.Should().Be(currentUser.Id);
            registrationRequest.Reference.Should().MatchRegex("^DE-\\d{4}-\\d{7}$");
            registrationRequest.Citizen.FirstName.Should().Be(command.RegistrationRequest.Citizen!.FirstName);

            registrationRequest.Citizen.FatherId.Should().NotBeNull();
            var father = await context.Citizens.FindAsync(registrationRequest.Citizen.FatherId);
            father.Should().NotBeNull();
            father!.FirstName.Should().Be(command.RegistrationRequest.Citizen!.NewFather!.FirstName);

            registrationRequest.Citizen.MotherId.Should().NotBeNull();
            var mother = await context.Citizens.FindAsync(registrationRequest.Citizen.MotherId);
            mother.Should().NotBeNull();
            mother!.FirstName.Should().Be(command.RegistrationRequest.Citizen!.NewMother!.FirstName);
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldSucceed_WhenFatherIdAndMotherIdAreProvided()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateRegistrationRequest]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var father = await CreateCitizenAsync(context, timeProvider, firstName: "Robert");
            var mother = await CreateCitizenAsync(context, timeProvider, firstName: "Lily");

            var registrationRequest = BuildValidRegistrationRequestModel(district.Id, timeProvider);
            registrationRequest.Citizen.NewFather = null;
            registrationRequest.Citizen.NewMother = null;
            registrationRequest.Citizen.FatherId = father.Id;
            registrationRequest.Citizen.MotherId = mother.Id;

            var command = new CreateRegistrationRequestCommand { RegistrationRequest = registrationRequest };

            // Act
            var result = await serviceProvider.SendAsync(command);

            // Assert
            var citizenCountByFirstName = await context.Citizens.CountAsync(c => c.FirstName == "Robert");
            citizenCountByFirstName.Should().Be(1);

            var registrationRequestDao = await context
                .RegistrationRequests.Include(r => r.Citizen)
                .FirstOrDefaultAsync(r => r.Id == result.Data);
            registrationRequestDao!.Citizen.FatherId.Should().Be(father.Id);
            registrationRequestDao.Citizen.MotherId.Should().Be(mother.Id);
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldReturnValidationException_WhenFatherIdDoesNotExist()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateRegistrationRequest]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var registrationRequest = BuildValidRegistrationRequestModel(district.Id, timeProvider);
            registrationRequest.Citizen.NewFather = null;
            registrationRequest.Citizen.FatherId = Guid.NewGuid();

            var command = new CreateRegistrationRequestCommand { RegistrationRequest = registrationRequest };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                nameof(CitizenModel.FatherId),
                ValidationErrorCode.CitizenMustExist
            );
        }

        [Fact]
        public async Task CreateRegistrationRequestTest_ShouldReturnValidationException_WhenFatherIdAndNewFatherAreBothMissing()
        {
            // Arrange
            var serviceProvider = CreateServiceCollection().BuildServiceProvider();
            await SetupCurrentUserAsync(serviceProvider, permissions: [AppPermission.CreateRegistrationRequest]);
            var context = serviceProvider.GetRequiredService<WritableDbContext>();
            var timeProvider = serviceProvider.GetRequiredService<TimeProvider>();

            var district = await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation);
            var registrationRequest = BuildValidRegistrationRequestModel(district.Id, timeProvider);
            registrationRequest.Citizen.NewFather = null;

            var command = new CreateRegistrationRequestCommand { RegistrationRequest = registrationRequest };

            // Act
            var result = await FluentActions
                .Invoking(() => serviceProvider.SendAsync(command))
                .Should()
                .ThrowAsync<ValidationException>();

            // Assert
            AssertValidationException(
                result.Subject,
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(CitizenModel.FatherId),
                    ValidationErrorCode.Required
                ),
                new KeyValuePair<string, ValidationErrorCode>(
                    nameof(CitizenModel.NewFather),
                    ValidationErrorCode.Required
                )
            );
        }

        private static RegistrationRequestModel BuildValidRegistrationRequestModel(
            long? districtId,
            TimeProvider timeProvider
        )
        {
            return new RegistrationRequestModel
            {
                DistrictId = districtId,
                RequestType = RegistrationRequestType.RegistrationRequest,
                Citizen = new CitizenModel
                {
                    Gender = Gender.Masculine,
                    FirstName = "Harvey",
                    LastName = "Specter",
                    BirthDate = timeProvider.GetUtcNow().AddYears(-25),
                    BirthPlace = "Yamoussoukro",
                    MaritalStatus = MaritalStatus.Single,
                    Nationality = "Ivoirienne",
                    PhysicalAddress = "123 Main St",
                    PostalAddress = "BP 123 Abidjan",
                    NewFather = BuildValidBasicCitizenModel(timeProvider, "Robert", "Specter"),
                    NewMother = BuildValidBasicCitizenModel(timeProvider, "Lily", "Specter"),
                },
            };
        }

        private static BasicCitizenModel BuildValidBasicCitizenModel(
            TimeProvider timeProvider,
            string firstName,
            string lastName
        )
        {
            return new BasicCitizenModel
            {
                Gender = Gender.Masculine,
                FirstName = firstName,
                LastName = lastName,
                BirthDate = timeProvider.GetUtcNow().AddYears(-50),
                BirthPlace = "Yamoussoukro",
                Nationality = "Ivoirienne",
            };
        }
    }
}
