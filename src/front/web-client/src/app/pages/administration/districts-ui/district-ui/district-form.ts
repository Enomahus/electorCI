import { FormControl, FormGroup, Validators } from '@angular/forms';
import { startWith } from 'rxjs';
import { ElectoralDistrictLevel } from '../../../../services/nswag/api-nswag-client';

export type DistrictForm = FormGroup<{
  code: FormControl<string>;
  wording: FormControl<string>;
  level: FormControl<ElectoralDistrictLevel>;
  parentId: FormControl<number | undefined>;
  isActive: FormControl<boolean>;
}>;

export function createDistrictForm(): DistrictForm {
  const form = new FormGroup({
    code: new FormControl<string>('', { nonNullable: true }),
    wording: new FormControl<string>('', { validators: Validators.required, nonNullable: true }),
    level: new FormControl<ElectoralDistrictLevel>('region', {
      validators: Validators.required,
      nonNullable: true,
    }),
    parentId: new FormControl<number | undefined>(undefined, {
      nonNullable: false,
    }),
    isActive: new FormControl<boolean>(true, {
      validators: Validators.required,
      nonNullable: true,
    }),
  }) as DistrictForm;

  // Une région n'a pas de parent : parentId n'est requis que pour les niveaux inférieurs.
  form.controls.level.valueChanges
    .pipe(startWith(form.controls.level.value))
    .subscribe((level) => {
      const parentIdControl = form.controls.parentId;
      if (level === 'region') {
        parentIdControl.clearValidators();
      } else {
        parentIdControl.setValidators(Validators.required);
      }
      parentIdControl.updateValueAndValidity({ emitEvent: false });
    });

  return form;
}
