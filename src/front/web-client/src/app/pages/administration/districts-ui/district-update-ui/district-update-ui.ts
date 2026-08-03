import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { filter, map, Observable, switchMap, tap } from 'rxjs';
import { Breadcrumb } from '../../../../models/breadcrumb.model';
import { DistrictApiService } from '../../../../services/api/district.api.service';
import { BreadcrumbService } from '../../../../services/breadcrumb.service';
import { DistrictTreeHelperService } from '../../../../services/district-tree-helper.service';
import {
  DistrictModel,
  GetDistrictResponse,
  UpdateDistrictCommand,
} from '../../../../services/nswag/api-nswag-client';
import { createDistrictForm, DistrictForm } from '../district-ui/district-form';
import { DistrictUi } from '../district-ui/district-ui';

@Component({
  selector: 'app-district-update-ui',
  imports: [TranslatePipe, DistrictUi],
  templateUrl: './district-update-ui.html',
  styleUrl: './district-update-ui.scss',
})
export class DistrictUpdateUi implements OnInit {
  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly translateService = inject(TranslateService);
  private readonly districtService = inject(DistrictApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly districtTreeHelper = inject(DistrictTreeHelperService);

  form = signal<DistrictForm>(createDistrictForm());
  district = signal<GetDistrictResponse | null>(null);
  districtId$: Observable<number>;
  isLoading = signal(false);
  isSaving = signal(false);

  constructor() {
    this.districtId$ = this.route.params.pipe(map((params) => parseInt(params['id'])));
  }

  ngOnInit(): void {
    this.districtId$
      .pipe(
        takeUntilDestroyed(this.destroyRef), // On se désabonne quand on quitte le composant
        tap((districtId) => {
          if (!districtId) {
            console.error('Missing id from route');
            this.router.navigate(['/']);
          }
        }), // On redirige quand l'id est null
        filter((districtId) => !!districtId), // On ignore les émissions où l'id est null
        //tap(() => (this.isLoading.set(true)),
        switchMap((districtId) =>
          this.districtService.getDistrict(districtId, {
            errorMessage: this.translateService.instant('stakeholder.form.errorGet'),
          }),
        ), // Chaque émission déclenche la souscription à cet observable
      )
      .subscribe({
        next: (result) => {
          this.district.set(result);
          this.setBreadcrumb(result);
          //this.districtId = result.id;
          //this.isSoren = result.isSoren;
          this.isLoading.set(false);
          //this.isUpToDate = true;
        },
        error: () => {
          this.isLoading.set(false);
          this.router.navigate(['/']);
        },
      });

  }

  private setBreadcrumb(district: GetDistrictResponse): void {
    let breadcrumbs: Breadcrumb[] = [];
    breadcrumbs = [
      {
        label: this.translateService.instant('breadcrumb.districts'),
        url: `/admin/districts`,
      },
      {
        //label: this.translateService.instant('breadcrumb.updateDistrict'),
        label: district.wording!,
      },
    ];
    this.breadcrumbService.setBreadcrumbs(breadcrumbs);
  }

  onSubmit(model: DistrictModel): void {
    const id = this.district()?.id;
    if (!id) return;

    this.isSaving.set(true);

    const command: UpdateDistrictCommand = { ...model, id };
    this.districtService
      .updateDistrict(id, command, {
        successMessage: this.translateService.instant('district.successUpdating'),
        errorMessage: this.translateService.instant('district.errorUpdating'),
      })
      .subscribe({
        next: () => {
          this.isSaving.set(false);
          this.districtTreeHelper.reload();
          this.goBack();
        },
        error: () => this.isSaving.set(false),
      });
  }

  goBack(): void {
    this.router.navigate(['admin', 'districts']);
  }
}
