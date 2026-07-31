import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { map, switchMap, tap } from 'rxjs';
import { PollingStationApiService } from '../../../../services/api/pollin-station.api.service';
import {
  GetPollingStationResponse,
  PollingStationModel,
  UpdatePollingStationCommand,
} from '../../../../services/nswag/api-nswag-client';
import { LoaderUi } from '../../../../shared/loader/loader';
import {
  createPollingStationForm,
  PollingStationForm,
} from '../polling-station-ui/polling-station-form';
import { PollingStationUi } from '../polling-station-ui/polling-station-ui';

@Component({
  selector: 'app-polling-station-update-ui',
  imports: [PollingStationUi, LoaderUi],
  templateUrl: './polling-station-update-ui.html',
  styleUrl: './polling-station-update-ui.scss',
})
export class PollingStationUpdateUi implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly translateService = inject(TranslateService);
  private readonly pollingStationService = inject(PollingStationApiService);

  isLoading = signal(false);
  isSaving = signal(false);
  pollingStationId = signal<number | undefined>(undefined);
  pollingStation = signal<GetPollingStationResponse | null>(null);
  form = signal<PollingStationForm>(createPollingStationForm(true));

  ngOnInit(): void {
    this.route.params.pipe(
      takeUntilDestroyed(this.destroyRef),
      map((params) => params['id']),
      tap((id) => this.pollingStationId.set(id)),
      switchMap((id) => this.pollingStationService.getPollingStation(id)),
      tap((station) => {
        this.pollingStation.set(station.data!);
        this.form().patchValue({
          stationNumber: station.data?.stationNumber,
          wording: station.data?.wording,
          districtId: station.data?.districtId,
          isActive: station.data?.isActive,
        });
      }),
    ).subscribe();
  }

  onUpdatePollingStation(model: PollingStationModel): void {
    const id = this.pollingStation()?.id;
    if (!id) return;

    this.isSaving.set(true);

    const successMsg = this.translateService.instant('pollingStations.updatePollingStationSuccess');
    const errorMsg = this.translateService.instant('pollingStations.updatePollingStationError');

    const cmd: UpdatePollingStationCommand = { id: id, ...model };

    this.pollingStationService
      .updatePollingStation(id, cmd, {
        successMessage: successMsg,
        errorMessage: errorMsg,
      })
      .subscribe({
        next: () => {
          this.isSaving.set(false);
          this.goBack();
        },
        error: () => this.isSaving.set(false),
      });
  }

  goBack(): void {
    this.router.navigate(['/admin/polling-stations']);
  }
}
