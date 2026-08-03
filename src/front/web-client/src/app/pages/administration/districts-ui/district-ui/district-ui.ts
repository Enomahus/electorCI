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
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { combineLatest, filter, startWith, switchMap, take } from 'rxjs';
import { DistrictNode } from '../../../../models/district.model';
import { DistrictTreeHelperService } from '../../../../services/district-tree-helper.service';
import {
  DistrictModel,
  ElectoralDistrictLevel,
  GetDistrictResponse,
} from '../../../../services/nswag/api-nswag-client';
import { LoaderUi } from '../../../../shared/loader/loader';
import { StickyButtonsContainerComponent } from '../../../../shared/sticky-buttons-container/sticky-buttons-container.component';
import { allLocationLevel } from '../../../types/enumerations';
import { DistrictForm } from './district-form';

@Component({
  selector: 'app-district-ui',
  imports: [TranslatePipe, ReactiveFormsModule, LoaderUi, StickyButtonsContainerComponent],
  templateUrl: './district-ui.html',
  styleUrl: './district-ui.scss',
})
export class DistrictUi implements OnInit {
  saveDistrict = output<DistrictModel>();
  goBack = output<void>();
  isSaving = input(false);
  form = input.required<DistrictForm>();
  isEditMode = input<boolean>(false);
  district = input<GetDistrictResponse | null>(null);

  private readonly store = inject(DistrictTreeHelperService);
  private readonly destroyRef = inject(DestroyRef);

  private readonly nodes$ = toObservable(this.store.nodesData);
  private readonly district$ = toObservable(this.district);

  readonly levels: ElectoralDistrictLevel[] = allLocationLevel;

  // Reflète la valeur courante du contrôle "level" sous forme de signal, afin de dériver
  // réactivement les niveaux parents à afficher (indépendamment de qui modifie le formulaire).
  private readonly selectedLevel = toSignal(
    toObservable(this.form).pipe(
      switchMap((form) =>
        form.controls.level.valueChanges.pipe(startWith(form.controls.level.value)),
      ),
    ),
    { initialValue: this.levels[0] },
  );

  private readonly levelIndex = computed(() => this.levels.indexOf(this.selectedLevel()));

  // Seuls les niveaux au-dessus du niveau sélectionné servent à choisir le parent :
  // les niveaux inférieurs (ou égaux) n'ont pas besoin d'être affichés.
  readonly ancestorLevels = computed<ElectoralDistrictLevel[]>(() =>
    this.levels.slice(0, this.levelIndex()),
  );

  readonly showDepartment = computed(() => this.ancestorLevels().includes('department'));
  readonly showSubPrefecture = computed(() => this.ancestorLevels().includes('subPrefecture'));
  readonly showMunicipality = computed(() => this.ancestorLevels().includes('municipality'));

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

  ngOnInit(): void {
    if (!this.isEditMode()) return;

    combineLatest([
      this.nodes$.pipe(filter((nodes) => nodes.length > 0)),
      this.district$.pipe(filter((district): district is GetDistrictResponse => !!district)),
    ])
      .pipe(take(1), takeUntilDestroyed(this.destroyRef))
      .subscribe(([, district]) => this.initializeFromDistrict(district));
  }

  onLevelChange(): void {
    this.resetAncestorChain();
    this.form().controls.parentId.setValue(undefined);
  }

  onRegionChange(regionId: number | null): void {
    this.selectedRegionId.set(regionId);
    this.selectedDepartmentId.set(null);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
    this.syncParentId();
  }

  onDepartmentChange(departmentId: number | null): void {
    this.selectedDepartmentId.set(departmentId);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
    this.syncParentId();
  }

  onSubPrefectureChange(subPrefectureId: number | null): void {
    this.selectedSubPrefectureId.set(subPrefectureId);
    this.selectedMunicipalityId.set(null);
    this.syncParentId();
  }

  onMunicipalityChange(municipalityId: number | null): void {
    this.selectedMunicipalityId.set(municipalityId);
    this.syncParentId();
  }

  protected parseDistrictId(event: Event): number | null {
    const value = (event.target as HTMLSelectElement).value;
    return value ? Number(value) : null;
  }

  onSubmit(): void {
    const form = this.form();
    form.markAllAsTouched();

    if (form.invalid) return;

    this.saveDistrict.emit(form.getRawValue());
  }

  private initializeFromDistrict(district: GetDistrictResponse): void {
    this.form().patchValue({
      code: district.code,
      wording: district.wording,
      level: district.level,
      parentId: district.parentId,
      isActive: district.isActive,
    });

    if (district.parentId) {
      this.initializeAncestorChain(district.parentId);
    }
  }

  // Remonte l'arbre à partir du parent courant afin de pré-remplir chaque niveau
  // supérieur (région -> ... -> parent), quel que soit le niveau du district édité.
  private initializeAncestorChain(parentId: number): void {
    const idsByLevel = new Map<ElectoralDistrictLevel, number>();
    let node = this.store.findNode(parentId);

    while (node) {
      idsByLevel.set(node.level, node.id);
      node = node.parentId ? this.store.findNode(node.parentId) : undefined;
    }

    this.selectedRegionId.set(idsByLevel.get('region') ?? null);
    this.selectedDepartmentId.set(idsByLevel.get('department') ?? null);
    this.selectedSubPrefectureId.set(idsByLevel.get('subPrefecture') ?? null);
    this.selectedMunicipalityId.set(idsByLevel.get('municipality') ?? null);
  }

  private resetAncestorChain(): void {
    this.selectedRegionId.set(null);
    this.selectedDepartmentId.set(null);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
  }

  // Le parent du district est le dernier niveau ancêtre affiché (juste au-dessus du niveau choisi).
  private syncParentId(): void {
    const parentId = this.resolveParentId();
    this.form().controls.parentId.setValue(parentId ?? undefined);
  }

  private resolveParentId(): number | null {
    switch (this.levelIndex()) {
      case 1:
        return this.selectedRegionId();
      case 2:
        return this.selectedDepartmentId();
      case 3:
        return this.selectedSubPrefectureId();
      case 4:
        return this.selectedMunicipalityId();
      default:
        return null;
    }
  }

  private findChildren(nodes: DistrictNode[], parentId: number | null): DistrictNode[] {
    if (!parentId) return [];
    return nodes.find((n) => n.id === parentId)?.children ?? [];
  }
}
