import { DestroyRef, inject, Injectable } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject, distinctUntilChanged, filter, map } from 'rxjs';
import { Breadcrumb } from '../models/breadcrumb.model';

@Injectable({
  providedIn: 'root',
})
export class BreadcrumbService {
  breadcrumbs$ = new BehaviorSubject<Breadcrumb[]>([]);

  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  constructor() {
    this.router.events
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        map((event: NavigationEnd) => this.extractPathWithoutQueryParams(event.urlAfterRedirects)),
        distinctUntilChanged(),
        map(() => {
          this.breadcrumbs$.next([]);
        }),
      )
      .subscribe();
  }

  extractPathWithoutQueryParams(url: string): string {
    return url.split('?')[0];
  }

  setBreadcrumbs(breadcrumbs: Breadcrumb[]): void {
    this.breadcrumbs$.next(breadcrumbs);
  }
}
