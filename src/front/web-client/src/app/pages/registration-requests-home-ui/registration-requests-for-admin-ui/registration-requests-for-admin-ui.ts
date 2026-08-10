import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { catchError, map, Observable, of } from 'rxjs';
import { RegistrationRequestsForAdminApiService } from '../../../services/api/registration-requests-for-admin.api.service';
import { PermissionDirective } from '../../../services/auth/permission.directive';
import {
  AppPermission,
  GetRegistrationRequestsForAdminResponse,
  GridDataQuery,
} from '../../../services/nswag/api-nswag-client';
import { LoaderUi } from '../../../shared/loader/loader';
import { AbstractRegistrationRequestsUi } from '../genric-registration-requests-ui/abstract-registration-requests-ui';

@Component({
  selector: 'app-registration-requests-for-admin-ui',
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
export class RegistrationRequestsForAdminUi extends AbstractRegistrationRequestsUi<GetRegistrationRequestsForAdminResponse> {
  protected override apiService = inject(RegistrationRequestsForAdminApiService);
  protected override titleKey = 'registrationRequests.titleForAdmin';
  protected override editPermissionKey = 'updateRegistrationRequestForAdmin' as AppPermission;
  protected override createPermissionKey = 'createRegistrationRequestForAdmin' as AppPermission;
  protected override deletePermissionKey = 'deleteRegistrationRequestForAdmin' as AppPermission;
  protected override routePrefix = 'registration-requests-for-admin';
  protected override isFromManagement = false;

  protected override displayedColumns = signal<string[]>([
    'requestReference',
    'citizen',
    'requestType',
    'districtName',
    'requestDate',
    'authorName',
    'status',
    'actions',
  ]);

  override getData(
    query: GridDataQuery,
  ): Observable<{ data: GetRegistrationRequestsForAdminResponse[]; total: number }> {
    return this.apiService.getRegistrationRequests(query).pipe(
      map((res) => ({ data: res.data, total: res.total })),
      catchError(() => of({ data: [], total: 0 })),
    );
  }

  protected override getAuthorName(
    element: GetRegistrationRequestsForAdminResponse,
  ): string | undefined {
    return element.authorName;
  }
}
