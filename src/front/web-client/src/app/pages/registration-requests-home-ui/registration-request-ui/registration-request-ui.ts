import { JsonPipe } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  OnChanges,
  OnInit,
  output,
  signal,
  SimpleChanges,
} from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { DistrictNode } from '../../../models/district.model';
import { AuthService } from '../../../services/auth/auth.service';
import { PermissionDirective } from '../../../services/auth/permission.directive';
import { BreadcrumbService } from '../../../services/breadcrumb.service';
import { DistrictTreeHelperService } from '../../../services/district-tree-helper.service';
import {
  GetRegistrationRequestResponse,
  RegistrationRequestModel,
  RegistrationStatus,
} from '../../../services/nswag/api-nswag-client';
import { InputDatepickerUi } from '../../../shared/input-datepicker-ui/input-datepicker-ui';
import { LoaderUi } from '../../../shared/loader/loader';
import { SearchOrCreateCitizenUi } from '../../../shared/search-or-create-citizen-ui/search-or-create-citizen-ui';
import { StickyButtonsContainerComponent } from '../../../shared/sticky-buttons-container/sticky-buttons-container.component';
import { UploadMultipleUi } from '../../../shared/upload-multiple-ui/upload-multiple-ui';
import {
  allGenders,
  allMaritalStatus,
  allPersonTitle,
  allRegistrationDocumentType,
  allRegistrationRequestType,
} from '../../types/enumerations';
import {
  CitizenForm,
  createRegistrationRequestForm,
  createSearchCreateCitizenForm,
  RegistrationRequestForm,
  RequestDocumentsForm,
  RequestForm,
  ResidenceForm,
} from './registration-request-form';

@Component({
  selector: 'app-registration-request-ui',
  imports: [
    TranslatePipe,
    FormsModule,
    ReactiveFormsModule,
    StickyButtonsContainerComponent,
    UploadMultipleUi,
    LoaderUi,
    PermissionDirective,
    InputDatepickerUi,
    SearchOrCreateCitizenUi,
    JsonPipe,
  ],
  templateUrl: './registration-request-ui.html',
  styleUrl: './registration-request-ui.scss',
})
export class RegistrationRequestUi implements OnInit, OnChanges {
  private readonly translateService = inject(TranslateService);
  private readonly store = inject(DistrictTreeHelperService);
  private readonly authService = inject(AuthService);
  private readonly breadcrumbService = inject(BreadcrumbService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  isSaving = input<boolean>(false);
  isEditMode = input<boolean>(false);
  selectedStatus = input<RegistrationStatus | null>(null);
  registrationRequest = input<GetRegistrationRequestResponse | undefined>(undefined);
  save = output<{
    registrationRequest: RegistrationRequestModel;
    identityDocs: File[];
    residenceCertificates: File[];
    photos: File[];
  }>();
  goBack = output<void>();

  isLoading = signal<boolean>(false);
  form = signal<RegistrationRequestForm>(createRegistrationRequestForm());
  searchCreateCitizenForm = createSearchCreateCitizenForm();
  canChangeCitizen = signal<boolean>(true);
  registrationRequestId = signal<string | undefined>(undefined);
  districtSelected = signal<DistrictNode[]>([]);
  selectedDistrictId = signal<number | null>(null);
  masculineGender = allGenders.find((g) => g === 'masculine');
  feminineGender = allGenders.find((g) => g === 'feminine');

  allRegistrationRequestType = allRegistrationRequestType;
  allMaritalStatus = allMaritalStatus;
  allGenders = allGenders;
  allPersonTitle = allPersonTitle;
  allRegistrationDocumentType = allRegistrationDocumentType;

  allRegions = this.store.nodesData;

  selectedRegionId = signal<number | null>(null);
  selectedDepartmentId = signal<number | null>(null);
  selectedSubPrefectureId = signal<number | null>(null);
  selectedMunicipalityId = signal<number | null>(null);
  selectedVotingLocationId = signal<number | null>(null);

  requestForm(): RequestForm {
    return this.form().controls.request;
  }
  citizenForm(): CitizenForm {
    return this.form().controls.citizen;
  }
  requestDocumentsForm(): RequestDocumentsForm {
    return this.form().controls.requestDocuments;
  }
  residenceForm(): ResidenceForm {
    return this.form().controls.residence;
  }

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

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['citizenId'] && changes['citizenId'].currentValue !== undefined) {
      this.canChangeCitizen.set(false);
      this.searchCreateCitizenForm.controls.citizenId.setValue(changes['citizenId'].currentValue);
      this.searchCreateCitizenForm.controls.citizenId.disable();
    }
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.params['id'];
    if (idParam) {
      this.registrationRequestId.set(idParam as string);
    }
    this.setBreadcrums();
  }

  onRegionChange(regionId: number | null): void {
    this.selectedRegionId.set(regionId);
    this.onSelectDistrcit(regionId!);
    this.selectedDepartmentId.set(null);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
  }
  onDepartmentChange(departmentId: number | null): void {
    this.selectedDepartmentId.set(departmentId);
    this.onSelectDistrcit(departmentId!);
    this.selectedSubPrefectureId.set(null);
    this.selectedMunicipalityId.set(null);
  }

  onSubPrefectureChange(subPrefectureId: number | null): void {
    this.selectedSubPrefectureId.set(subPrefectureId);
    this.onSelectDistrcit(subPrefectureId!);
    this.selectedMunicipalityId.set(null);
  }

  onMunicipalityChange(municipalityId: number | null): void {
    this.selectedMunicipalityId.set(municipalityId);
    this.onSelectDistrcit(municipalityId!);
  }

  onVotingLocationChange(votingLocationId: number | null): void {
    this.selectedVotingLocationId.set(votingLocationId);
    this.onSelectDistrcit(votingLocationId!);
  }

  onSelectDistrcit(id: number): void {
    this.selectedDistrictId.set(id);
    let node = this.store.findNode(id);
    if (node) this.districtSelected.set([node]);
    this.form().controls.request.controls.districtId.setValue(id);
  }

  protected parseDistrictId(event: Event): number | null {
    const value = (event.target as HTMLSelectElement).value;
    return value ? Number(value) : null;
  }

  private getRegistrationRequestModel(): RegistrationRequestModel {
    const formValue = this.form().getRawValue();
    const { id, request, citizen, requestDocuments } = formValue;
    return {
      id,
      districtId: request?.districtId,
      requestType: request?.requestType,
      comment: request?.comment,
      citizen: citizen
        ? {
            ...citizen,
            birthDate: citizen.birthDate ? citizen.birthDate.toISOString() : undefined,
          }
        : undefined,
      identityDocumentIds: requestDocuments.identityDocAttachments?.distantFileIds,
      residenceCertificateIds: requestDocuments.residenceCertificateAttachments?.distantFileIds,
      photoIds: requestDocuments.photoAttachments?.distantFileIds,
    };
  }

  onSave(): void {
    if (this.form().invalid) {
      this.form().markAllAsTouched();
      return;
    }

    const registrationRequest = this.getRegistrationRequestModel();
    const identityDocAttachments =
      this.form().value.requestDocuments?.identityDocAttachments?.localFiles ?? [];
    const residenceCertificateAttachments =
      this.form().value.requestDocuments?.residenceCertificateAttachments?.localFiles ?? [];
    const photoAttachments = this.form().value.requestDocuments?.photoAttachments?.localFiles ?? [];

    this.save.emit({
      registrationRequest,
      identityDocs: identityDocAttachments,
      residenceCertificates: residenceCertificateAttachments,
      photos: photoAttachments,
    });
  }

  cancel(): void {
    this.goBack.emit();
  }

  setBreadcrums(): void {
    this.authService.getPermissions().subscribe((permissions) => {
      let label = '';
      let targetRoute = '/home';

      if (permissions.includes('accessRegistrationRequestsForAdminPage')) {
        targetRoute = '/registration-requests-for-admin';
      } else if (permissions.includes('accessRegistrationRequestsForManagementPage')) {
        targetRoute = '/registration-requests-for-management';
      } else if (permissions.includes('accessRegistrationRequestsPage')) {
        targetRoute = '/registration-requests';
      }

      if (permissions.some((p) => p === 'getRegistrationRequestForAdmin')) {
        label = this.translateService.instant('breadcrumb.registrationRequestsForAdmin');
      } else if (permissions.some((p) => p === 'getRegistrationRequestForManagement')) {
        label = this.translateService.instant('breadcrumb.registrationRequestsForManagement');
      } else if (permissions.some((p) => p === 'createRegistrationRequest')) {
        label = this.translateService.instant('breadcrumb.registrationRequests');
      }

      this.breadcrumbService.setBreadcrumbs([
        {
          label: label,
          url: targetRoute,
        },
        {
          label: this.isEditMode()
            ? (this.registrationRequest()?.reference ?? '')
            : this.translateService.instant('breadcrumb.registrationRequestAdd'),
        },
      ]);
    });
  }
}
