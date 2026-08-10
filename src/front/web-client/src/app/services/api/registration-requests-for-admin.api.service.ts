import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  GetRegistrationRequestsForAdminQuery,
  GridDataResponseOfGetRegistrationRequestsForAdminResponse,
  Result,
} from '../nswag/api-nswag-client';
import { ApiBaseService } from './api-base.service';
import { ApiToastOptions } from './models/api-toast-options';

@Injectable({
  providedIn: 'root',
})
export class RegistrationRequestsForAdminApiService extends ApiBaseService {
  getRegistrationRequests(
    query: GetRegistrationRequestsForAdminQuery,
    options: ApiToastOptions = {},
  ): Observable<GridDataResponseOfGetRegistrationRequestsForAdminResponse> {
    return this.apiClient
      .getRegistrationRequestsForAdmin(query)
      .pipe(this.handleDataResult(options));
  }

  deleteRegistrationRequest(id: string, options: ApiToastOptions = {}): Observable<Result> {
    return this.apiClient.deleteRegistrationeRequestForAdmin(id).pipe(this.handleResult(options));
  }
}
