import { DatePipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { finalize, takeUntil } from 'rxjs';
import { RegistrationRequestsForAdminApiService } from '../../../services/api/registration-requests-for-admin.api.service';
import { RegistrationRequestsForManagementApiService } from '../../../services/api/registration-requests-for-management.api.service';
import { RegistrationRequestsApiService } from '../../../services/api/registration-requests.api.service';
import {
  AppPermission,
  GetRegistrationRequestsResponseModel,
} from '../../../services/nswag/api-nswag-client';
import { BaseTable } from '../../../shared/base-table/base-table';
import { ConfirmDialogUi } from '../../../shared/confirm-dialog/confirm-dialog';

@Component({
  standalone: true,
  providers: [DatePipe],
  template: '',
})
export abstract class AbstractRegistrationRequestsUi<TResponse extends GetRegistrationRequestsResponseModel>
  extends BaseTable<TResponse>
  implements OnInit
{
  protected abstract titleKey: string;
  protected abstract createPermissionKey: AppPermission;
  protected abstract editPermissionKey: AppPermission;
  protected abstract deletePermissionKey: AppPermission;
  protected abstract routePrefix: string;
  protected abstract apiService:
    | RegistrationRequestsApiService
    | RegistrationRequestsForAdminApiService
    | RegistrationRequestsForManagementApiService;
  protected abstract isFromManagement: boolean;

  protected displayedColumns = signal<string[]>([
    'requestReference',
    'citizen',
    'requestType',
    'districtName',
    'requestDate',
    'status',
    'actions',
  ]);

  protected readonly translateService = inject(TranslateService);
  protected readonly dialog = inject(MatDialog);

  protected isDeleting = signal(false);
  protected updateRoutePrefix?: string;

  constructor() {
    super();
  }

  ngOnInit(): void {
    this.updateRoutePrefix = this.isFromManagement
      ? '/registration-requests-for-management'
      : '/registration-requests/';
  }

  /** Overridden by subclasses whose response includes an author (admin / management views). */
  protected getAuthorName(_element: TResponse): string | undefined {
    return undefined;
  }

  protected onDelete(element: TResponse): void {
    if (!element.canBeDeleted || this.isDeleting()) {
      return;
    }

    const dialogRef = this.dialog.open(ConfirmDialogUi, {
      width: '400px',
      data: { name: element.requestReference },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (!result) {
        return;
      }

      this.isDeleting.set(true);
      this.apiService
        .deleteRegistrationRequest(element.id, {
          successMessage: this.translateService.instant('registrationRequests.deleteSuccess'),
          errorMessage: this.translateService.instant('registrationRequests.deleteError'),
        })
        .pipe(
          finalize(() => this.isDeleting.set(false)),
          takeUntil(this.destroy$),
        )
        .subscribe({
          next: () => this.refreshData(),
          error: () => undefined,
        });
    });
  }
}
