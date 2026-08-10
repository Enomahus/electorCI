import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RegistrationRequestsForAdminApiService } from '../../../services/api/registration-requests-for-admin.api.service';
import { RegistrationRequestsForManagementApiService } from '../../../services/api/registration-requests-for-management.api.service';
import { RegistrationRequestsApiService } from '../../../services/api/registration-requests.api.service';
import { AppPermission } from '../../../services/nswag/api-nswag-client';
import { BaseTable } from '../../../shared/base-table/base-table';

@Component({
  standalone: true,
  providers: [DatePipe],
  template: '',
})
export abstract class AbstractRegistrationRequestsUi<TResponse>
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

  protected isDeleting = false;
  protected updateRoutePrefix?: string;

  constructor() {
    super();
  }

  ngOnInit(): void {
    this.updateRoutePrefix = this.isFromManagement
      ? '/registration-requests-for-management'
      : '/registration-requests/';
  }
}
