import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  OnInit,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { combineLatest, filter, startWith, take } from 'rxjs';
import { Breadcrumb } from '../../../../models/breadcrumb.model';
import { DistrictNode } from '../../../../models/district.model';
import { BreadcrumbService } from '../../../../services/breadcrumb.service';
import { DistrictTreeHelperService } from '../../../../services/district-tree-helper.service';
import {
  GetPollingStationResponse,
  PollingStationModel,
} from '../../../../services/nswag/api-nswag-client';
import { LoaderUi } from '../../../../shared/loader/loader';
import { StickyButtonsContainerComponent } from '../../../../shared/sticky-buttons-container/sticky-buttons-container.component';
import { PollingStationForm } from './polling-station-form';

@Component({
  selector: 'app-polling-station-ui',
  imports: [
    TranslatePipe,
    CommonModule,
    ReactiveFormsModule,
    StickyButtonsContainerComponent,
    LoaderUi,
  ],
  templateUrl: './polling-station-ui.html',
  styleUrl: './polling-station-ui.scss',
})
export class PollingStationUi implements OnInit {
  private readonly translateService = inject(TranslateService);
  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly store = inject(DistrictTreeHelperService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly nodes$ = toObservable(this.store.nodesData);

  submittedForm = output<PollingStationModel>();
  goBack = output<void>();
  pollingStation = input<GetPollingStationResponse | null>(null);
  isSaving = input<boolean>(false);
  isEditMode = input<boolean>(false);
  form = input.required<PollingStationForm>();

  allRegions = this.store.nodesData;

  selectedRegionId = signal<number | null>(null);
  selectedDepartmentId = signal<number | null>(null);
  selectedSubPrefectureId = signal<number | null>(null);
  selectedMunicipalityId = signal<number | null>(null);
  selectedVotingLocationId = signal<number | null>(null);

  allDepartments = computed<DistrictNode[]>(() =>
    this.store.findChildren(this.allRegions(), this.selectedRegionId()),
  );

  allSubPrefectures = computed<DistrictNode[]>(() =>
    this.store.findChildren(this.allDepartments(), this.selectedDepartmentId()),
  );

  allMunicipalities = computed<DistrictNode[]>(() =>
    this.store.findChildren(this.allSubPrefectures(), this.selectedSubPrefectureId()),
  );

  allVotingLocations = computed<DistrictNode[]>(() =>
    this.store.findChildren(this.allMunicipalities(), this.selectedMunicipalityId()),
  );

  isDepartmentDisabled = computed(() => this.selectedRegionId() == null);
  isSubPrefectureDisabled = computed(() => this.selectedDepartmentId() == null);
  isMunicipalityDisabled = computed(() => this.selectedSubPrefectureId() == null);
  isVotingLocationDisabled = computed(() => this.selectedMunicipalityId() == null);

  ngOnInit(): void {
    if (this.pollingStation()) {
      this.setBreadcrumb(this.pollingStation()!);
    }
    if (!this.isEditMode()) return;

    const districtIdControl = this.form().controls.districtId;

    combineLatest([
      this.nodes$.pipe(filter((nodes) => nodes.length > 0)),
      districtIdControl.valueChanges.pipe(startWith(districtIdControl.value)),
    ])
      .pipe(
        filter(([, districtId]) => !!districtId),
        take(1),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(([, districtId]) => this.initilizeChainFromDistrict(districtId!));
  }

  private initilizeChainFromDistrict(districtId: number): void {
    this.selectedRegionId.set(null);
    this.selectedDepartmentId.set(null);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
    this.selectedVotingLocationId.set(null);
    let node = this.store.findNode(districtId);

    while (node) {
      switch (node.level) {
        case 'region':
          this.selectedRegionId.set(node.id);
          break;
        case 'department':
          this.selectedDepartmentId.set(node.id);
          break;
        case 'subPrefecture':
          this.selectedSubPrefectureId.set(node.id);
          break;
        case 'municipality':
          this.selectedMunicipalityId.set(node.id);
          break;
        case 'votingLocation':
          this.selectedVotingLocationId.set(node.id);
          break;
      }
      node = node.parentId ? this.store.findNode(node.parentId) : undefined;
    }
  }

  onRegionChange(regionId: number | null): void {
    this.selectedRegionId.set(regionId);
    this.selectedDepartmentId.set(null);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
  }

  onDepartmentChange(departmentId: number | null): void {
    this.selectedDepartmentId.set(departmentId);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
  }

  onSubPrefectureChange(subPrefectureId: number | null): void {
    this.selectedSubPrefectureId.set(subPrefectureId);
    this.selectedMunicipalityId.set(null);
  }

  onMunicipalityChange(municipalityId: number | null): void {
    this.selectedMunicipalityId.set(municipalityId);
  }

  onVotingLocationChange(votingLocationId: number | null): void {
    this.selectedVotingLocationId.set(votingLocationId);
    this.form().controls.districtId.setValue(votingLocationId!);
  }

  protected parseDistrictId(event: Event): number | null {
    const value = (event.target as HTMLSelectElement).value;
    return value ? Number(value) : null;
  }

  private setBreadcrumb(station: GetPollingStationResponse): void {
    let breadcrumb: Breadcrumb[] = [];

    breadcrumb = [
      {
        label: this.translateService.instant('breadcrumb.pollingStations'),
        url: `/admin/polling-stations`,
      },
      {
        label: this.isEditMode()
          ? `${station.stationNumber} ${station.wording}`
          : this.translateService.instant('breadcrumb.pollingStationCreate'),
      },
    ];
    this.breadcrumbService.setBreadcrumbs(breadcrumb);
  }

  onSubmit(): void {
    if (this.form().invalid) {
      this.form().markAsTouched();
      return;
    }
    const model = this.form().getRawValue() as PollingStationModel;
    this.submittedForm.emit(model);
  }
}
