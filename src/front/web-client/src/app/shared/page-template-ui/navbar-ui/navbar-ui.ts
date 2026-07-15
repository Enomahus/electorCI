import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { filter, Observable, tap } from 'rxjs';
import { AuthService } from '../../../services/auth/auth.service';
import { CurrentUserService } from '../../../services/current-user.service';
import { NavbarDesktopUi } from './navbar-desktop-ui/navbar-desktop-ui';
import { NavbarMobileUi } from './navbar-mobile-ui/navbar-mobile-ui';

@Component({
  selector: 'app-navbar-ui',
  imports: [RouterLink, TranslatePipe, NavbarDesktopUi, NavbarMobileUi, RouterLink],
  templateUrl: './navbar-ui.html',
  styleUrl: './navbar-ui.scss',
})
export class NavbarUi {
  private readonly authService = inject(AuthService);
  private readonly currentUserService = inject(CurrentUserService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);

  isToggleMobileMenu = signal(false);
  userName$: Observable<string>;

  constructor() {
    this.userName$ = this.currentUserService.currentUserName$.pipe(
      takeUntilDestroyed(this.destroyRef),
    );

    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        tap(() => {
          this.isToggleMobileMenu.set(false);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  toggleMobileMenu() {
    this.isToggleMobileMenu.set(!this.isToggleMobileMenu());
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
