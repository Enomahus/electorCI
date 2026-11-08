import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { finalize } from 'rxjs';
import { UserApiService } from '../../../services/api/user.api.service';
import { AuthService } from '../../../services/auth/auth.service';
import {
  Gender,
  PersonTitle,
  RegisterUserCommand,
  ResultOfError,
  UserModel,
} from '../../../services/nswag/api-nswag-client';
import { createUserForm, UserFormFactory } from '../../../shared/user-form-ui/user-form';
import { UserFormUi } from '../../../shared/user-form-ui/user-form-ui';
import { RegistrationRequestPrefill } from '../../registration-requests-home-ui/registration-request-ui/registration-request-form';

const civilityToGender: Record<PersonTitle, Gender> = {
  mr: 'masculine',
  mrs: 'feminine',
  ms: 'feminine',
};

@Component({
  selector: 'app-create-account-ui',
  imports: [TranslatePipe, UserFormUi],
  templateUrl: './create-account-ui.html',
  styleUrl: './create-account-ui.scss',
})
export class CreateAccountUi implements OnInit {
  private readonly userService = inject(UserApiService);
  private readonly authService = inject(AuthService);
  private readonly translateService = inject(TranslateService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  isSaving = signal(false);
  form!: UserFormFactory;

  ngOnInit(): void {
    this.form = createUserForm(false);
  }

  onSubmit(user: UserModel): void {
    this.isSaving.set(true);

    const password = this.form.value.password;
    const command: RegisterUserCommand = { ...user, password };

    this.userService
      .registerUser(command, {
        successMessage: this.translateService.instant('users.form.registerSuccess'),
        errorMessage: this.translateService.instant('users.form.registerError'),
      })
      .subscribe({
        next: () => this.loginAndRedirectToNewRegistrationRequest(user, password),
        error: (err: ResultOfError) => {
          if (err.data?.code === 'validation' && err.data?.additionalData) {
            for (const field in err.data.additionalData) {
              const validationCode = err.data.additionalData[field];
              if (validationCode === 'unique') {
                this.form.controls.email.setErrors({ unique: true });
              }
            }
          }
          this.isSaving.set(false);
        },
      });
  }

  goBack(): void {
    this.router.navigate(['/home']);
  }

  /**
   * The account the user just registered isn't signed in yet, and the new-registration-request
   * page requires authentication, so we log them in with the credentials they just entered before
   * redirecting. If that login unexpectedly fails, we still redirect: the route guard will then
   * send them to /home rather than lose their input silently.
   */
  private loginAndRedirectToNewRegistrationRequest(user: UserModel, password?: string): void {
    this.authService
      .login(user.email ?? '', password ?? '')
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => this.redirectToNewRegistrationRequest(user),
        error: () => this.redirectToNewRegistrationRequest(user),
      });
  }

  private redirectToNewRegistrationRequest(user: UserModel): void {
    const prefill: RegistrationRequestPrefill = {
      citizen: {
        gender: user.title ? civilityToGender[user.title] : undefined,
        firstName: user.firstName,
        lastName: user.lastName,
        email: user.email,
      },
      districtId: user.districtId,
      requestType: 'registrationRequest',
    };

    this.router.navigate(['/registration-requests/new'], { state: { prefill } });
  }
}
