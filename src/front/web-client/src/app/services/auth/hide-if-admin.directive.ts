import { Directive, inject, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs';
import { AuthService } from './auth.service';

@Directive({
  selector: '[appHideIfAdmin]',
})
export class HideIfAdminDirective {
  private readonly templateRef = inject(TemplateRef<unknown>);
  private readonly viewContainer = inject(ViewContainerRef);
  private readonly authService = inject(AuthService);

  constructor() {
    this.authService
      .isAdmin()
      .pipe(take(1))
      .subscribe((isAdmin) => {
        if (!isAdmin) {
          this.viewContainer.createEmbeddedView(this.templateRef);
        } else {
          this.viewContainer.clear();
        }
      });
  }
}
