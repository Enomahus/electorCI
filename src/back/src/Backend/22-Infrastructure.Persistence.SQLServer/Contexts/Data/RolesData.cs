using Application.Common.Enums;
using Tools.Constants;

namespace Infrastructure.Persistence.SQLServer.Contexts.Data
{
    /// <summary>
    /// Static, role-based seed of the authorization matrix.
    /// Three application roles drive the front-end navigation menus:
    ///   - Elector : creates and consults his own registration requests.
    ///   - Agent   : manages (validates / rejects) registration requests for his organism.
    ///   - Admin   : full access plus administration menus.
    /// </summary>
    public static class RolesData
    {
        public static readonly IReadOnlyDictionary<
            AppAction,
            IReadOnlyList<AppPermission>
        > ActionsSeed = new Dictionary<AppAction, IReadOnlyList<AppPermission>>
        {
            [AppAction.SuperAdmin] = Enum.GetValues<AppPermission>(),

            [AppAction.UsersAdministration] =
            [
                AppPermission.AccessUsersAdminPage,
                AppPermission.GetUser,
                AppPermission.GetUsers,
                AppPermission.CreateUser,
                AppPermission.UpdateUser,
                AppPermission.DeleteUser,
                AppPermission.GetRoles,
                AppPermission.CheckEmailBeUnique,
            ],

            [AppAction.ConstituencyAdministration] =
            [
                AppPermission.AccessConstituenciesAdminPage,
                AppPermission.GetConstituencies,
                AppPermission.GetConstituency,
                AppPermission.CreateConstituency,
                AppPermission.UpdateConstituency,
                AppPermission.DeleteConstituency,
            ],

            [AppAction.PollingStationAdministration] =
            [
                AppPermission.AccessPollingStationsAdminPage,
                AppPermission.GetPollingStation,
                AppPermission.GetPollingStations,
                AppPermission.CreatePollingStation,
                AppPermission.UpdatePollingStation,
                AppPermission.DeletePollingStation,
            ],

            // AGENT : receive, list globally and process (approve / refuse) requests.
            [AppAction.RegistrationRequestManagement] =
            [
                AppPermission.AccessRegistrationRequestsForManagementPage,
                AppPermission.GetRegistrationRequestsForManagement,
                AppPermission.UpdateRegistrationRequestsForManagement,
                AppPermission.GetRegistrationRequestForManagement,
                //AppPermission.GetRegistrationRequests,
                AppPermission.TriggerActionOnRegistrationRequest,
                AppPermission.CheckRegistrationReferenceBeUnique,
            ],

            // ELECTOR : create and update a request as long as it is not approved / refused.
            [AppAction.RegistrationRequestCreation] =
            [
                AppPermission.CreateRegistrationRequest,
                AppPermission.AccessUpdateRegistrationRequest,
                AppPermission.UpdateRegistrationRequest,
                AppPermission.UploadRegistrationRequestTempDocument,
            ],

            // ELECTOR : read-only access restricted to his own data.
            [AppAction.RegistrationRequestConsultation] =
            [
                AppPermission.AccessRegistrationRequestsPage,
                AppPermission.GetRegistrationRequests,
                AppPermission.GetRegistrationRequest,
                AppPermission.GetRegistrationRequestForCurrentUser,
            ],

            // ADMIN : full set of specific rights on every request.
            [AppAction.RegistrationRequestAdministration] =
            [
                AppPermission.AccessRegistrationRequestsForAdminPage,
                AppPermission.AccessUpdateRegistrationRequest,
                AppPermission.GetRegistrationRequest,
                AppPermission.GetRegistrationRequestsForAdmin,
                AppPermission.DeleteRegistrationRequest,
                AppPermission.GetRegistrationRequests,
                AppPermission.UpdateRegistrationRequest,
                AppPermission.TriggerActionOnRegistrationRequest,
            ],

            [AppAction.CommonAccess] =
            [
                AppPermission.GetRoles,
                AppPermission.GetProfile,
                AppPermission.GetConstituencies,
                AppPermission.GetPollingStations,
                AppPermission.GetCitizens,
                AppPermission.CreateBasicCitizen,
            ],
        };

        public static readonly IReadOnlyDictionary<string, IReadOnlyList<AppAction>> RolesSeed =
            new Dictionary<string, IReadOnlyList<AppAction>>
            {
                [AppConstants.SuperAdminRole] = [AppAction.SuperAdmin],
                [AppConstants.OrganismAgentRole] =
                [
                    AppAction.CommonAccess,
                    AppAction.RegistrationRequestManagement,
                    AppAction.PollingStationAdministration,
                ],

                [AppConstants.ElectorRole] =
                [
                    AppAction.CommonAccess,
                    AppAction.RegistrationRequestCreation,
                    AppAction.RegistrationRequestConsultation,
                ],
            };
    }
}
