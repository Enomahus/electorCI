using System.Globalization;
using System.Reflection;
using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Features;
using Application.Features.Districts.Common;
using Application.Features.RegistrationRequests.Common;
using Application.Features.Security.Common;
using Application.Interfaces.Services;
using Application.Models.Errors;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Infrastructure.Configurations;
using Infrastructure.ExternalAuth;
using Infrastructure.ExternalAuth.Interfaces;
using Infrastructure.Persistence.Configurations;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer;
using Infrastructure.Persistence.SQLServer.Contexts;
using Infrastructure.Persistence.SQLServer.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using Pcea.Core.Net.Authorization;
using Pcea.Core.Net.Authorization.Application.Interfaces.Services;
using Pcea.Core.Net.Authorization.Handlers;
using Pcea.Core.Net.Authorization.Interfaces.Handlers;
using Pcea.Core.Net.Authorization.Models;
using Pcea.Core.Net.Authorization.Persistence;
using Pcea.Core.Net.Authorization.Web.Interfaces.Services;
using Tools.Configuration;
using Web.Services;

namespace Application.UnitTests.Common
{
    public class TestBase
    {
        protected TestBase() { }

        public static IServiceCollection CreateServiceCollection(
            Action<TimeProvider>? setupDateService = null,
            Action<BlobClient>? setupBlobClient = null,
            Action<IFileService>? setupFileService = null,
            bool mockAuthorization = true
        )
        {
            var currentUserServiceSub = Substitute.For<ICurrentUserService>();
            var tokenRoleClaimBuilderSub = Substitute.For<ITokenRoleClaimBuilder<long>>();
            var currentUserPermissionsProviderSub = Substitute.For<ICurrentUserPermissionsProvider>();
            var currentUserEntityPermissionsProviderSub = Substitute.For<
                ICurrentUserEntityPermissionsProvider<long>
            >();

            var timeProviderSub = Substitute.For<TimeProvider>();
            BlobServiceClient blobServiceSub = ConfigureBlobServiceSubstitute(setupBlobClient);
            timeProviderSub.GetUtcNow().Returns(new DateTimeOffset(2026, 1, 1, 10, 0, 0, TimeSpan.Zero));
            var externalAuthSub = Substitute.For<IExternalAuthService>();
            var fileServiceSub = Substitute.For<IFileService>();

            setupDateService?.Invoke(timeProviderSub);
            setupFileService?.Invoke(fileServiceSub);

            var configuration = new ConfigurationBuilder().Build();

            var services = new ServiceCollection();
            services
                .AddApplicationServices()
                .AddApplicationFeaturesServices()
                .AddMediator()
                .AddDatabase(timeProviderSub)
                .AddInfrastructureIdentityServices(configuration)
                .AddPceaCoreNetAuthorization()
                .AddPceaCoreNetAuthorizationPersistence()
                .AddSingleton(currentUserServiceSub)
                .AddSingleton(tokenRoleClaimBuilderSub)
                .AddSingleton(currentUserPermissionsProviderSub)
                .AddSingleton(currentUserEntityPermissionsProviderSub)
                .AddSingleton(timeProviderSub)
                .AddSingleton(blobServiceSub)
                .AddSingleton(fileServiceSub)
                .AddScoped<ITokenService, TokenService>()
                .AddScoped<ITokenHelper, TokenHelper>()
                .AddScoped<IAuthorizationHandler, AuthorizationHandler>()
                //.AddScoped<CriteriaService>()
                //.AddScoped<PanelReferenceService>()
                .AddScoped<CitizenService>()
                .AddScoped<DistrictService>()
                .AddKeyedSingleton(ExternalAuthServiceKeys.GoogleAuthService, externalAuthSub)
                .AddKeyedSingleton(ExternalAuthServiceKeys.MicrosoftAuthService, externalAuthSub)
                .Configure<TokenConfiguration>(c => c.Secret = "DEV_SECRET_JWT_KEY_VERY_LONG_FOR_SECURITY")
                .Configure<AppConfiguration>(c =>
                {
                    c.AppUrl = "http://localhost:44026";
                })
                .Configure<DataConfiguration>(c =>
                {
                    c.Seed = true;
                    c.DefaultUserPassword = "Secret12";
                });

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentUICulture = CultureInfo.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentUICulture;

            var serviceProvider = services.BuildServiceProvider();
            var seeder = ActivatorUtilities.CreateInstance<DataSeeder>(serviceProvider);
            seeder.SeedDataAsync().Wait();

            if (mockAuthorization)
            {
                var substitute = Substitute.For<IAuthorizationHandler>();
                substitute
                    .HandleAsync()
                    .Returns(Task.FromResult(new AuthorizationResult() { IsAuthorized = true }));
                var descriptor = new ServiceDescriptor(
                    typeof(IAuthorizationHandler),
                    p => substitute,
                    ServiceLifetime.Transient
                );
                services.Replace(descriptor);
                currentUserPermissionsProviderSub
                    .IsCurrentUserAuthenticatedAsync()
                    .Returns(Task.FromResult(true));
            }

            return services;
        }

        private static BlobServiceClient ConfigureBlobServiceSubstitute(Action<BlobClient>? setupBlobClient)
        {
            var blobDownloadResult = CreateBlobDownloadStreamingResult(new MemoryStream());
            var mockResponse = Substitute.For<Response<BlobDownloadStreamingResult>>();
            mockResponse.Value.Returns(blobDownloadResult);
            mockResponse.HasValue.Returns(true);
            var blobClientSub = Substitute.For<BlobClient>();
            blobClientSub
                .DownloadStreamingAsync(Arg.Any<BlobDownloadOptions?>(), Arg.Any<CancellationToken>())
                .Returns(mockResponse);
            var blobContainerClientSub = Substitute.For<BlobContainerClient>();
            blobContainerClientSub.GetBlobClient(Arg.Any<string>()).Returns(blobClientSub);
            var blobServiceSub = Substitute.For<BlobServiceClient>();
            blobServiceSub.GetBlobContainerClient(Arg.Any<string>()).Returns(blobContainerClientSub);
            setupBlobClient?.Invoke(blobClientSub);

            return blobServiceSub;
        }

        protected static BlobDownloadStreamingResult? CreateBlobDownloadStreamingResult(Stream content)
        {
            var result = (BlobDownloadStreamingResult?)
                Activator.CreateInstance(typeof(BlobDownloadStreamingResult), nonPublic: true);
            var contentProperty = typeof(BlobDownloadStreamingResult).GetProperty(
                nameof(BlobDownloadStreamingResult.Content)
            );
            contentProperty?.SetValue(
                result,
                content,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                null,
                null
            );
            return result;
        }

        #region Data Creation

        public static async Task<UserDao> SetupCurrentUserAsync(
            IServiceProvider serviceProvider,
            string email = "dev@yopmail.com",
            string? password = "Secret1",
            long? districtId = null,
            string? firstName = null,
            string? lastName = null,
            string? phoneNumber = null,
            List<AppPermission>? permissions = null
        )
        {
            var user = await CreateUserAsync(
                serviceProvider,
                email: email,
                password: password,
                districtId: districtId,
                firstName: firstName,
                lastName: lastName,
                phoneNumber: phoneNumber
            );
            var userService = serviceProvider.GetRequiredService<ICurrentUserService>();
            userService.UserId.Returns(user.Id);
            userService.UserEmail.Returns(user.Email);
            var currentPermissionService =
                serviceProvider.GetRequiredService<ICurrentUserPermissionsProvider>();
            currentPermissionService
                .GetCurrentUserPermissionsAsync()
                .Returns(Task.FromResult(permissions?.Select(p => p.ToString()) ?? []));
            currentPermissionService.IsCurrentUserAuthenticatedAsync().Returns(Task.FromResult(true));
            return user;
        }

        protected static async Task<UserDao> CreateUserAsync(
            IServiceProvider serviceProvider,
            string? email = "user@yopmail.com",
            string? password = "Secret12",
            long? districtId = null,
            string? firstName = null,
            string? lastName = null,
            DateTimeOffset? disabledDate = null,
            string? phoneNumber = null,
            Guid? roleId = null,
            AuthProvider? authProvider = null,
            bool isActive = true
        )
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<UserDao>>();
            var context = serviceProvider.GetRequiredService<WritableDbContext>();

            var user = new UserDao()
            {
                UserName = email,
                FirstName = firstName ?? "FirstName",
                LastName = lastName ?? "LastName",
                Email = email,
                DisabledDate = disabledDate,
                PhoneNumber = phoneNumber,
                AuthProvider = authProvider,
            };
            if (districtId != null)
            {
                user.UserDistricts = [new UserDistrictDao() { DistrictId = districtId!.Value }];
            }
            if (roleId != null)
            {
                user.UserRoles = [new UserRoleDao() { RoleId = roleId!.Value }];
            }
            if (password != null)
            {
                await userManager.CreateAsync(user, password);
            }
            else
            {
                await userManager.CreateAsync(user);
            }

            await context.SaveChangesAsync();

            return user;
        }

        protected static async Task<DistrictDao> CreateDistrictAsync(
            WritableDbContext context,
            string code = "Code",
            string name = "Name",
            ElectoralDistrictLevel? level = null,
            long? parentId = null,
            bool isEnabled = true,
            DateTimeOffset dateNow = default
        )
        {
            var district = new DistrictDao()
            {
                Code = code,
                Wording = name,
                Level = level ?? ElectoralDistrictLevel.VotingLocation,
                ParentId = parentId ?? 151,
                DisabledDate = isEnabled ? null : dateNow,
            };

            await context.Districts.AddAsync(district);
            await context.SaveChangesAsync();

            return district;
        }

        protected static async Task<CitizenDao> CreateCitizenAsync(
            WritableDbContext context,
            TimeProvider timeProvider,
            string firstName = "Harvey",
            string lastName = "Specter",
            Gender gender = Gender.Masculine,
            MaritalStatus maritalStatus = MaritalStatus.Single,
            Guid? fatherId = null,
            Guid? motherId = null
        )
        {
            var now = timeProvider.GetUtcNow();

            var citizen = new CitizenDao()
            {
                Gender = gender,
                FirstName = firstName,
                LastName = lastName,
                BirthDate = now.AddYears(-30),
                BirthPlace = "Yamoussoukro",
                Nationality = "Ivoirienne",
                MaritalStatus = maritalStatus,
                Email = $"{firstName}.{lastName}@yopmail.com".ToLowerInvariant(),
                PhysicalAddress = "123 Main St",
                PostalAddress = "BP 123 Abidjan",
                FatherId = fatherId,
                MotherId = motherId,
                CreatedAt = now,
                ModifiedAt = now,
            };

            await context.Citizens.AddAsync(citizen);
            await context.SaveChangesAsync();

            return citizen;
        }

        protected static async Task<RegistrationRequestDao> CreateRegistrationRequestAsync(
            WritableDbContext context,
            TimeProvider timeProvider,
            Guid authorId,
            string reference = "DE-2026-0000001",
            long? districtId = null,
            RegistrationStatus status = RegistrationStatus.ToBeProcessed,
            RegistrationRequestType requestType = RegistrationRequestType.RegistrationRequest,
            string? reasonForRejection = ""
        )
        {
            districtId ??= (
                await CreateDistrictAsync(context, level: ElectoralDistrictLevel.VotingLocation)
            ).Id;

            var now = timeProvider.GetUtcNow();

            var registrationRequest = new RegistrationRequestDao()
            {
                AuthorId = authorId,
                RequestType = requestType,
                Status = status,
                SubmissionDate = now,
                DistrictId = districtId.Value,
                LastUpdaterId = authorId,
                ReasonForRejection = reasonForRejection,
                Reference = reference,
                Citizen = new CitizenDao()
                {
                    Gender = Gender.Masculine,
                    LastName = "Spector",
                    FirstName = "Harvey",
                    BirthDate = timeProvider.GetUtcNow().AddYears(-28),
                    BirthPlace = "Yamoussoukro",
                    Nationality = "Ivoirienne",
                    CreatedAt = now,
                    MaritalStatus = MaritalStatus.Single,
                    Email = "harvey.spector@yopmail.com",
                    ModifiedAt = now,
                },
            };

            await context.RegistrationRequests.AddAsync(registrationRequest);
            await context.SaveChangesAsync();

            return registrationRequest;
        }

        #endregion

        #region Asserts

        protected static void AssertValidationException(
            Application.Exceptions.ValidationException exception,
            string key,
            ValidationErrorCode code
        )
        {
            exception.AdditionalData.Should().ContainSingle();
            var error = exception.AdditionalData.Single();
            error.Key.Should().Be(key);
            error.Value.Should().Be(code.ToString());
        }

        protected static void AssertValidationException(
            Application.Exceptions.ValidationException exception,
            string key,
            string message
        )
        {
            exception.AdditionalData.Should().ContainSingle();
            var error = exception.AdditionalData.Single();
            error.Key.Should().Be(key);
            error.Value.Should().Be(message);
        }

        protected static void AssertValidationException(
            IEnumerable<Application.Exceptions.ValidationException> exceptions,
            string key,
            ValidationErrorCode code
        )
        {
            exceptions.Should().ContainSingle();
            var exception = exceptions.Single();
            AssertValidationException(exception, key, code);
        }

        protected static void AssertValidationException(
            IEnumerable<Application.Exceptions.ValidationException> exceptions,
            params KeyValuePair<string, ValidationErrorCode>[] values
        )
        {
            exceptions.Should().ContainSingle();
            var exception = exceptions.Single();

            var valuesGroupBy = values
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Select(v => v.Value).ToArray());

            foreach (var value in valuesGroupBy)
            {
                AssertValidationException(exception, value.Key, value.Value);
            }
        }

        protected static void AssertValidationException(
            IEnumerable<Application.Exceptions.ValidationException> exceptions,
            params KeyValuePair<string, string>[] values
        )
        {
            exceptions.Should().ContainSingle();
            var exception = exceptions.Single();

            var valuesGroupBy = values
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Select(v => v.Value).ToArray());

            foreach (var value in valuesGroupBy)
            {
                AssertValidationException(exception, value.Key, value.Value);
            }
        }

        protected static void AssertValidationException(
            Application.Exceptions.ValidationException exception,
            params KeyValuePair<string, ValidationErrorCode>[] values
        )
        {
            var valuesGroupBy = values
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Select(v => v.Value).ToArray());

            foreach (var value in valuesGroupBy)
            {
                AssertValidationException(exception, value.Key, value.Value);
            }
        }

        protected static void AssertValidationException(
            Application.Exceptions.ValidationException exception,
            params KeyValuePair<string, string>[] values
        )
        {
            var valuesGroupBy = values
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.Select(v => v.Value).ToArray());

            foreach (var value in valuesGroupBy)
            {
                AssertValidationException(exception, value.Key, value.Value);
            }
        }

        protected static void AssertValidationException(
            Application.Exceptions.ValidationException exception,
            string key,
            params ValidationErrorCode[] codes
        )
        {
            var value = string.Join(" ", codes);
            exception
                .AdditionalData.Should()
                .ContainEquivalentOf(new KeyValuePair<string, string>(key, value));
        }

        protected static void AssertValidationException(
            Application.Exceptions.ValidationException exception,
            string key,
            params string[] messages
        )
        {
            var rule = exception.AdditionalData.Any(v =>
                v.Key == key && v.Value == string.Join(" ", messages)
            );
            rule.Should().BeTrue();
        }

        #endregion
    }
}
