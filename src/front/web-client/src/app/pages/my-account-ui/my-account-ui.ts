import { Component, inject, OnInit, signal } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { UserApiService } from '../../services/api/user.api.service';
import { CurrentUserService } from '../../services/current-user.service';
import { GetCurrentUserResponse } from '../../services/nswag/api-nswag-client';
import { LoaderUi } from '../../shared/loader/loader';
import { createUserForm } from '../../shared/user-form-ui/user-form';
import { UserFormUi } from '../../shared/user-form-ui/user-form-ui';

@Component({
  selector: 'app-my-account-ui',
  imports: [TranslatePipe, UserFormUi, LoaderUi],
  templateUrl: './my-account-ui.html',
  styleUrl: './my-account-ui.scss',
})
export class MyAccountUi implements OnInit {
  private readonly userService = inject(UserApiService);
  private readonly currentUserService = inject(CurrentUserService);
  private readonly translateService = inject(TranslateService);

  constituencyId = signal<number | undefined>(undefined);
  isSaving = signal(false);
  isLoading = signal(false);
  form = createUserForm(false);
  user = signal<GetCurrentUserResponse | undefined>(undefined);

  ngOnInit(): void {
    this.isLoading.set(true);
    this.userService.getCurrentUser().subscribe({
      next: (userInfo) => {
        this.form.patchValue(
          {
            firstName: userInfo.firstName,
            lastName: userInfo.lastName,
            phone: userInfo.phone,
            email: userInfo.email,
            roles: userInfo.roles,
            employeeNumber: userInfo.employeeNumber,
            authProvider: userInfo.authProvider,
            districtId: userInfo.districtId,
          },
          { emitEvent: false },
        );
        this.user.set(userInfo);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }

  reloadUi(): void {
    window.location.reload();
  }
}
