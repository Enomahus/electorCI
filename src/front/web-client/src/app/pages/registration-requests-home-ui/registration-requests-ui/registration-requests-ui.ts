import { Component, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RegistrationRequestsApiService } from '../../../services/api/registration-requests.api.service';
import {
  AppPermission,
  GetRegistrationRequestsResponse,
  GridDataQuery,
} from '../../../services/nswag/api-nswag-client';
import { AbstractRegistrationRequestsUi } from '../genric-registration-requests-ui/abstract-registration-requests-ui';

@Component({
  selector: 'app-registration-requests-ui',
  imports: [],
  templateUrl: '../genric-registration-requests-ui/genric-registration-requests-ui.html',
  styleUrl: '../genric-registration-requests-ui/genric-registration-requests-ui.scss',
})
export class RegistrationRequestsUi extends AbstractRegistrationRequestsUi<GetRegistrationRequestsResponse> {
  protected override apiService = inject(RegistrationRequestsApiService);
  protected override titleKey = 'registrationRequests.title';
  protected override editPermissionKey = 'updateRegistrationRequest' as AppPermission;
  protected override createPermissionKey = 'createRegistrationRequest' as AppPermission;
  protected override deletePermissionKey = 'deleteRegistrationRequest' as AppPermission;
  protected override routePrefix = 'registration-requests';
  protected override isFromManagement = false;

  protected override getData(
    query: GridDataQuery,
  ): Observable<{ data: GetRegistrationRequestsResponse[]; total: number }> {}
}
