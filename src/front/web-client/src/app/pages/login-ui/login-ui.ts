import { JsonPipe } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { form, FormField, maxLength, minLength, required } from '@angular/forms/signals';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { combineLatest, Observable, take } from 'rxjs';
import { AuthService } from '../../services/auth/auth.service';
import { AuthProvider, ResultOfTokenResponse } from '../../services/nswag/api-nswag-client';
import { LoaderUi } from '../../shared/loader/loader';
import { LoginPageTemplateUi } from './login-page-template-ui/login-page-template-ui';

@Component({
  selector: 'app-login-ui',
  imports: [TranslatePipe, LoginPageTemplateUi, RouterLink, LoaderUi, FormField, JsonPipe],
  templateUrl: './login-ui.html',
  styleUrl: './login-ui.scss',
})
export class LoginUi implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly translateService = inject(TranslateService);

  showPassword = signal(false);
  isLoggingIn = signal(false);
  isLoggingInExternal = signal(false);
  loginFailed = signal(false);
  externalLogingFailed = signal(false);

  returnUrl = signal('' as string | undefined);

  loginModel = signal({
    userName: '',
    password: '',
  });

  loginForm = form(this.loginModel, (schemaPath) => {
    required(schemaPath.userName, { message: this.translateService.instant('formError.required') });
    required(schemaPath.password, { message: this.translateService.instant('formError.required') });
    minLength(schemaPath.password, 8, {
      message: this.translateService.instant('formError.passwordFormat'),
    });
    maxLength(schemaPath.password, 24, {
      message: this.translateService.instant('formError.passwordFormat'),
    });
  });

  ngOnInit(): void {
    this.authService.logout();
    combineLatest([this.route.paramMap, this.route.queryParamMap])
      .pipe(take(1))
      .subscribe(([params, queryParams]) => {
        const provider = params.get('provider') as AuthProvider | null;
        const code = queryParams.get('code');
        if (provider && code) {
          this.isLoggingInExternal.set(true);
          switch (provider) {
            case 'google':
              //this.handleLogin(this.authService.loginGoogle(code), true);
              break;
            case 'microsoft':
              //this.handleLogin(this.authService.loginMicrosoft(code), true);
              break;
          }
        }
        this.returnUrl.set(queryParams.get('state') ?? undefined);
      });
  }

  loginEmail(event: Event): void {
    event.preventDefault();
    if (this.loginForm().invalid()) {
      this.loginForm().markAsTouched();
      return;
    }
    this.handleLogin(
      this.authService.login(this.loginModel().userName, this.loginModel().password),
    );
  }

  private handleLogin(tokenQuery: Observable<ResultOfTokenResponse>, isExternal = false): void {
    this.isLoggingIn.set(!isExternal);
    this.isLoggingInExternal.set(isExternal);
    tokenQuery.subscribe({
      next: () => {
        this.isLoggingIn.set(false);
        this.isLoggingInExternal.set(false);
        this.router.navigateByUrl(this.returnUrl() ?? '/home');
      },
      error: () => {
        this.isLoggingIn.set(false);
        this.isLoggingInExternal.set(false);
        this.loginFailed.set(!isExternal);
        this.externalLogingFailed.set(isExternal);
      },
    });
  }

  googleAuth(): void {
    this.authService.requestGoogleAuthCode(this.returnUrl());
  }
  microsoftAuth(): void {
    this.authService.requestMicrosoftAuthCodeAsync(this.returnUrl());
  }
}
