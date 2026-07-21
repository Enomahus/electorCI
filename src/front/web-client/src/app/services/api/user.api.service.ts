import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateUserCommand,
  GetCurrentUserResponse,
  GetUsersQuery,
  GridDataResponseOfGetUsersResponse,
  RegisterUserCommand,
  UpdateCurrentUserCommand,
  UpdateUserCommand,
  UserModel,
} from '../nswag/api-nswag-client';
import { ApiBaseService } from './api-base.service';
import { ApiToastOptions } from './models/api-toast-options';

@Injectable({
  providedIn: 'root',
})
export class UserApiService extends ApiBaseService {
  getCurrentUser(options: ApiToastOptions = {}): Observable<GetCurrentUserResponse> {
    return this.apiClient.getCurrentUser().pipe(this.handleDataResult(options));
  }

  getUsers(
    query: GetUsersQuery,
    options: ApiToastOptions = {},
  ): Observable<GridDataResponseOfGetUsersResponse> {
    return this.apiClient.getUsers(query).pipe(this.handleDataResult(options));
  }

  getUser(id: string, options: ApiToastOptions = {}): Observable<UserModel> {
    return this.apiClient.getUser(id).pipe(this.handleDataResult(options));
  }

  createUser(command: CreateUserCommand, options: ApiToastOptions = {}): Observable<string> {
    return this.apiClient.createUser(command).pipe(this.handleDataResult(options));
  }

  udpateUser(
    userId: string,
    command: UpdateUserCommand,
    options: ApiToastOptions = {},
  ): Observable<string> {
    return this.apiClient.updateUser(userId, command).pipe(this.handleDataResult(options));
  }

  udpateCurrentUser(
    command: UpdateCurrentUserCommand,
    options: ApiToastOptions = {},
  ): Observable<string> {
    return this.apiClient.updateCurrentUser(command).pipe(this.handleDataResult(options));
  }

  registerUser(command: RegisterUserCommand, options: ApiToastOptions = {}): Observable<string> {
    return this.apiClient.registerUser(command).pipe(this.handleDataResult(options));
  }
}
