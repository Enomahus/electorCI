import { Component, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RegistrationRequestsForManagementApiService } from '../../../services/api/registration-requests-for-management.api.service';
import {
  AppPermission,
  GetRegistrationRequestsForManagementResponse,
  GridDataQuery,
} from '../../../services/nswag/api-nswag-client';
import { AbstractRegistrationRequestsUi } from '../genric-registration-requests-ui/abstract-registration-requests-ui';

@Component({
  selector: 'app-registration-requests-for-management-ui',
  imports: [],
  templateUrl: '../genric-registration-requests-ui/genric-registration-requests-ui.html',
  styleUrl: '../genric-registration-requests-ui/genric-registration-requests-ui.scss',
})
export class RegistrationRequestsForManagementUi extends AbstractRegistrationRequestsUi<GetRegistrationRequestsForManagementResponse> {
  protected override apiService = inject(RegistrationRequestsForManagementApiService);
  protected override titleKey = 'registrationRequests.titleForManagement';
  protected override editPermissionKey = 'updateRegistrationRequestForManagement' as AppPermission;
  protected override createPermissionKey = 'createRegistrationRequestRegistration' as AppPermission;
  protected override deletePermissionKey =
    'deleteRegistrationRequestForManagement' as AppPermission;
  protected override routePrefix = 'registration-requests-for-management';
  protected override isFromManagement = true;

  protected override getData(
    query: GridDataQuery,
  ): Observable<{ data: GetRegistrationRequestsForManagementResponse[]; total: number }> {}
}
