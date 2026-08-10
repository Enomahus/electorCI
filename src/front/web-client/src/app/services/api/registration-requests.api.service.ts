import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  FileParameter,
  GetRegistrationRequestsQuery,
  GridDataResponseOfGetRegistrationRequestsResponse,
  RegistrationRequestModel,
  Result,
  ResultOfGetRegistrationRequestResponse,
  ResultOfGuid,
  TriggerActionOnRegistrationRequestCommand,
} from '../nswag/api-nswag-client';
import { ApiBaseService } from './api-base.service';
import { ApiToastOptions } from './models/api-toast-options';

@Injectable({
  providedIn: 'root',
})
export class RegistrationRequestsApiService extends ApiBaseService {
  createRegistrationRequest(
    registrationRequestJson: RegistrationRequestModel,
    IdentityDocument: FileParameter[],
    residenceCertificate: FileParameter[],
    photo: FileParameter[],
    options: ApiToastOptions = {},
  ): Observable<ResultOfGuid> {
    return this.apiClient
      .createRegistrationRequest(
        registrationRequestJson,
        IdentityDocument,
        residenceCertificate,
        photo,
      )
      .pipe(this.handleResult(options));
  }

  getRegistrationRequest(
    id: string,
    options: ApiToastOptions = {},
  ): Observable<ResultOfGetRegistrationRequestResponse> {
    return this.apiClient.getRegistrationRequest(id).pipe(this.handleResult(options));
  }

  getRegistrationRequests(
    query: GetRegistrationRequestsQuery,
    options: ApiToastOptions = {},
  ): Observable<GridDataResponseOfGetRegistrationRequestsResponse> {
    return this.apiClient.getRegistrationRequests(query).pipe(this.handleDataResult(options));
  }

  deleteRegistrationRequest(id: string, options: ApiToastOptions = {}): Observable<Result> {
    return this.apiClient.deleteRegistrationeRequest(id).pipe(this.handleResult(options));
  }

  tirggerActionOnRegistrationRequest(
    id: string,
    command: TriggerActionOnRegistrationRequestCommand,
    options: ApiToastOptions = {},
  ): Observable<ResultOfGuid> {
    return this.apiClient
      .triggerActionOnRegistrationRequest(id, command)
      .pipe(this.handleResult(options));
  }
}
