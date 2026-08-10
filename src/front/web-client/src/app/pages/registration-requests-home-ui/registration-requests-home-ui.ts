import { Component, inject, signal } from '@angular/core';
import { map } from 'rxjs';
import { AuthService } from '../../services/auth/auth.service';
import { HideIfAdminDirective } from '../../services/auth/hide-if-admin.directive';
import { PermissionDirective } from '../../services/auth/permission.directive';
import { RegistrationRequestsForAdminUi } from './registration-requests-for-admin-ui/registration-requests-for-admin-ui';
import { RegistrationRequestsForManagementUi } from './registration-requests-for-management-ui/registration-requests-for-management-ui';
import { RegistrationRequestsUi } from './registration-requests-ui/registration-requests-ui';

@Component({
  selector: 'app-registration-requests-home-ui',
  imports: [
    RegistrationRequestsUi,
    RegistrationRequestsForAdminUi,
    RegistrationRequestsForManagementUi,
    HideIfAdminDirective,
    PermissionDirective,
  ],
  templateUrl: './registration-requests-home-ui.html',
  styleUrl: './registration-requests-home-ui.scss',
})
export class RegistrationRequestsHomeUi {
  showRequests = signal(false);
  showManagementRequests = signal(false);
  showAdminRequests = signal(false);

  private readonly authService = inject(AuthService);

  constructor() {
    this.authService
      .getPermissions()
      .pipe(
        map((permisions) => {
          if (permisions.includes('accessRegistrationRequestsForAdminPage')) {
            this.showAdminRequests.set(true);
          }
          if (permisions.includes('accessRegistrationRequestsForManagementPage')) {
            this.showManagementRequests.set(true);
          }
          if (permisions.includes('accessRegistrationRequestsPage')) {
            this.showRequests.set(true);
          }
        }),
      )
      .subscribe();
  }
}
