import { computed, inject, Injectable, signal } from "@angular/core";
import { DistrictTreeHelperService } from "./district-tree-helper.service";
import { DistrictNode } from "../models/district.model";

@Injectable({
  providedIn: 'root',
})
export class DistrictSynchronizeService {
  private readonly store = inject(DistrictTreeHelperService);

  allRegions = this.store.nodesData;

  selectedRegionId = signal<number | null>(null);
  selectedDepartmentId = signal<number | null>(null);
  selectedSubPrefectureId = signal<number | null>(null);
  selectedMunicipalityId = signal<number | null>(null);

  allDepartments = computed<DistrictNode[]>(() =>
    this.findChildren(this.allRegions(), this.selectedRegionId()),
  );
  allSubPrefectures = computed<DistrictNode[]>(() =>
    this.findChildren(this.allDepartments(), this.selectedDepartmentId()),
  );
  allMunicipalities = computed<DistrictNode[]>(() =>
    this.findChildren(this.allSubPrefectures(), this.selectedSubPrefectureId()),
  );

  isDepartmentDisabled = computed(() => this.selectedRegionId() == null);
  isSubPrefectureDisabled = computed(() => this.selectedDepartmentId() == null);
  isMunicipalityDisabled = computed(() => this.selectedSubPrefectureId() == null);

  private findChildren(nodes: DistrictNode[], parentId: number | null): DistrictNode[] {
    if (!parentId) return [];
    return nodes.find((n) => n.id === parentId)?.children ?? [];
  }
  
}
