import { inject, Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { catchError, filter, map, Observable, pipe, tap, throwError, UnaryFunction } from 'rxjs';
import { ToastService } from '../toast.service';
import { ApiToastOptions } from './models/api-toast-options';
import { DataResult } from './models/data-result';

@Injectable({
  providedIn: 'root',
})
export class ApiBaseService {
  protected toastService = inject(ToastService);
  protected translateService = inject(TranslateService);

  handleDataResult<T, R extends DataResult<T>>(
    options: ApiToastOptions = {},
  ): UnaryFunction<Observable<R>, Observable<NonNullable<T>>> {
    return pipe(
      filter((r: R) => !!r.data),
      map((r: R) => r.data!),
      this.toastSuccess(options.successMessage),
      this.catchErrorResult(options),
    );
  }

  handleResult<R>(
    options: ApiToastOptions = {},
  ): UnaryFunction<Observable<NonNullable<R>>, Observable<NonNullable<R>>> {
    return pipe(this.toastSuccess(options.successMessage), this.catchErrorResult(options));
  }

  private toastSuccess<T>(
    successMessage: string | undefined = undefined,
  ): UnaryFunction<Observable<NonNullable<T>>, Observable<NonNullable<T>>> {
    return pipe(
      tap(() => {
        if (successMessage) this.toastService.toastSuccess(successMessage);
      }),
    );
  }

  private catchErrorResult<T>(
    options: ApiToastOptions = {},
  ): UnaryFunction<Observable<NonNullable<T>>, Observable<NonNullable<T>>> {
    return pipe(
      catchError((err) => {
        console.error(err);
        if (!options.preventErrorToasting) {
          let title = options.errorMessage ?? this.translateService.instant('errors.serverError');
          let builtMessage = '';
          if (Object.hasOwn(err, 'data')) {
            const errorData = err.data as ErrorDto;
            builtMessage = this.buildErrorMessageHtml(errorData);
          }
          if (builtMessage.length === 0) {
            builtMessage = title;
            title = undefined;
          }
          this.toastService.toastError(builtMessage, title, { enableHtml: true });
        }
        return throwError(() => err);
      }),
    );
  }

  private buildErrorMessageHtml(errorData: ErrorDto): string {
    let builtMessage = '';
    if (errorData.code) {
      const errorTranslationKey = `errors.backendCodes.${errorData.code}`;
      builtMessage = this.hasTranslation(errorTranslationKey)
        ? this.translateService.instant(errorTranslationKey)
        : '';
      if (errorData.code == 'validation' && errorData.additionalData) {
        for (const field in errorData.additionalData) {
          const validationCode = errorData.additionalData[field];
          console.error(`Validation error : ${field}, ${validationCode}`);
          const translationKey = `errors.backendCodes.validationCodes.${validationCode}`;
          if (this.hasTranslation(translationKey)) {
            const validationMsg = this.translateService.instant(translationKey, {
              field: this.getFieldLabel(field),
            });
            builtMessage += `<br/>- ${validationMsg}`;
          }
        }
      }
    }
    return builtMessage;
  }

  private getFieldLabel(field: string): string {
    return this.hasTranslation(field) ? this.translateService.instant(field) : field;
  }

  private hasTranslation(key: string): boolean {
    if (!key) return false;

    const translation = this.translateService.instant(key);
    return translation !== key && translation !== '';
  }
}

export interface ErrorDto {
  code?: ErrorCode;
  description?: string;
  kind?: ErrorKind;
  additionalData?: { [key: string]: string };
  values?: { [key: string]: string };
}

export type ErrorCode =
  | 'none'
  | 'validation'
  | 'invalidParameter'
  | 'invalidStatus'
  | 'resourceAlreadyExists'
  | 'notFound'
  | 'missingData'
  | 'genericServerError'
  | 'accessRights'
  | 'authenticationFailed'
  | 'configurationMissing'
  | 'dataSeeding'
  | 'storage'
  | 'export'
  | 'incorrectEntityTypeLinked';

export type ErrorKind =
  | 'none'
  | 'authentication'
  | 'accessRights'
  | 'validation'
  | 'technical'
  | 'requestData'
  | 'domainRule';
