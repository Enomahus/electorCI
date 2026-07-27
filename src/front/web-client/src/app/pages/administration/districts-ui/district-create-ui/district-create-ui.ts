import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Breadcrumb } from '../../../../models/breadcrumb.model';
import { DistrictApiService } from '../../../../services/api/district.api.service';
import { BreadcrumbService } from '../../../../services/breadcrumb.service';
import { DistrictModel } from '../../../../services/nswag/api-nswag-client';
import { createDistrictForm, DistrictForm } from '../district-ui/district-form';
import { DistrictUi } from '../district-ui/district-ui';

@Component({
  selector: 'app-district-create-ui',
  imports: [DistrictUi, TranslatePipe],
  templateUrl: './district-create-ui.html',
  styleUrl: './district-create-ui.scss',
})
export class DistrictCreateUi implements OnInit {
  private readonly districtService = inject(DistrictApiService);
  private readonly translateService = inject(TranslateService);
  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly router = inject(Router);

  form = signal<DistrictForm>(createDistrictForm());

  isSaving = signal(false);

  ngOnInit(): void {
    this.setBreadcrumb();
  }

  private setBreadcrumb(): void {
    let breadcrumbs: Breadcrumb[] = [];
    breadcrumbs = [
      {
        label: this.translateService.instant('breadcrumb.districts'),
        url: `/admin/districts`,
      },
      {
        label: this.translateService.instant('breadcrumb.createDistrict'),
      },
    ];
    this.breadcrumbService.setBreadcrumbs(breadcrumbs);
  }

  onSubmit(model: DistrictModel): void {
    this.isSaving.set(true);

    this.districtService
      .createDistrict(model, {
        successMessage: this.translateService.instant('district.successCreating'),
        errorMessage: this.translateService.instant('district.errorCreating'),
      })
      .subscribe({
        next: () => {
          this.isSaving.set(true);
          this.goBack();
        },
        error: () => this.isSaving.set(false),
      });
  }

  goBack(): void {
    this.router.navigate(['/admin/districts']);
  }
}
