import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { DistrictNode } from '../../../models/district.model';
import { DistrictApiService } from '../../../services/api/district.api.service';
import { DistrictTreeHelperService } from '../../../services/district-tree-helper.service';
import { ToggleActiveDistrictCommand } from '../../../services/nswag/api-nswag-client';
import { DistrictTreeUi } from '../../../shared/district-tree-ui/district-tree-ui';

@Component({
  selector: 'app-districts-ui',
  imports: [TranslatePipe, RouterLink, DistrictTreeUi],
  templateUrl: './districts-ui.html',
  styleUrl: './districts-ui.scss',
})
export class DistrictsUi {
  private readonly districtService = inject(DistrictApiService);
  private readonly router = inject(Router);
  private readonly store = inject(DistrictTreeHelperService);
  private readonly translateService = inject(TranslateService);

  isDeleting = signal(false);
  readonly nodes = this.store.nodesData;
  readonly isLoading = this.store.isLoading;

  refresh(): void {
    this.store.reload();
  }

  onEdit(node: DistrictNode): void {
    this.router.navigate(['admin', 'districts', node.id, 'edit']);
  }

  onDelete(node: DistrictNode): void {
    if (node.id === undefined) return;

    this.isDeleting.set(true);

    this.districtService
      .deleteDistrict(node.id, {
        successMessage: this.translateService.instant('district.successDeleting'),
        errorMessage: this.translateService.instant('district.errorDeleting'),
      })
      .subscribe({
        next: () => {
          this.isDeleting.set(false);
          this.store.reload();
        },
        error: () => {
          this.isDeleting.set(false);
        },
      });
  }

  onToggleStatus(node: DistrictNode): void {
    if (node.id === undefined) return;

    const cmd: ToggleActiveDistrictCommand = {
      id: node.id,
    };
    this.districtService
      .toggleActiveDistrict(cmd, {
        successMessage: this.translateService.instant('district.successUpdating'),
        errorMessage: this.translateService.instant('district.errorUpdating'),
      })
      .subscribe(() => {
        this.store.reload();
      });
  }
}
