import { Component, computed, inject, input, OnInit, output, signal } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { DistrictApiService } from '../../services/api/district.api.service';
import { UserApiService } from '../../services/api/user.api.service';
import { DistrictTreeHelperService } from '../../services/district-tree-helper.service';
import { GetDistrictsResponse, UserModel } from '../../services/nswag/api-nswag-client';
import { UserFormFactory } from './user-form';

@Component({
  selector: 'app-user-form-ui',
  imports: [TranslatePipe],
  templateUrl: './user-form-ui.html',
  styleUrl: './user-form-ui.scss',
})
export class UserFormUi implements OnInit {
  form = input.required<UserFormFactory>();
  isSaving = input<boolean>(false);
  formSubmitted = output<UserModel>();
  goBack = output<void>();

  private readonly districtService = inject(DistrictApiService);
  private readonly userService = inject(UserApiService);
  private readonly store = inject(DistrictTreeHelperService);

  hidePassword = signal(true);
  hideConfirmPassword = signal(true);

  nodes = this.store.nodesData;
  selectedNode = this.store.selectedNode;

  allRegion = signal<GetDistrictsResponse[] | null>(null);
  selectedRegionId = signal<number | null>(null);
  selectedDepartmentId = signal<number | null>(null);
  selectedSubPrefectureId = signal<number | null>(null);
  selectedMunicipalityId = signal<number | null>(null);

  allDepartement = computed<GetDistrictsResponse[]>(() => {
    const regionId = this.selectedRegionId();
    if (!regionId) return [];

    const selectedRegion = this.allRegion()?.find((r) => r.id === regionId);
    return selectedRegion?.children ?? [];
  });

  allSubPrefectures = computed<GetDistrictsResponse[]>(() => {
    const departmentId = this.selectedDepartmentId();
    if (!departmentId) return [];

    const selectedDepartment = this.allDepartement()?.find((d) => d.id === departmentId);
    return selectedDepartment?.children ?? [];
  });

  allMunicipalities = computed<GetDistrictsResponse[]>(() => {
    const subPrefectureId = this.selectedSubPrefectureId();
    if (!subPrefectureId) return [];

    const selectedSubPrefecture = this.allSubPrefectures()?.find((sp) => sp.id === subPrefectureId);
    return selectedSubPrefecture?.children ?? [];
  });

  allVottingLocation = computed<GetDistrictsResponse[]>(() => {
    const municipalityId = this.selectedMunicipalityId();
    if (!municipalityId) return [];

    const selectedMunicipality = this.allMunicipalities()?.find((vl) => vl.id === municipalityId);
    return selectedMunicipality?.children ?? [];
  });

  ngOnInit(): void {
    this.districtService.getDistricts({}).subscribe({
      next: (result) => {
        const regionDataLevel = result?.filter((d) => d.level === 'region');
        this.allRegion.set(regionDataLevel);
      },
    });
  }

  
}
