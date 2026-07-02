import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AppPermission } from '../auth/auth.service';
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

export interface UserModel {
  id?: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string | undefined;
  phoneNumber2?: string | undefined;
  email?: string | undefined;
  stakeholderId?: number;
  title?: PersonTitle;
  isAdmin?: boolean | undefined;
  isActive?: boolean;
  principalActivityId?: number | undefined;
  userActivities?: ActivityCode[];
  additionalRoles?: string[];
  stakeHolderName?: string | undefined;
  isUserSoren?: boolean;
}

export interface GetCurrentUserResponse extends UserModel {
  isSuperAdmin?: boolean;
  permissions?: AppPermission[];
}

export type PersonTitle = 'mr' | 'mrs';

export type ActivityCode =
  | 'holder'
  | 'voluntaryDropOff'
  | 'logistician'
  | 'gatheringCenter'
  | 'reuseCenter'
  | 'treatmentCenter';
