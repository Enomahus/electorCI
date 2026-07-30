import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  GetPollingStationsQuery,
  GridDataResponseOfGetPollingStationsResponse,
  Result,
  ResultOfGetPollingStationResponse,
  ToggleActivatePollingStationCommand,
  UpdatePollingStationCommand,
} from '../nswag/api-nswag-client';
import { ApiBaseService } from './api-base.service';
import { ApiToastOptions } from './models/api-toast-options';

@Injectable({
  providedIn: 'root',
})
export class PollingStationApiService extends ApiBaseService {
  getPollingStation(
    id: number,
    options: ApiToastOptions = {},
  ): Observable<ResultOfGetPollingStationResponse> {
    return this.apiClient.getPollingStation(id).pipe(this.handleResult(options));
  }

  getPollingStations(
    query: GetPollingStationsQuery,
    options: ApiToastOptions = {},
  ): Observable<GridDataResponseOfGetPollingStationsResponse> {
    return this.apiClient.getPollingStations(query).pipe(this.handleDataResult(options));
  }

  updatePollingStation(
    id: number,
    cmd: UpdatePollingStationCommand,
    options: ApiToastOptions = {},
  ): Observable<number> {
    return this.apiClient.updatePollingStation(id, cmd).pipe(this.handleDataResult(options));
  }

  toggleActivePollingStation(
    cmd: ToggleActivatePollingStationCommand,
    options: ApiToastOptions = {},
  ): Observable<Result> {
    return this.apiClient.toggleActivatePollingStation(cmd).pipe(this.handleResult(options));
  }

  deletePollingStation(id: number, options: ApiToastOptions = {}): Observable<Result> {
    return this.apiClient.deletePollingStation(id).pipe(this.handleResult(options));
  }
}
