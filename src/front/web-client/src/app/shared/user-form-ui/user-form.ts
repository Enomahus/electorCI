import { effect, Injector, runInInjectionContext } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { patternPassword } from '../../constants';
import { AuthProvider } from '../../services/nswag/api-nswag-client';
import { passwordMatchValidator, phoneNumberValidator } from '../helpers/form.helper';

export type UserFormFactory = FormGroup<{
  //civility: FormControl<PersonTitle>;
  lastName: FormControl<string | undefined>;
  firstName: FormControl<string | undefined>;
  phone: FormControl<string | undefined>;
  email: FormControl<string | undefined>;
  password: FormControl<string | undefined>;
  confirmPassword: FormControl<string | undefined>;
  employeNumber: FormControl<string | undefined>;
  roles: FormControl<string[]>;
  authProvider: FormControl<AuthProvider | undefined>;
  districtId: FormControl<number | undefined>;
}>;

export function createUserForm(isEditMode: boolean, injector?: Injector): UserFormFactory {
  const form = new FormGroup(
    {
      //civility: new FormControl<PersonTitle>('mr', { nonNullable: true }),
      lastName: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required],
      }),
      firstName: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required],
      }),
      employeNumber: new FormControl<string | undefined>(undefined),
      phone: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required, phoneNumberValidator()],
      }),
      email: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required, Validators.email],
      }),
      password: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required, Validators.pattern(patternPassword)],
      }),
      confirmPassword: new FormControl<string | undefined>(undefined, {
        validators: [Validators.required],
      }),
      roles: new FormControl<string[]>([], { nonNullable: true }),
      authProvider: new FormControl<AuthProvider | undefined>({ value: undefined, disabled: true }),
      districtId: new FormControl<number | undefined>({ value: undefined, disabled: true }),
    },
    {
      validators: [
        passwordMatchValidator('password', 'confirmPassword'),
        employeeNumberRequiredValidator,
      ],
    },
  ) as UserFormFactory;

  if (isEditMode) {
    form.controls.password.clearValidators();
    form.controls.password.updateValueAndValidity({ emitEvent: false });
    form.controls.confirmPassword.clearValidators();
    form.controls.confirmPassword.updateValueAndValidity({ emitEvent: false });
  }

  const setupReactivity = () => {
    const rolesSignal = toSignal(form.controls.roles.valueChanges, {
      initialValue: form.controls.roles.value,
    });

    // L'effet réagit automatiquement aux changements du signal
    effect(() => {
      const roles = rolesSignal();
      const employeeControl = form.controls.employeNumber;

      const isOnlyDemandeur =
        Array.isArray(roles) && roles.length === 1 && roles[0] === 'requester';

      if (!isOnlyDemandeur) {
        employeeControl.setValidators([Validators.required]);
      } else {
        employeeControl.clearValidators();
      }
      employeeControl.updateValueAndValidity({ emitEvent: false });
    });
  };

  // Si on est dans une méthode statique hors injection context, on utilise l'Injector fourni
  if (injector) {
    runInInjectionContext(injector, setupReactivity);
  } else {
    // Supposé être appelé directement dans un constructor() ou lors de l'initialisation des champs d'un composant
    setupReactivity();
  }

  return form;
}

export const employeeNumberRequiredValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const roles = control.get('roles')?.value as string[] | undefined;
  const employeeNumber = control.get('employeeNumber')?.value;

  const isProfessional = roles?.some((r) => r === 'agent' || r === 'admin');

  if (isProfessional && !employeeNumber) {
    return { employeeNumberRequired: true };
  }

  return null;
};
