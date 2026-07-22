import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { map, switchMap, tap } from 'rxjs';
import { Breadcrumb } from '../../../../models/breadcrumb.model';
import { UserApiService } from '../../../../services/api/user.api.service';
import { BreadcrumbService } from '../../../../services/breadcrumb.service';
import { UpdateUserCommand, UserModel } from '../../../../services/nswag/api-nswag-client';
import { LoaderUi } from '../../../../shared/loader/loader';
import { createUserForm } from '../../../../shared/user-form-ui/user-form';
import { UserFormUi } from '../../../../shared/user-form-ui/user-form-ui';

@Component({
  selector: 'app-user-update-ui',
  imports: [TranslatePipe, UserFormUi, LoaderUi],
  templateUrl: './user-update-ui.html',
  styleUrl: './user-update-ui.scss',
})
export class UserUpdateUi implements OnInit {
  private readonly translateService = inject(TranslateService);
  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly userService = inject(UserApiService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  isSaving = signal(false);
  user = signal<UserModel | null>(null);
  userId = signal<string | undefined>(undefined);
  form = createUserForm(true);

  ngOnInit(): void {
    this.route.params
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        map((p) => p['id']),
        tap((id) => this.userId.set(id)),
        switchMap((id) => this.userService.getUser(id)),
        tap((user) => {
          this.user.set(user);
          this.updateFormContent(user);
        }),
      )
      .subscribe();
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
        label: this.translateService.instant('breadcrumb.userEdit'),
      },
    ];
    this.breadcrumbService.setBreadcrumbs(breadcrumbs);
  }

  updateFormContent(user: UserModel): void {
    this.form.patchValue({
      firstName: user.firstName,
      lastName: user.lastName,
      phone: user.phone,
      email: user.email,
      // password: user.password,
      // confirmPassword: user.password,
      // employeNumber: user.employeNumber,
      roles: user.roles,
      districtId: user.districtId,
      authProvider: user.authProvider,
    });
  }

  onSave(user: UserModel): void {
    if (!this.userId()) return;

    const command: UpdateUserCommand = { ...user };
    this.userService
      .udpateUser(this.userId()!, command, {
        successMessage: this.translateService.instant('user.successUpdating'),
        errorMessage: this.translateService.instant('user.errorUpdating'),
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
