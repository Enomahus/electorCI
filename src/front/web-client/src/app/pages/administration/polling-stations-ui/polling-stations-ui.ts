import { NgClass } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import {
  BehaviorSubject,
  catchError,
  debounceTime,
  distinctUntilChanged,
  finalize,
  map,
  Observable,
  of,
  takeUntil,
} from 'rxjs';
import { PollingStationApiService } from '../../../services/api/pollin-station.api.service';
import { PermissionDirective } from '../../../services/auth/permission.directive';
import {
  GetPollingStationsResponse,
  GridDataQuery,
  ToggleActivatePollingStationCommand,
} from '../../../services/nswag/api-nswag-client';
import { BaseTable } from '../../../shared/base-table/base-table';
import { ConfirmDialogUi } from '../../../shared/confirm-dialog/confirm-dialog';
import { LoaderUi } from '../../../shared/loader/loader';
@Component({
  selector: 'app-polling-stations-ui',
  imports: [
    TranslatePipe,
    RouterLink,
    PermissionDirective,
    NgClass,
    MatTableModule,
    MatSortModule,
    MatPaginatorModule,
    MatFormFieldModule,
    MatInputModule,
    LoaderUi,
  ],
  templateUrl: './polling-stations-ui.html',
  styleUrl: './polling-stations-ui.scss',
})
export class PollingStationsUi extends BaseTable<GetPollingStationsResponse> {
  private readonly pollingStationService = inject(PollingStationApiService);
  private readonly translateService = inject(TranslateService);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);

  readonly displayedColumns = signal<string[]>([
    'stationNumber',
    'votingLocationName',
    'municipalityName',
    'subPrefectureName',
    'departmentName',
    'regionName',
    'actions',
  ]);

  isDeleting = signal(false);
  private currentSearch = signal('');
  private searchSubject = new BehaviorSubject<string>('');

  constructor() {
    super();

    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((value) => {
        this.currentSearch.set(value);
        if (this.paginator) {
          this.paginator.pageIndex = 0;
        }
        this.refreshData();
      });
  }

  override getData(
    query: GridDataQuery,
  ): Observable<{ data: GetPollingStationsResponse[]; total: number }> {
    const search = this.currentSearch().trim();

    return this.pollingStationService
      .getPollingStations({
        ...query,
        search: search || undefined,
      })
      .pipe(
        map((res) => ({
          data: res.data ?? [],
          total: res.total ?? 0,
        })),
        catchError(() => {
          return of({ data: [], total: 0 });
        }),
      );
  }

  onToggleActivePollingStation(ps: GetPollingStationsResponse): void {
    if (!ps.stationId) return;

    const cmd: ToggleActivatePollingStationCommand = {
      id: ps.stationId,
    };
    this.pollingStationService
      .toggleActivePollingStation(cmd, {
        successMessage: this.translateService.instant(
          'pollingStations.disablePollinStationSuccess',
        ),
        errorMessage: this.translateService.instant('pollingStations.disablePollinStationError'),
      })
      .subscribe({
        next: () => this.refreshData(),
        error: () => undefined,
      });
  }

  onDelete(ps: GetPollingStationsResponse): void {
    const dialogRef = this.dialog.open(ConfirmDialogUi, {
      width: '400',
      data: { name: `${ps.stationNumber}` },
    });

    const successMsg = this.translateService.instant('pollingStations.deletePollingStationSuccess');
    const errorMsg = this.translateService.instant('pollingStations.deletePollingStationError');

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.pollingStationService
          .deletePollingStation(ps.stationId!, {
            successMessage: successMsg,
            errorMessage: errorMsg,
          })
          .pipe(
            finalize(() => this.isDeleting.set(false)),
            takeUntil(this.destroy$),
          )
          .subscribe({
            next: () => {
              this.refreshData();
            },
            error: () => undefined,
          });
      }
    });
  }
}
