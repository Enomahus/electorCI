import { Component, DestroyRef, TemplateRef, computed, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { DistrictApiService } from '../../../services/api/district.api.service';
import { GetDistrictsResponse, GridDataQuery } from '../../../services/nswag/api-nswag-client';
import { DataGridColumn } from '../../../shared/data-grid-ui/data-grid-column';
import { DataGridUi } from '../../../shared/data-grid-ui/data-grid-ui';

@Component({
  selector: 'app-districts-ui',
  imports: [DataGridUi, TranslatePipe],
  templateUrl: './districts-ui.html',
  styleUrl: './districts-ui.scss',
})
export class DistrictsUi {
  private readonly districtService = inject(DistrictApiService);
  private readonly destroyRef = inject(DestroyRef);

  readonly districts = signal<GetDistrictsResponse[]>([]);
  readonly total = signal(0);
  readonly loading = signal(false);



}
