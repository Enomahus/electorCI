using Application.Common.Enums;
using Application.Common.Interfaces.Services;
using Application.Features.Common.Citizen;
using Application.Features.Common.GridData;
using Application.Features.RegistrationRequests.Common;
using Application.Models;
using Infrastructure.Persistence.Entities;
using Infrastructure.Persistence.SQLServer.Contexts;
using Microsoft.EntityFrameworkCore;
using Pcea.Core.Net.Authorization.Application.Attributes;
using Tools.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Application.Features.RegistrationRequests.GetRegistrationRequestsForManagement
{
    [WithPermission([nameof(AppPermission.GetRegistrationRequestForManagement)])]
    public class GetRegistrationRequestsForManagementQuery
        : GetRegistrationRequestsBase<GetRegistrationRequestsForManagementResponse> { }

    public class GetRegistrationRequestsForManagementQueryValidator
        : GetRegistrationRequestsQueryValidatorBase<
            GetRegistrationRequestsForManagementQuery,
            GetRegistrationRequestsForManagementResponse
        > { }

    public class GetRegistrationRequestsForManagementQueryHandler(
        ReadOnlyDbContext context,
        ICurrentUserService currentUserService
    )
        : GetRegistrationRequestsHandlerBase<
            GetRegistrationRequestsForManagementQuery,
            GetRegistrationRequestsForManagementResponse
        >()
    {
        public override async Task<
            Result<GridDataResponse<GetRegistrationRequestsForManagementResponse>>
        > Handle(GetRegistrationRequestsForManagementQuery request, CancellationToken cancellationToken)
        {
            using var activity = ActivitySourceLog.CQRS.Start();

            var currentUser = context
                .Users.Include(u => u.UserDistricts)
                    .ThenInclude(us => us.District)
                .Single(u => u.Id == currentUserService.UserId);

            var districtIds = await GetDistrictIdsInUserRegionAsync(currentUser.Id, cancellationToken);

            var registrationRequests = await context
                .RegistrationRequests.AsNoTracking()
                .AsSplitQuery()
                .Where(r => districtIds.Contains(r.DistrictId))
                .Include(r => r.District)
                .Include(r => r.Author)
                .Include(r => r.Citizen)
                    .ThenInclude(c => c.Father)
                .Include(r => r.Citizen)
                    .ThenInclude(c => c.Mother)
                .ToListAsync(cancellationToken);

            var rows = registrationRequests.Select(MapToResponse).AsQueryable();

            var result = rows.ApplyGrid(request);
            throw new NotImplementedException();
        }

        protected override GetRegistrationRequestsForManagementResponse MapToResponse(
            RegistrationRequestDao registrationRequest
        ) =>
            new()
            {
                Id = registrationRequest.Id,
                RequestReference = registrationRequest.Reference,
                RequestDate = registrationRequest.SubmissionDate,
                RequestType = registrationRequest.RequestType,
                Status = registrationRequest.Status,
                DistrictId = registrationRequest.DistrictId,
                DistrictName = registrationRequest.District.Wording,
                Comment = registrationRequest.ReasonForRejection ?? string.Empty,
                Citizen = CitizenModel.FromDao(registrationRequest.Citizen),
                CanBeDeleted =
                    registrationRequest.RequestType == RegistrationRequestType.RegistrationRequest
                    && registrationRequest.Status != RegistrationStatus.Approved,
                CreatedAt = registrationRequest.SubmissionDate,
                AuthorName = $"{registrationRequest.Author.FirstName} {registrationRequest.Author.LastName}",
            };

        private async Task<List<long>> GetDistrictIdsInUserRegionAsync(
            Guid currentUserId,
            CancellationToken token
        )
        {
            //Remonter l'arbre pour trouver les IDs de la (ou des) région(s) de l'utilisateur.
            var userRegionIds = await context
                .UserDistricts.Where(ud => ud.UserId == currentUserId)
                .Select(ud =>
                    ud.District.Level == ElectoralDistrictLevel.Region ? ud.DistrictId
                    : ud.District.Parent!.Level == ElectoralDistrictLevel.Region ? ud.District.ParentId
                    : ud.District.Parent!.Parent!.Level == ElectoralDistrictLevel.Region
                        ? ud.District.Parent!.ParentId
                    : ud.District.Parent!.Parent!.Parent!.Level == ElectoralDistrictLevel.Region
                        ? ud.District.Parent!.Parent!.ParentId
                    : ud.District.Parent!.Parent!.Parent!.ParentId // Maximum depth (VotingLocation)
                )
                .Where(id => id.HasValue) // Sécurité contre les orphelins
                .Select(id => id!.Value)
                .Distinct()
                .ToListAsync(token);

            // Fail-fast : Si l'utilisateur n'est rattaché à aucune région, on stoppe là.
            if (userRegionIds.Count == 0)
            {
                return [];
            }

            // ÉTAPE 2 : Descendre l'arbre pour récupérer TOUS les sous-districts liés à cette région.
            // On récupère uniquement les IDs (Select) pour éviter l'instanciation des entités EF en mémoire (Tracked Entities).
            var districtIdsInRegion = await context
                .Districts.Where(d =>
                    userRegionIds.Contains(d.Id)
                    || // Est la région
                    userRegionIds.Contains(d.ParentId ?? 0)
                    || // Niveau 1 (Departement)
                    userRegionIds.Contains(d.Parent!.ParentId ?? 0)
                    || // Niveau 2 (SubPrefecture)
                    userRegionIds.Contains(d.Parent!.Parent!.ParentId ?? 0)
                    || // Niveau 3 (Municipality)
                    userRegionIds.Contains(d.Parent!.Parent!.Parent!.ParentId ?? 0) // Niveau 4 (VotingLocation)
                )
                .Select(d => d.Id)
                .ToListAsync(token);

            return districtIdsInRegion;
        }
    }
}
