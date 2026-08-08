namespace Application.Common.Enums;

public enum AppPermission
{
    SuperAdmin,

    AccessUsersAdminPage,
    CreateUser,
    UpdateUser,
    DeleteUser,
    GetCurrentUser,
    GetUser,
    GetUsers,
    CheckEmailBeUnique,
    GetRoles,
    GetProfile,

    AccessDistrictsAdminPage,
    CreateDistrict,
    UpdateDistrict,
    DeleteDistrict,
    GetDistrict,
    GetDistricts,

    AccessPollingStationsAdminPage,
    CreatePollingStation,
    UpdatePollingStation,
    DeletePollingStation,
    GetPollingStation,
    GetPollingStations,

    CreateBasicCitizen,
    GetCitizens,

    CreateRegistrationRequest,
    UpdateRegistrationRequest,
    DeleteRegistrationRequest,

    GetRegistrationRequest,
    GetRegistrationRequests,

    GetRegistrationRequestForManagement,
    GetRegistrationRequestsForManagement,

    GetRegistrationRequestsForAdmin,
    GetRegistrationRequestForAdmin,
    DeleteRegistrationRequestForAdmin,

    GetRegistrationRequestForCurrentUser,
    AccessUpdateRegistrationRequest,
    AccessRegistrationRequestsForAdminPage,
    AccessRegistrationRequestsForManagementPage,
    UpdateRegistrationRequestsForManagement,
    DeleteRegistrationRequestForManagement,
    TriggerActionOnRegistrationRequest,
    CheckRegistrationReferenceBeUnique,
    AccessRegistrationRequestsPage,
    UploadRegistrationRequestTempDocument,
    UpdateRegistrationRequestDraft,

    ImportExcelData,
    ExportExcelData,
}
