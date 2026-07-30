using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Enums;
using Infrastructure.Persistence.Configurations;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tools.Constants;

namespace Infrastructure.Persistence.SQLServer.Seeders
{
    public class TestDataSeeder(
        WritableDbContext context,
        UserManager<UserDao> userManager,
        IOptions<DataConfiguration> dataConfig,
        TimeProvider timeProvider
    ) : SeederBase(context, userManager)
    {
        public override async Task SeedDataAsync()
        {
            if (!dataConfig.Value.SeedTest)
            {
                return;
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteInTransactionAsync(
                async () =>
                {
                    await SeedUsersAsync();
                    await SeedPollingStationAsync();
                    await SeedRegistrationRequestsAsync();
                },
                () => Task.FromResult(true)
            );
        }

        #region Seeds Data

        private async Task SeedUsersAsync()
        {
            var users = GetMockUsers();

            foreach (var user in users)
            {
                if (!_context.Users.Any(u => u.UserName == user.Item1.UserName))
                    await SeedUserAsync(user.Item1, "Secret12", user.Item2);
            }

            await _context.SaveChangesAsync();
        }

        private async Task SeedPollingStationAsync()
        {
            var pollingStations = GetMockPollingStations();
            foreach (var station in pollingStations)
            {
                var dbStation = await _context.PollingStations.FirstOrDefaultAsync(s =>
                    s.Wording == station.Wording && s.StationNumber == station.StationNumber
                );
                if (dbStation == null)
                {
                    await _context.PollingStations.AddAsync(station);
                }
                else
                {
                    dbStation.StationNumber = station.StationNumber;
                    _context.PollingStations.Update(dbStation);
                }
            }
            await _context.SaveChangesAsync();
        }

        private async Task SeedRegistrationRequestsAsync()
        {
            var constituencies = await _context
                .Districts.Include(uc => uc.UserDistricts)
                    .ThenInclude(u => u.User)
                .ToListAsync();
            var pollingStations = await _context
                .PollingStations.Include(s => s.District)
                .ToListAsync();

            var stationTest = pollingStations.FirstOrDefault(s =>
                s.Wording == "EPP Allanikro" && s.StationNumber == "01"
            );

            if (stationTest == null)
                return;

            var registrationRequests = GetMockRegistrationRequests(constituencies);
            var mockReferences = registrationRequests.Select(rr => rr.Reference).ToList();
            var existingReferences = await _context
                .RegistrationRequests.Where(rr => mockReferences.Contains(rr.Reference))
                .Select(rr => rr.Reference)
                .ToHashSetAsync();

            int sequence = 0;
            string zoneCode = stationTest.District.Code.PadLeft(5, '0');
            var now = timeProvider.GetUtcNow();

            // Listes pour le traitement par lot (Bulk preparation)
            var requestsToInsert = new List<RegistrationRequestDao>();
            var electorsToInsert = new List<ElectorDao>();

            foreach (var item in registrationRequests)
            {
                if (existingReferences.Contains(item.Reference))
                {
                    continue;
                }

                requestsToInsert.Add(item);

                if (item.Status == RegistrationStatus.Approved)
                {
                    sequence++;
                    string sequenceStr = sequence.ToString().PadLeft(6, '0');
                    long rawNumber = long.Parse($"{zoneCode}{sequenceStr}");
                    string checKey = (rawNumber % 97).ToString().PadLeft(2, '0');
                    var elector = new ElectorDao
                    {
                        Id = item.Citizen.Id,
                        RegistrationDate = now,
                        Status = ElectorStatus.Active,
                        PollingStationId = stationTest.Id,
                        PollingStation = stationTest,
                        CreatedAt = now,
                        ModifiedAt = now,
                        VoterRegistrationNumber = $"V {zoneCode} {sequenceStr} {checKey}",
                    };

                    electorsToInsert.Add(elector);
                }
            }

            if (requestsToInsert.Count > 0)
            {
                await _context.RegistrationRequests.AddRangeAsync(requestsToInsert);
            }

            if (electorsToInsert.Count > 0)
            {
                await _context.Electors.AddRangeAsync(electorsToInsert); // Correction : Ajout manquant dans ton code initial
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Mock Data

        public static IEnumerable<Tuple<UserDao, List<string>>> GetMockUsers()
        {
            return
            [
                Tuple.Create(
                    new UserDao()
                    {
                        UserName = "admin",
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john.doe@pcea.com",
                        PhoneNumber = "01 02 03 04 05",
                        EmployeeNumber = "221167P",
                        AuthProvider = AuthProvider.Email,
                        UserDistricts = [new() { DistrictId = 16 }],
                    },
                    new List<string> { AppConstants.SuperAdminRole }
                ),
                Tuple.Create(
                    new UserDao()
                    {
                        UserName = "user1",
                        FirstName = "Sam",
                        LastName = "Gamegie",
                        Email = "sam.gamegie@pcea.com",
                        PhoneNumber = "01 02 03 04 05",
                        EmployeeNumber = "221157T",
                        AuthProvider = AuthProvider.Email,
                        UserDistricts = [new() { DistrictId = 16 }],
                    },
                    new List<string> { AppConstants.OrganismAgentRole }
                ),
                Tuple.Create(
                    new UserDao()
                    {
                        UserName = "user2",
                        FirstName = "Bilbo",
                        LastName = "Baggins",
                        PhoneNumber = "01 02 03 04 05",
                        Email = "bilbo.baggins@pcea.com",
                        AuthProvider = AuthProvider.Email,
                        UserDistricts = [new() { DistrictId = 151 }],
                    },
                    new List<string> { AppConstants.ElectorRole }
                ),
                Tuple.Create(
                    new UserDao()
                    {
                        UserName = "user3",
                        FirstName = "Éowyn",
                        LastName = "Shieldmaiden",
                        PhoneNumber = "01 02 03 04 05",
                        Email = "eowyn.shieldmaiden@pcea.com",
                        AuthProvider = AuthProvider.Email,
                        UserDistricts = [new() { DistrictId = 151 }],
                    },
                    new List<string> { AppConstants.ElectorRole }
                ),
                Tuple.Create(
                    new UserDao()
                    {
                        UserName = "user4",
                        FirstName = "Éomer",
                        LastName = "RiderOfRohan",
                        PhoneNumber = "01 02 03 04 05",
                        Email = "eomer.riderofrohan@pcea.com",
                        EmployeeNumber = "221168K",
                        AuthProvider = AuthProvider.Email,
                        UserDistricts = [new() { DistrictId = 16 }],
                    },
                    new List<string> { AppConstants.OrganismAgentRole }
                ),
                Tuple.Create(
                    new UserDao()
                    {
                        UserName = "user5",
                        FirstName = "Harvey",
                        LastName = "Spector",
                        PhoneNumber = "01 02 03 04 05",
                        Email = "harvey.spector@pcea.com",
                        EmployeeNumber = "221169T",
                        AuthProvider = AuthProvider.Email,
                        UserDistricts = [new() { DistrictId = 152 }],
                    },
                    new List<string> { AppConstants.OrganismAgentRole }
                ),
            ];
        }

        public static IEnumerable<PollingStationDao> GetMockPollingStations()
        {
            return
            [
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Allanikro",
                    DistrictId = 152,
                },
                new PollingStationDao()
                {
                    StationNumber = "02",
                    Wording = "EPP Allanikro",
                    DistrictId = 152,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Anokoi-Kouamekro",
                    DistrictId = 153,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Labo",
                    DistrictId = 154,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Adjebo",
                    DistrictId = 155,
                },
                new PollingStationDao()
                {
                    StationNumber = "02",
                    Wording = "EPP Adjebo",
                    DistrictId = 155,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Takikro",
                    DistrictId = 156,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Aka Kouamekro",
                    DistrictId = 157,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Kongobo",
                    DistrictId = 158,
                },
                new PollingStationDao()
                {
                    StationNumber = "1",
                    Wording = "EPP Boli 1",
                    DistrictId = 159,
                },
                new PollingStationDao()
                {
                    StationNumber = "02",
                    Wording = "EPP Boli 1",
                    DistrictId = 159,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Boli 3",
                    DistrictId = 160,
                },
                new PollingStationDao()
                {
                    StationNumber = "02",
                    Wording = "EPP Boli 3",
                    DistrictId = 160,
                },
                new PollingStationDao()
                {
                    StationNumber = "03",
                    Wording = "EPP Boli 3",
                    DistrictId = 160,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Yoboueplissou",
                    DistrictId = 161,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Anokoi-Djezou",
                    DistrictId = 162,
                },
                new PollingStationDao()
                {
                    StationNumber = "02",
                    Wording = "EPP Anokoi-Djezou",
                    DistrictId = 162,
                },
                new PollingStationDao()
                {
                    StationNumber = "01",
                    Wording = "EPP Grodiekro",
                    DistrictId = 163,
                },
                new PollingStationDao()
                {
                    StationNumber = "02",
                    Wording = "EPP Grodiekro",
                    DistrictId = 163,
                },
            ];
        }

        public static IEnumerable<RegistrationRequestDao> GetMockRegistrationRequests(
            List<DistrictDao> constituencies
        )
        {
            if (constituencies.Count == 0)
                yield break;

            CitizenSeedData[] citizens =
            [
                new(
                    "Koné",
                    "Amadou",
                    Gender.Masculine,
                    new(1985, 3, 15),
                    "Bouaké",
                    "Ivoirienne",
                    MaritalStatus.Married,
                    "Agriculteur",
                    "Quartier Liberté, Bouaké"
                ),
                new(
                    "Touré",
                    "Fatoumata",
                    Gender.Feminine,
                    new(1992, 7, 22),
                    "Abidjan",
                    "Ivoirienne",
                    MaritalStatus.Married,
                    "Commerçante",
                    "Cocody, Abidjan"
                ),
                new(
                    "Diabaté",
                    "Ibrahim",
                    Gender.Masculine,
                    new(1978, 11, 5),
                    "Korhogo",
                    "Ivoirienne",
                    MaritalStatus.Divorced,
                    "Enseignant",
                    "Quartier Commerce, Korhogo"
                ),
                new(
                    "Coulibaly",
                    "Mariama",
                    Gender.Feminine,
                    new(1990, 4, 18),
                    "Yamoussoukro",
                    "Ivoirienne",
                    MaritalStatus.Single,
                    "Secrétaire",
                    "Habitat, Yamoussoukro"
                ),
                new(
                    "Bamba",
                    "Ousmane",
                    Gender.Masculine,
                    new(1983, 9, 1),
                    "Daloa",
                    "Ivoirienne",
                    MaritalStatus.Married,
                    "Chauffeur",
                    "Orly 1, Daloa"
                ),
                new(
                    "Traoré",
                    "Aïcha",
                    Gender.Feminine,
                    new(1995, 1, 30),
                    "Gagnoa",
                    "Ivoirienne",
                    MaritalStatus.Single,
                    "Infirmière",
                    "Résidence Kossou, Gagnoa"
                ),
                new(
                    "Diomandé",
                    "Moussa",
                    Gender.Masculine,
                    new(1972, 6, 10),
                    "San-Pédro",
                    "Ivoirienne",
                    MaritalStatus.Widowed,
                    "Pêcheur",
                    "Cité des Pêcheurs, San-Pédro"
                ),
                new(
                    "Ouattara",
                    "Aminata",
                    Gender.Feminine,
                    new(1988, 2, 25),
                    "Abengourou",
                    "Ivoirienne",
                    MaritalStatus.Married,
                    "Institutrice",
                    "Dioulakro, Abengourou"
                ),
                new(
                    "Sanogo",
                    "Seydou",
                    Gender.Masculine,
                    new(1991, 8, 14),
                    "Man",
                    "Ivoirienne",
                    MaritalStatus.Single,
                    "Mécanicien",
                    "Carrefour, Man"
                ),
                new(
                    "Gbagbo",
                    "Aya",
                    Gender.Feminine,
                    new(1980, 12, 3),
                    "Sassandra",
                    "Ivoirienne",
                    MaritalStatus.Divorced,
                    "Couturière",
                    "Quartier Lahou, Sassandra"
                ),
                new(
                    "Yao",
                    "Kouassi",
                    Gender.Masculine,
                    new(1975, 5, 20),
                    "Bondoukou",
                    "Ivoirienne",
                    MaritalStatus.Married,
                    "Fonctionnaire",
                    "Résidence Administrative, Bondoukou"
                ),
                new(
                    "Assi",
                    "Adjoua",
                    Gender.Feminine,
                    new(1993, 10, 8),
                    "Divo",
                    "Ivoirienne",
                    MaritalStatus.Single,
                    "Étudiante",
                    "Cité Universitaire, Divo"
                ),
                new(
                    "N'Guessan",
                    "Franck",
                    Gender.Masculine,
                    new(1987, 7, 17),
                    "Issia",
                    "Ivoirienne",
                    MaritalStatus.Single,
                    "Ingénieur",
                    "Zone Industrielle, Issia"
                ),
                new(
                    "Kouamé",
                    "Rosine",
                    Gender.Feminine,
                    new(1982, 3, 29),
                    "Agboville",
                    "Ivoirienne",
                    MaritalStatus.Married,
                    "Commerçante",
                    "Marché Central, Agboville"
                ),
                new(
                    "Dembélé",
                    "Bakary",
                    Gender.Masculine,
                    new(1969, 11, 12),
                    "Odienné",
                    "Ivoirienne",
                    MaritalStatus.Widowed,
                    "Éleveur",
                    "Quartier Dioulabougou, Odienné"
                ),
                new(
                    "Tchéhi",
                    "Pélagie",
                    Gender.Feminine,
                    new(1996, 6, 4),
                    "Lakota",
                    "Ivoirienne",
                    MaritalStatus.Single,
                    "Vendeuse",
                    "Quartier Résidentiel, Lakota"
                ),
                new(
                    "Méïté",
                    "Aboubakar",
                    Gender.Masculine,
                    new(1984, 2, 11),
                    "Soubré",
                    "Ivoirienne",
                    MaritalStatus.Married,
                    "Commerçant",
                    "Cité Cacaoyère, Soubré"
                ),
                new(
                    "Silué",
                    "Mariam",
                    Gender.Feminine,
                    new(1979, 9, 23),
                    "Séguéla",
                    "Ivoirienne",
                    MaritalStatus.Divorced,
                    "Infirmière",
                    "Quartier Nimboyo, Séguéla"
                ),
                new(
                    "Gnahoua",
                    "Bi Ernest",
                    Gender.Masculine,
                    new(1994, 4, 7),
                    "Tabou",
                    "Ivoirienne",
                    MaritalStatus.Single,
                    "Technicien",
                    "Bord de Mer, Tabou"
                ),
                new(
                    "Akissi",
                    "Bénédicte",
                    Gender.Feminine,
                    new(1986, 1, 16),
                    "Tiassalé",
                    "Ivoirienne",
                    MaritalStatus.Married,
                    "Enseignante",
                    "Quartier Résidence, Tiassalé"
                ),
            ];

            (
                RegistrationStatus Status,
                RegistrationRequestType Type,
                DateTimeOffset Date,
                string? Rejection
            )[] configs =
            [
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.Approved,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 1, 12, 10, 30, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.Rejected,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 1, 15, 14, 0, 0, TimeSpan.Zero),
                    "Dossier incomplet : certificat de nationalité manquant"
                ),
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 1, 20, 8, 45, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.Approved,
                    RegistrationRequestType.RegistrationDataUpdate,
                    new(2026, 2, 3, 11, 0, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 2, 8, 9, 15, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.Rejected,
                    RegistrationRequestType.RegistrationDataUpdate,
                    new(2026, 2, 14, 16, 0, 0, TimeSpan.Zero),
                    "Défaut de CNI valide"
                ),
                (
                    RegistrationStatus.Approved,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 2, 18, 10, 0, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 3, 1, 8, 30, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationDataUpdate,
                    new(2026, 3, 5, 9, 45, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.Approved,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 3, 10, 14, 30, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 3, 17, 11, 0, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.Rejected,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 3, 22, 10, 0, 0, TimeSpan.Zero),
                    "Photo non conforme"
                ),
                (
                    RegistrationStatus.Approved,
                    RegistrationRequestType.RegistrationDataUpdate,
                    new(2026, 4, 2, 9, 0, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 4, 8, 8, 0, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 4, 14, 10, 15, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.Approved,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 4, 20, 9, 30, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationDataUpdate,
                    new(2026, 5, 3, 11, 45, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.ToBeProcessed,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 5, 7, 8, 0, 0, TimeSpan.Zero),
                    null
                ),
                (
                    RegistrationStatus.Approved,
                    RegistrationRequestType.RegistrationRequest,
                    new(2026, 5, 12, 10, 0, 0, TimeSpan.Zero),
                    null
                ),
            ];

            for (int i = 0; i < citizens.Length; i++)
            {
                var cfg = configs[i];
                var constituency = constituencies[i % constituencies.Count];
                yield return BuildRegistrationRequest(
                    citizens[i],
                    constituency.Id,
                    cfg.Status,
                    cfg.Type,
                    $"DE-2026-{(i + 1):D7}",
                    cfg.Date,
                    cfg.Rejection
                );
            }
        }

        private record CitizenSeedData(
            string LastName,
            string FirstName,
            Gender Gender,
            DateTime BirthDate,
            string BirthPlace,
            string Nationality,
            MaritalStatus MaritalStatus,
            string Profession,
            string PhysicalAddress
        );

        private static RegistrationRequestDao BuildRegistrationRequest(
            CitizenSeedData data,
            long DistrictId,
            RegistrationStatus status,
            RegistrationRequestType requestType,
            string reference,
            DateTimeOffset submissionDate,
            string? rejectionReason = null
        )
        {
            var citizen = new CitizenDao
            {
                Id = Guid.NewGuid(),
                LastName = data.LastName,
                FirstName = data.FirstName,
                Gender = data.Gender,
                BirthDate = new DateTimeOffset(data.BirthDate, TimeSpan.Zero),
                BirthPlace = data.BirthPlace,
                Nationality = data.Nationality,
                MaritalStatus = data.MaritalStatus,
                Profession = data.Profession,
                PhysicalAddress = data.PhysicalAddress,
                CreatedAt = submissionDate,
                ModifiedAt = submissionDate,
            };

            var idDoc = new DocumentDao
            {
                FileName = $"CNI_OR_CERT_NAT_{data.LastName}.pdf",
                ContentType = "application/pdf",
                FileSize = 512_000,
            };

            var photoDoc = new DocumentDao
            {
                FileName = $"PHOTO_{data.LastName}.jpg",
                ContentType = "image/jpeg",
                FileSize = 128_000,
            };

            return new RegistrationRequestDao
            {
                Reference = reference,
                SubmissionDate = submissionDate,
                RequestType = requestType,
                Status = status,
                ReasonForRejection = rejectionReason,
                Citizen = citizen,
                DistrictId = DistrictId,
                RegistrationRequestDocuments =
                [
                    new()
                    {
                        RegistrationRequestDocumentType =
                            RegistrationRequestDocumentType.IdentityDocumentOrNationalCertificate,
                        Document = idDoc,
                    },
                    new()
                    {
                        RegistrationRequestDocumentType =
                            RegistrationRequestDocumentType.PassportPhoto,
                        Document = photoDoc,
                    },
                ],
            };
        }

        #endregion
    }
}
