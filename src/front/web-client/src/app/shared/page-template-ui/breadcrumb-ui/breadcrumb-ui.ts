import { AsyncPipe } from '@angular/common';
import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { Breadcrumb } from '../../../models/breadcrumb.model';
import { BreadcrumbService } from '../../../services/breadcrumb.service';

@Component({
  selector: 'app-breadcrumb-ui',
  imports: [RouterLink, AsyncPipe],
  templateUrl: './breadcrumb-ui.html',
  styleUrl: './breadcrumb-ui.scss',
})
export class BreadcrumbUi implements OnInit {
  breadcrumbs$?: Observable<Breadcrumb[]>;

  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly destroyRef = inject(DestroyRef);

  ngOnInit(): void {
    this.breadcrumbs$ = this.breadcrumbService.breadcrumbs$.pipe(
      takeUntilDestroyed(this.destroyRef),
    );
  }
}
