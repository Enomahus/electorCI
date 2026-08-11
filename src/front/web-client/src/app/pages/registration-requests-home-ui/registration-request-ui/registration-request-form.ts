import { FormControl, FormGroup, Validators } from '@angular/forms';
import {
  Gender,
  MaritalStatus,
  RegistrationRequestType,
  RegistrationStatus,
} from '../../../services/nswag/api-nswag-client';
import { UploadMultipleFormValue } from '../../../shared/upload-multiple-ui/upload-multiple-form-value';

export type RegistrationRequestForm = FormGroup<{
  id: FormControl<string | undefined>;
  residence: ResidenceForm;
  request: RequestForm;
  citizen: CitizenForm;
  requestDocuments: RequestDocumentsForm;
}>;

export type ResidenceForm = FormGroup<{
  regionId: FormControl<number | undefined>;
  departmentId: FormControl<number | undefined>;
  subPrefectureId: FormControl<number | undefined>;
  municipalityId: FormControl<number | undefined>;
  vottingLocationId: FormControl<number | undefined>;
}>;

export type RequestForm = FormGroup<{
  districtId: FormControl<number | undefined>;
  requestType: FormControl<RegistrationRequestType | undefined>;
  requestStatus: FormControl<RegistrationStatus | undefined>;
  comment: FormControl<string | undefined>;
}>;

export type CitizenForm = FormGroup<{
  gender: FormControl<Gender | undefined>;
  firstName: FormControl<string | undefined>;
  lastName: FormControl<string | undefined>;
  birthDate: FormControl<Date | undefined>;
  birthPlace: FormControl<string | undefined>;
  maritalStatus: FormControl<MaritalStatus | undefined>;
  marriedName: FormControl<string | undefined>;
  nationality: FormControl<string | undefined>;
  profession: FormControl<string | undefined>;
  email: FormControl<string | undefined>;
  physicalAddress: FormControl<string | undefined>;
  postalAddress: FormControl<string | undefined>;
  fatherId: FormControl<string | undefined>;
  motherId: FormControl<string | undefined>;
}>;

export type RequestDocumentsForm = FormGroup<{
  identityDocAttachments: FormControl<UploadMultipleFormValue | undefined>;
  residenceCertificateAttachments: FormControl<UploadMultipleFormValue | undefined>;
  photoAttachments: FormControl<UploadMultipleFormValue | undefined>;
}>;

export function createRequestForm(): RequestForm {
  return new FormGroup({
    districtId: new FormControl<number | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    requestType: new FormControl<RegistrationRequestType | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    comment: new FormControl<string | undefined>(undefined, {
      validators: Validators.maxLength(500),
      nonNullable: true,
    }),
    requestStatus: new FormControl<RegistrationStatus | undefined>('toBeProcessed', {
      nonNullable: true,
    }),
  }) as RequestForm;
}

export function createResidenceForm(): ResidenceForm {
  return new FormGroup({
    regionId: new FormControl<number | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    departmentId: new FormControl<number | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    subPrefectureId: new FormControl<number | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    municipalityId: new FormControl<number | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    vottingLocationId: new FormControl<number | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
  }) as ResidenceForm;
}

export function createRequestDocumentsForm(): RequestDocumentsForm {
  return new FormGroup({
    identityDocAttachments: new FormControl<UploadMultipleFormValue | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    residenceCertificateAttachments: new FormControl<UploadMultipleFormValue | undefined>(
      undefined,
      {
        validators: Validators.required,
        nonNullable: true,
      },
    ),
    photoAttachments: new FormControl<UploadMultipleFormValue | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
  }) as RequestDocumentsForm;
}

export function createCitizenForm(): CitizenForm {
  return new FormGroup({
    gender: new FormControl<Gender | undefined>(undefined, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    firstName: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    lastName: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    birthDate: new FormControl<Date | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    birthPlace: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    maritalStatus: new FormControl<MaritalStatus | undefined>(undefined, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    marriedName: new FormControl<string | undefined>(undefined),
    nationality: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    profession: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    email: new FormControl<string | undefined>(undefined, {
      nonNullable: true,
    }),
    physicalAddress: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    postalAddress: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    fatherId: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    motherId: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
  }) as CitizenForm;
}

export function createRegistrationRequestForm(): RegistrationRequestForm {
  return new FormGroup({
    id: new FormControl<string | undefined>(undefined, { nonNullable: true }),
    request: createRequestForm(),
    citizen: createCitizenForm(),
    requestDocuments: createRequestDocumentsForm(),
    residence: createResidenceForm(),
  }) as RegistrationRequestForm;
}
