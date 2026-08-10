import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { catchError, map, Observable, of } from 'rxjs';
import { RegistrationRequestsApiService } from '../../../services/api/registration-requests.api.service';
import { PermissionDirective } from '../../../services/auth/permission.directive';
import {
  AppPermission,
  GetRegistrationRequestsResponse,
  GridDataQuery,
} from '../../../services/nswag/api-nswag-client';
import { LoaderUi } from '../../../shared/loader/loader';
import { AbstractRegistrationRequestsUi } from '../genric-registration-requests-ui/abstract-registration-requests-ui';

@Component({
  selector: 'app-registration-requests-ui',
  imports: [
    TranslatePipe,
    DatePipe,
    RouterLink,
    PermissionDirective,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    LoaderUi,
  ],
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

  override getData(
    query: GridDataQuery,
  ): Observable<{ data: GetRegistrationRequestsResponse[]; total: number }> {
    return this.apiService.getRegistrationRequests(query).pipe(
      map((res) => ({ data: res.data, total: res.total })),
      catchError(() => of({ data: [], total: 0 })),
    );
  }
}
