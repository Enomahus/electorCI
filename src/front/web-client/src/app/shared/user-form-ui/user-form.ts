import {
  AbstractControl,
  FormControl,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { AuthProvider, PersonTitle } from '../../services/nswag/api-nswag-client';
import { passwordMatchValidator, phoneNumberValidator } from '../helpers/form.helper';

export type UserFormFactory = FormGroup<{
  civility: FormControl<PersonTitle>;
  lastName: FormControl<string | undefined>;
  firstName: FormControl<string | undefined>;
  phone: FormControl<string | undefined>;
  email: FormControl<string | undefined>;
  isActive: FormControl<boolean | undefined>;
  password: FormControl<string | undefined>;
  confirmPassword: FormControl<string | undefined>;
  employeeNumber: FormControl<string | undefined>;
  roles: FormControl<string[]>;
  authProvider: FormControl<AuthProvider | undefined>;
  districtId: FormControl<number | undefined>;
}>;

export function createUserForm(isEditMode: boolean): UserFormFactory {
  const passwordValidators = isEditMode ? [] : [Validators.required];
  const confirmPasswordValidators = isEditMode ? [] : [Validators.required];

  const form = new FormGroup(
    {
      civility: new FormControl<PersonTitle>('mr', { nonNullable: true }),
      lastName: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required],
      }),
      firstName: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required],
      }),
      employeeNumber: new FormControl<string | undefined>(undefined),
      phone: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required, phoneNumberValidator()],
      }),
      email: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required, Validators.email],
      }),
      isActive: new FormControl<boolean | undefined>(undefined, {
        validators: [Validators.required],
        nonNullable: true,
      }),
      password: new FormControl<string | undefined>(undefined, {
        validators: passwordValidators,
      }),
      confirmPassword: new FormControl<string | undefined>(undefined, {
        validators: confirmPasswordValidators,
      }),
      roles: new FormControl<string[]>([], { nonNullable: true }),
      authProvider: new FormControl<AuthProvider | undefined>('email', {
        validators: Validators.required,
        nonNullable: true,
      }),
      districtId: new FormControl<number | undefined>(undefined, {
        validators: Validators.required,
      }),
    },
    {
      validators: [passwordMatchValidator('password', 'confirmPassword')],
    },
  ) as UserFormFactory;

  //form.controls.authProvider.disable();

  return form;
}

export const employeeNumberRequiredValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const roles = control.get('roles')?.value as string[] | undefined;
  const employeeNumber = control.get('employeeNumber')?.value;

  const requiresEmployeeNumber = roles?.some((r) => r !== 'ElectorRole');

  if (requiresEmployeeNumber && !employeeNumber) {
    return { employeeNumberRequired: true };
  }

  return null;
};
