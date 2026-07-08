import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { GetCurrentUserResponse } from '../nswag/api-nswag-client';
import { ApiBaseService } from './api-base.service';
import { ApiToastOptions } from './models/api-toast-options';

@Injectable({
  providedIn: 'root',
})
export class UserApiService extends ApiBaseService {
  getCurrentUser(options: ApiToastOptions = {}): Observable<GetCurrentUserResponse> {
    return this.apiClient.getCurrentUser().pipe(
      this.handleDataResult({
        ...options,
        errorMessage:
          options.errorMessage ?? this.translateService.instant('global.errorGetCurrentUser'),
      }),
    );
  }
}
