import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  GetRegistrationRequestsForManagementQuery,
  GridDataResponseOfGetRegistrationRequestsForManagementResponse,
  Result,
} from '../nswag/api-nswag-client';
import { ApiBaseService } from './api-base.service';
import { ApiToastOptions } from './models/api-toast-options';

@Injectable({
  providedIn: 'root',
})
export class RegistrationRequestsForManagementApiService extends ApiBaseService {
  getRegistrationRequests(
    query: GetRegistrationRequestsForManagementQuery,
    options: ApiToastOptions = {},
  ): Observable<GridDataResponseOfGetRegistrationRequestsForManagementResponse> {
    return this.apiClient
      .getRegistrationRequestsForManagement(query)
      .pipe(this.handleDataResult(options));
  }

  deleteRegistrationRequest(id: string, options: ApiToastOptions = {}): Observable<Result> {
    return this.apiClient
      .deleteRegistrationeRequestForManagement(id)
      .pipe(this.handleResult(options));
  }
}
