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
import { takeUntilDestroyed, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { combineLatest, filter, startWith, take } from 'rxjs';
import { DistrictNode } from '../../models/district.model';
import { UserApiService } from '../../services/api/user.api.service';
import { DistrictTreeHelperService } from '../../services/district-tree-helper.service';
import { UserModel } from '../../services/nswag/api-nswag-client';
import { LoaderUi } from '../loader/loader';
import { PhoneInputUi } from '../phone-input-ui/phone-input-ui';
import { StickyButtonsContainerComponent } from '../sticky-buttons-container/sticky-buttons-container.component';
import { UserFormFactory } from './user-form';

@Component({
  selector: 'app-user-form-ui',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslatePipe,
    PhoneInputUi,
    StickyButtonsContainerComponent,
    LoaderUi,
  ],
  templateUrl: './user-form-ui.html',
  styleUrl: './user-form-ui.scss',
})
export class UserFormUi implements OnInit {
  form = input.required<UserFormFactory>();
  user = input<UserModel>();
  isSaving = input<boolean>(false);
  isEditMode = input<boolean>(false);
  formSubmitted = output<UserModel>();
  goBack = output<void>();

  private readonly userService = inject(UserApiService);
  private readonly store = inject(DistrictTreeHelperService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly nodes$ = toObservable(this.store.nodesData);

  hidePassword = signal(true);
  hideConfirmPassword = signal(true);
  allDistrict = signal<DistrictNode[]>([]);
  selectedDistrictId = signal<number | null>(null);
  roles = toSignal(this.userService.getUserRoles(), { initialValue: [] });

  allRegions = this.store.nodesData;

  selectedRegionId = signal<number | null>(null);
  selectedDepartmentId = signal<number | null>(null);
  selectedSubPrefectureId = signal<number | null>(null);
  selectedMunicipalityId = signal<number | null>(null);
  selectedVotingLocationId = signal<number | null>(null);
  selectedRoleId = signal<string | null>(null);

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

  // En création, chaque niveau reste verrouillé tant que son parent n'est pas choisi.
  // En édition, tous les niveaux sont actifs car ils sont déjà déduits du districtId existant.
  isDepartmentDisabled = computed(() => this.selectedRegionId() == null);
  isSubPrefectureDisabled = computed(() => this.selectedDepartmentId() == null);
  isMunicipalityDisabled = computed(() => this.selectedSubPrefectureId() == null);
  isVotingLocationDisabled = computed(() => this.selectedMunicipalityId() == null);

  ngOnInit(): void {
    this.setupEmployeeNumberValidation();
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
      .subscribe(([, districtId]) => this.initializeChainFromDistrict(districtId!));
  }

  private setupEmployeeNumberValidation(): void {
    const rolesControl = this.form().controls.roles;
    const employeeNuberControl = this.form().controls.employeeNumber;

    rolesControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((roles) => {
      const requiresEmployeeNumber = this.requiredEmployeeNumber();

      if (requiresEmployeeNumber) {
        employeeNuberControl.setValidators([Validators.required]);
      } else {
        employeeNuberControl.clearValidators();
      }
      employeeNuberControl.updateValueAndValidity();
    });
  }

  requiredEmployeeNumber(): boolean {
    const selectedRoleIds = this.form().controls.roles.value;
    const allRoles = this.roles();
    if (selectedRoleIds.length === 0) return false;

    const selectedRoles = allRoles.filter((r) => selectedRoleIds.includes(r.id!));

    if (selectedRoles.length === 0) return false;

    return selectedRoles.some((r) => r.name !== 'ElectorRole');
  }

  onRegionChange(regionId: number | null): void {
    this.selectedRegionId.set(regionId);
    this.onSelectDistrcit(regionId!);
    this.selectedDepartmentId.set(null);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
    //this.setMunicipality(null);
  }

  onDepartmentChange(departmentId: number | null): void {
    this.selectedDepartmentId.set(departmentId);
    this.onSelectDistrcit(departmentId!);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
    //this.setMunicipality(null);
  }

  onSubPrefectureChange(subPrefectureId: number | null): void {
    this.selectedSubPrefectureId.set(subPrefectureId);
    this.onSelectDistrcit(subPrefectureId!);
    this.selectedMunicipalityId.set(null);
    //this.setMunicipality(null);
  }

  onMunicipalityChange(municipalityId: number | null): void {
    this.selectedMunicipalityId.set(municipalityId);
    this.onSelectDistrcit(municipalityId!);
    //this.setMunicipality(municipalityId);
  }

  onVotingLocationChange(votingLocationId: number | null): void {
    this.setVotingLocation(votingLocationId);
    this.onSelectDistrcit(votingLocationId!);
  }

  onSelectDistrcit(id: number): void {
    this.selectedDistrictId.set(id);
    let node = this.store.findNode(id);
    if (node) this.allDistrict.set([node]);
    this.form().controls.districtId.setValue(id);
  }

  protected parseDistrictId(event: Event): number | null {
    const value = (event.target as HTMLSelectElement).value;
    return value ? Number(value) : null;
  }

  // private setMunicipality(municipalityId: number | null): void {
  //   this.selectedMunicipalityId.set(municipalityId);
  //   this.form().controls.districtId.setValue(municipalityId ?? undefined);
  // }

  private setVotingLocation(votingLocationId: number | null): void {
    this.selectedVotingLocationId.set(votingLocationId);
    this.form().controls.districtId.setValue(votingLocationId ?? undefined);
  }

  // private findChildren(nodes: DistrictNode[], parentId: number | null): DistrictNode[] {
  //   if (!parentId) return [];
  //   return nodes.find((n) => n.id === parentId)?.children ?? [];
  // }

  // Le districtId d'un utilisateur peut pointer n'importe quel niveau (municipalité si aucun
  // lieu de vote n'a été choisi, lieu de vote sinon) : on remonte l'arbre via parentId en se
  // basant sur le niveau de chaque nœud rencontré, jusqu'à la région.
  private initializeChainFromDistrict(districtId: number): void {
    this.selectedRegionId.set(null);
    this.selectedDepartmentId.set(null);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
    this.selectedVotingLocationId.set(null);

    let node = this.store.findNode(districtId);
    this.allDistrict.set([node!]);
    if (node) this.selectedDistrictId.set(node.id);

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

  togglePassword() {
    this.hidePassword.update((v) => !v);
  }

  toggleConfirmPassword() {
    this.hideConfirmPassword.update((v) => !v);
  }

  onSubmit(): void {
    this.form().markAllAsTouched();

    if (this.form().invalid) return;

    const formValue: UserModel = this.form().getRawValue();
    this.formSubmitted.emit(formValue);
  }
}
