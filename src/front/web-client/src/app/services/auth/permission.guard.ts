import { Injectable } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivate,
  GuardResult,
  MaybeAsync,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { map, switchMap, tap } from 'rxjs';
import { AppPermission } from '../nswag/api-nswag-client';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class PermissionsGuard implements CanActivate {
  constructor(
    private readonly authService: AuthService,
    private readonly router: Router,
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): MaybeAsync<GuardResult> {
    const requiredPermission = route.data['permission'] as AppPermission | undefined;
    const canActivate$ = this.authService.isAuthenticated().pipe(
      tap((isAuthenticated) => {
        if (!isAuthenticated) {
          this.router.navigate(['/home'], { queryParams: { state: route.url.join('/') } });
        }
      }),
      map(() => true),
    );

    if (!requiredPermission) return canActivate$;

    return canActivate$.pipe(
      switchMap(() => this.authService.getPermissions()),
      map((perms) => perms.includes(requiredPermission)),
      tap((hasPerm) => {
        if (!hasPerm) this.router.navigate(['/home']);
      }),
    );
  }
}
