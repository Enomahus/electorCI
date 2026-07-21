import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateDistrictCommand,
  GetDistrictResponse,
  GetDistrictsQuery,
  GridDataResponseOfGetDistrictsResponse,
} from '../nswag/api-nswag-client';
import { ApiBaseService } from './api-base.service';
import { ApiToastOptions } from './models/api-toast-options';

@Injectable({
  providedIn: 'root',
})
export class DistrictApiService extends ApiBaseService {
  getDistricts(
    query: GetDistrictsQuery,
    options: ApiToastOptions = {},
  ): Observable<GridDataResponseOfGetDistrictsResponse> {
    return this.apiClient.getDistricts(query).pipe(this.handleDataResult(options));
  }

  getDistrict(id: number, options: ApiToastOptions = {}): Observable<GetDistrictResponse> {
    return this.apiClient.getDistrict(id).pipe(this.handleDataResult(options));
  }

  createDistrict(
    command: CreateDistrictCommand,
    options: ApiToastOptions = {},
  ): Observable<number> {
    return this.apiClient.createDistrict(command).pipe(this.handleDataResult(options));
  }
}
