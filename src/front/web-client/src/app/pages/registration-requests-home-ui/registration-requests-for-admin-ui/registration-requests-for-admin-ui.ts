import { Component, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RegistrationRequestsForAdminApiService } from '../../../services/api/registration-requests-for-admin.api.service';
import {
  AppPermission,
  GetRegistrationRequestsForAdminResponse,
  GridDataQuery,
} from '../../../services/nswag/api-nswag-client';
import { AbstractRegistrationRequestsUi } from '../genric-registration-requests-ui/abstract-registration-requests-ui';

@Component({
  selector: 'app-registration-requests-for-admin-ui',
  imports: [],
  templateUrl: '../genric-registration-requests-ui/genric-registration-requests-ui.html',
  styleUrl: '../genric-registration-requests-ui/genric-registration-requests-ui.scss',
})
export class RegistrationRequestsForAdminUi extends AbstractRegistrationRequestsUi<GetRegistrationRequestsForAdminResponse> {
  protected override apiService = inject(RegistrationRequestsForAdminApiService);
  protected override titleKey = 'registrationRequests.titleForAdmin';
  protected override editPermissionKey = 'updateRegistrationRequestForAdmin' as AppPermission;
  protected override createPermissionKey = 'createRegistrationRequestForAdmin' as AppPermission;
  protected override deletePermissionKey = 'deleteRegistrationRequestForAdmin' as AppPermission;
  protected override routePrefix = 'registration-requests-for-admin';
  protected override isFromManagement = false;

  protected override getData(
    query: GridDataQuery,
  ): Observable<{ data: GetRegistrationRequestsForAdminResponse[]; total: number }> {}
}
