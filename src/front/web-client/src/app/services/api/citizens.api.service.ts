import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  GetCitizensQuery,
  ResultOfListOfGetCitizensResponse,
  SearchCitizenResponse,
} from '../nswag/api-nswag-client';
import { ApiBaseService } from './api-base.service';
import { ApiToastOptions } from './models/api-toast-options';

@Injectable({
  providedIn: 'root',
})
export class CitizensApiService extends ApiBaseService {
  getCitizens(
    query: GetCitizensQuery,
    options: ApiToastOptions = {},
  ): Observable<ResultOfListOfGetCitizensResponse> {
    return this.apiClient.getCitizens(query).pipe(this.handleResult(options));
  }

  searchCitizenById(
    id: string,
    options: ApiToastOptions = {},
  ): Observable<SearchCitizenResponse[]> {
    return this.apiClient.searchCitizen(null, id).pipe(this.handleDataResult(options));
  }

  searchCitizen(
    search: string,
    options: ApiToastOptions = {},
  ): Observable<SearchCitizenResponse[]> {
    return this.apiClient.searchCitizen(search, null).pipe(this.handleDataResult(options));
  }
}
