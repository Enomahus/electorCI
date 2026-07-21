import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { map, take, tap } from 'rxjs';
import { AppPermission } from '../nswag/api-nswag-client';
import { AuthService } from './auth.service';

@Directive({
  selector: '[appHasPermission]',
})
export class PermissionDirective {
  @Input() set appHasPermission(permission: AppPermission | AppPermission[] | undefined) {
    if (!permission) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      return;
    }
    let permissionArray = permission;
    if (typeof permissionArray == 'string') {
      permissionArray = [permissionArray];
    }

    this.authService
      .getPermissions()
      .pipe(
        take(1),
        map((perms) =>
          perms.some((p1) => permissionArray.some((p2) => p1.localeCompare(p2) === 0)),
        ),
        tap((hasPerm) => {
          if (hasPerm) {
            this.viewContainer.createEmbeddedView(this.templateRef);
          } else {
            this.viewContainer.clear();
          }
        }),
      )
      .subscribe();
  }

  constructor(
    private readonly templateRef: TemplateRef<unknown>,
    private readonly viewContainer: ViewContainerRef,
    private readonly authService: AuthService,
  ) {}
}
