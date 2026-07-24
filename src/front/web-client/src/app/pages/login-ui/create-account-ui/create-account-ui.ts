import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { UserApiService } from '../../../services/api/user.api.service';
import {
  RegisterUserCommand,
  ResultOfError,
  UserModel,
} from '../../../services/nswag/api-nswag-client';
import { createUserForm, UserFormFactory } from '../../../shared/user-form-ui/user-form';
import { UserFormUi } from '../../../shared/user-form-ui/user-form-ui';

@Component({
  selector: 'app-create-account-ui',
  imports: [TranslatePipe, UserFormUi],
  templateUrl: './create-account-ui.html',
  styleUrl: './create-account-ui.scss',
})
export class CreateAccountUi implements OnInit {
  private readonly userService = inject(UserApiService);
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

    const command: RegisterUserCommand = { ...user, password: this.form.value.password };

    this.userService
      .registerUser(command, {
        successMessage: this.translateService.instant('users.form.registerSuccess'),
        errorMessage: this.translateService.instant('users.form.registerError'),
      })
      .subscribe({
        next: () => {
          this.isSaving.set(false);
        },
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
}
