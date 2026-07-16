import {
  Component,
  computed,
  DestroyRef,
  HostListener,
  inject,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { filter, tap } from 'rxjs';
import { Language } from '../../../enums/language.enum';
import { AuthService } from '../../../services/auth/auth.service';
import { CurrentUserService } from '../../../services/current-user.service';
import { LanguageService } from '../../../services/language.service';
import { AppPermission } from '../../../services/nswag/api-nswag-client';

@Component({
  standalone: true,
  template: '',
})
export abstract class BaseNavbar {
  logout = output<void>();

  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly languageService = inject(LanguageService);
  private readonly authService = inject(AuthService);
  private readonly currentUserService = inject(CurrentUserService);

  /** Name of the currently open dropdown (e.g. 'admin', 'user'), or null when all are closed. */
  readonly openDropdown = signal<string | null>(null);

  readonly userName = toSignal(this.currentUserService.currentUserName$, { initialValue: '' });

  /** True when a user is authenticated (the current user name is set). */
  readonly isLoggedIn = computed(() => this.userName().trim().length > 0);

  private readonly permissions = toSignal(this.authService.getPermissions(), {
    initialValue: [] as AppPermission[],
  });
  readonly showAdminRequestsText = computed(() =>
    this.permissions().includes('accessRegistrationRequestsForAdminPage'),
  );
  readonly showOrganismRequestsText = computed(() =>
    this.permissions().includes('accessRegistrationRequestsForManagementPage'),
  );
  readonly showElectorRequestsText = computed(() =>
    this.permissions().includes('accessRegistrationRequestsPage'),
  );
  readonly showRequests = computed(
    () =>
      this.showElectorRequestsText() ||
      this.showOrganismRequestsText() ||
      this.showAdminRequestsText(),
  );
  readonly isAdmin = computed(() => this.permissions().includes('superAdmin'));

  constructor() {
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        tap(() => {
          this.openDropdown.set(null);
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe();
  }

  isRouteMatching(route: string): boolean {
    return this.router.url.includes(route);
  }

  toggleDropdown(name: string): void {
    this.openDropdown.set(this.openDropdown() === name ? null : name);
  }

  isDropdownOpen(name: string): boolean {
    return this.openDropdown() === name;
  }

  @HostListener('document:click', ['$event'])
  onClick(event: Event): void {
    // Hide dropdown after having clicked outside.
    const targetElement = event.target as HTMLElement;
    if (
      (!targetElement.closest('.dropdown-content') && !targetElement.closest('.dropbtn')) ||
      targetElement.offsetParent?.className === 'dropdown-content'
    ) {
      this.openDropdown.set(null);
    }
  }

  langChange(lang: Language): void {
    this.languageService.changeLanguage(lang);
  }
}
