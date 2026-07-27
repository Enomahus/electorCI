import { Component, DestroyRef, inject, resource, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';
import { DistrictNode } from '../../../models/district.model';
import { DistrictApiService } from '../../../services/api/district.api.service';
import { DistrictTreeHelperService } from '../../../services/district-tree-helper.service';
import { DistrictTreeUi } from '../../../shared/district-tree-ui/district-tree-ui';

@Component({
  selector: 'app-districts-ui',
  imports: [TranslatePipe, RouterLink, DistrictTreeUi],
  templateUrl: './districts-ui.html',
  styleUrl: './districts-ui.scss',
})
export class DistrictsUi {
  private readonly districtService = inject(DistrictApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly store = inject(DistrictTreeHelperService);
  private readonly translateService = inject(TranslateService);

  isDeleting = signal(false);
  readonly nodes = this.store.nodesData;

  readonly districtsResource = resource({
    loader: async () => {
      const res = await firstValueFrom(this.districtService.getDistricts({}));
      return res ?? [];
    },
  });

  readonly isLoading = this.districtsResource.isLoading;

  refresh(): void {
    this.districtsResource.reload();
  }

  onEdit(node: DistrictNode): void {
    this.router.navigate(['admin', 'districts', node.id, 'edit']);
  }

  onDelete(node: DistrictNode): void {
    if (node.id === undefined) return;

    this.isDeleting.set(true);

    // this.districtService
    //   .deleteDistrict(node.id, {
    //     successMessage: this.translateService.instant('district.successDeleting'),
    //     errorMessage: this.translateService.instant('district.errorDeleting'),
    //   })
    //   .subscribe({
    //     next: () => {
    //       this.isDeleting.set(false);
    //       this.nodes();
    //     },
    //     error: () => {
    //       this.isDeleting.set(false);
    //     },
    //   });
  }

  onToggleStatus(node: DistrictNode): void {
    if (node.id === undefined) return;

    console.log('Toggle status for node:', node);
    // this.constituencyService
    //   .toggleConstituencyStatus(node.id, {
    //     successMessage: this.translateService.instant('constituency.successUpdating'),
    //     errorMessage: this.translateService.instant('constituency.errorUpdating'),
    //   })
    //   .subscribe(() => {
    //     this.nodes();
    //   });
  }
}
