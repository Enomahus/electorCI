import { Component, inject, Injector, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Breadcrumb } from '../../../../models/breadcrumb.model';
import { UserApiService } from '../../../../services/api/user.api.service';
import { BreadcrumbService } from '../../../../services/breadcrumb.service';
import { CreateUserCommand, UserModel } from '../../../../services/nswag/api-nswag-client';
import { createUserForm } from '../../../../shared/user-form-ui/user-form';
import { UserFormUi } from '../../../../shared/user-form-ui/user-form-ui';

@Component({
  selector: 'app-user-create-ui',
  imports: [TranslatePipe, UserFormUi],
  templateUrl: './user-create-ui.html',
  styleUrl: './user-create-ui.scss',
})
export class UserCreateUi implements OnInit {
  private readonly userService = inject(UserApiService);
  private breadcrumbService = inject(BreadcrumbService);
  private readonly translateService = inject(TranslateService);
  private readonly router = inject(Router);

  isSaving = signal(false);
  form = createUserForm(false);

  ngOnInit(): void {
    this.setBreadcrumb();
  }

  private setBreadcrumb(): void {
    let breadcrumbs: Breadcrumb[] = [];
    breadcrumbs = [
      {
        label: this.translateService.instant('breadcrumb.users'),
        url: `/admin/users`,
      },
      {
        label: this.translateService.instant('breadcrumb.userCreate'),
      },
    ];
    this.breadcrumbService.setBreadcrumbs(breadcrumbs);
  }

  onSubmit(user: UserModel): void {
    this.isSaving.set(true);

    const command: CreateUserCommand = { ...user };
    this.userService
      .createUser(command, {
        successMessage: this.translateService.instant('user.successCreating'),
        errorMessage: this.translateService.instant('user.errorCreating'),
      })
      .subscribe({
        next: () => {
          this.isSaving.set(false);
          this.goBack();
        },
        error: () => {
          this.isSaving.set(false);
        },
      });
  }

  goBack(): void {
    this.router.navigate(['admin', 'users']);
  }
}
