import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { jwtDecode } from 'jwt-decode';
import {
  BehaviorSubject,
  catchError,
  filter,
  first,
  firstValueFrom,
  from,
  map,
  Observable,
  of,
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

@Injectable({
  providedIn: 'root',
})
export class AuthService extends ApiBaseService {
  private readonly accessToken$ = new BehaviorSubject<string | undefined>(undefined);
  private readonly refreshing$ = new BehaviorSubject<boolean>(false);
  private readonly permissions$ = new BehaviorSubject<AppPermission[] | undefined>(undefined);

  private readonly router = inject(Router);
  private readonly currentUserService = inject(CurrentUserService);
  private readonly userApiService = inject(UserApiService);

  constructor() {
    super();
    this.getAccessToken().subscribe();
  }

  login(userName: string, password: string): Observable<ResultOfTokenResponse> {
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

  isSuperAdmin(): Observable<boolean> {
    return this.currentUserService.currentUserName$.pipe(
      switchMap(() =>
        this.getPermissions().pipe(
          first(),
          map((p) => p.includes('superAdmin')),
        ),
      ),
    );
  }

  getCurrentUser(): string | null {
    return localStorage.getItem('currentUserKey');
  }

  getPermissions(): Observable<AppPermission[]> {
    return this.permissions$.pipe(filter((p) => !!p)) as Observable<AppPermission[]>;
  }

  logout(): void {
    this.accessToken$.next(undefined);
    this.permissions$.next(undefined);
    localStorage.removeItem(refreshTokenKey);
    localStorage.removeItem(currentUserKey);
    this.router.navigateByUrl('/login');
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
      switchMap((result) => from(this.storeTokens(result)).pipe(map(() => result))),
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
        lastName: string | undefined;
        firstName: string | undefined;
        role: string | string[] | undefined;
      }>(result?.data?.accessToken);

      const name = payload.name;

      if (!name) {
        this.logout();
        return;
      }
      await this.fetchPermissions();
      this.currentUserService.changeCurrentUserName(`${payload.firstName} ${payload.lastName}`);
      localStorage.setItem(currentUserKey, name);
    } else {
      await this.fetchPermissions();
    }
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
