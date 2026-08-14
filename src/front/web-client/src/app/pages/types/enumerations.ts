import {
  ElectoralDistrictLevel,
  Gender,
  MaritalStatus,
  PersonTitle,
  RegistrationRequestDocumentType,
  RegistrationRequestType,
} from '../../services/nswag/api-nswag-client';

export const allLocationLevel: ElectoralDistrictLevel[] = [
  'region',
  'department',
  'subPrefecture',
  'municipality',
  'votingLocation',
];

export const allRegistrationRequestType: RegistrationRequestType[] = [
  'registrationDataUpdate',
  'registrationRequest',
];
export const allMaritalStatus: MaritalStatus[] = ['single', 'married', 'divorced', 'widowed'];
export const allGenders: Gender[] = ['feminine', 'masculine'];
export const allPersonTitle: PersonTitle[] = ['mr', 'mrs', 'ms'];
export const allRegistrationDocumentType: RegistrationRequestDocumentType[] = [
  'identityDocumentOrNationalCertificate',
  'residenceCertificate',
  'passportPhoto',
];
