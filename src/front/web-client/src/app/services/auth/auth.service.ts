import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import {
  BehaviorSubject,
  catchError,
  filter,
  firstValueFrom,
  map,
  Observable,
  of,
  ReplaySubject,
  switchMap,
  take,
  tap,
} from 'rxjs';
import { ApiBaseService } from '../api/api-base.service';
import { UserApiService } from '../api/user.api.service';
import { CurrentUserService } from '../current-user.service';
import { AppPermission, ResultOfTokenResponse } from '../nswag/api-nswag-client';

const refreshTokenKey = 'refreshTokenKey';
const currentUserKey = 'currentUserKey';
const currentEmailKey = 'currentEmailKey';
const currentUserIdKey = 'currentUserIdKey';

@Injectable({
  providedIn: 'root',
})
export class AuthService extends ApiBaseService {
  private readonly accessToken$ = new BehaviorSubject<string | undefined>(undefined);
  private readonly refreshing$ = new BehaviorSubject<boolean>(false);
  private permissions$ = new ReplaySubject<AppPermission[]>(1);

  private readonly router = inject(Router);
  private readonly currentUserService = inject(CurrentUserService);
  private readonly userApiService = inject(UserApiService);

  constructor() {
    super();
    this.getAccessToken().subscribe();
  }

  login(userName: string, password: string): Observable<ResultOfTokenResponse> {
    this.refreshing$.next(true);
    return this.apiClient
      .authenticate({
        userName,
        password,
      })
      .pipe(
        tap((result) => {
          this.storeTokens(result);
        }),
      );
  }

  getAccessToken(): Observable<string | undefined> {
    return this.accessToken$.pipe(
      switchMap((token) => {
        if (!token) {
          return this.refreshToken();
        }

        const data = jwtDecode(token);
        const expirationData = new Date((data.exp ?? 0) * 1000);
        const dateDiff = expirationData.getTime() - new Date().getTime();
        const isTokenValid = dateDiff > 5000;

        // If less than 5 seconds before expiry
        if (!isTokenValid) {
          return this.refreshToken();
        }

        return of(token);
      }),
      take(1),
    );
  }

  isAuthenticated(): Observable<boolean> {
    return this.getAccessToken().pipe(map((token) => !!token));
  }

  isAdmin(): Observable<boolean> {
    return this.permissions$.pipe(
      take(1),
      map((perms) => perms?.includes('superAdmin')),
    );
  }

  getCurrentUser(): string | null {
    return localStorage.getItem(currentUserKey);
  }

  getPermissions(): Observable<AppPermission[]> {
    return this.permissions$;
  }

  getCurrentUserEmail(): string | null {
    return localStorage.getItem(currentEmailKey);
  }
  getCurrentUserId(): string | null {
    return localStorage.getItem(currentUserIdKey);
  }

  logout(): void {
    this.accessToken$.next(undefined);
    this.refreshing$.next(false);
    this.permissions$ = new ReplaySubject<AppPermission[]>(1);
    this.currentUserService.changeCurrentUserName('');
    localStorage.removeItem(refreshTokenKey);
    localStorage.removeItem(currentUserKey);
  }

  private refreshToken(): Observable<string | undefined> {
    if (this.refreshing$.value) {
      return this.refreshing$.pipe(
        filter((r) => !r), // Wait until other refresh has happened
        switchMap(() => this.accessToken$),
      );
    }

    this.refreshing$.next(true);
    const refreshToken = this.getRefreshToken();
    const userName = this.getCurrentUser();
    if (!refreshToken || !userName) {
      this.logout();
      return of(undefined);
    }

    return this.apiClient.refreshToken({ refreshToken, userName }).pipe(
      catchError((err) => {
        console.error(err);
        this.logout();
        return of(undefined);
      }),
      tap((result) => {
        this.storeTokens(result);
      }),
      map((result) => result?.data?.accessToken ?? undefined),
      take(1),
    );
  }

  private getRefreshToken(): string | null {
    return localStorage.getItem(refreshTokenKey);
  }

  private async storeTokens(result: ResultOfTokenResponse | undefined): Promise<void> {
    this.accessToken$.next(result?.data?.accessToken);
    this.setRefreshToken(result?.data?.refreshToken);
    if (result?.data?.accessToken) {
      const payload = jwtDecode<{
        name: string | undefined;
        email: string | undefined;
        lastName: string | undefined;
        firstName: string | undefined;
        sub: string | undefined;
      }>(result?.data?.accessToken);

      const name = payload.name;
      const email = payload.email;
      const id = payload.sub;

      if (!name || !email || !id) {
        this.logout();
        return;
      }
      this.currentUserService.changeCurrentUserName(`${payload.firstName} ${payload.lastName}`);
      localStorage.setItem(currentUserKey, name);
      localStorage.setItem(currentEmailKey, email);
      localStorage.setItem(currentUserIdKey, id);
    }
    await this.fetchPermissions();

    this.refreshing$.next(false);
  }

  private async fetchPermissions(): Promise<void> {
    const currentUser = await firstValueFrom(this.userApiService.getCurrentUser());
    const permissions = currentUser.permissions ?? [];
    this.permissions$.next(permissions);
  }

  private setRefreshToken(token: string | undefined): void {
    if (token) {
      localStorage.setItem(refreshTokenKey, token);
    } else {
      localStorage.removeItem(refreshTokenKey);
    }
  }
}
