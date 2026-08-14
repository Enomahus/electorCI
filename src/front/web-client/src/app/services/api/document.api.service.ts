import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  FileResponse,
  GetDocumentInfoResponse,
  GetDocumentsInfosQuery,
  GetDocumentsInfosResponse,
} from '../nswag/api-nswag-client';
import { ApiBaseService } from './api-base.service';
import { ApiToastOptions } from './models/api-toast-options';

@Injectable({
  providedIn: 'root',
})
export class DocumentApiService extends ApiBaseService {
  getDocumentInfo(
    documentId: string,
    options: ApiToastOptions = {},
  ): Observable<GetDocumentInfoResponse> {
    return this.apiClient.getDocumentInfo(documentId).pipe(this.handleDataResult(options));
  }

  getDocumentsInfos(
    documentIds: string[],
    options: ApiToastOptions = {},
  ): Observable<GetDocumentsInfosResponse> {
    const query = { documentIds: documentIds } as GetDocumentsInfosQuery;
    return this.apiClient.getDocumentsInfos(query).pipe(this.handleDataResult(options));
  }

  download(documentId: string, options: ApiToastOptions = {}): Observable<FileResponse> {
    return this.apiClient.downloadDocument(documentId).pipe(this.handleResult(options));
  }
}
