import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ElectoralDistrictLevel } from '../../../../services/nswag/api-nswag-client';

export type DistrictForm = FormGroup<{
  code: FormControl<string>;
  wording: FormControl<string>;
  level: FormControl<ElectoralDistrictLevel>;
  parentId: FormControl<number | undefined>;
  isActive: FormControl<boolean>;
}>;

export function createDistrictForm(): DistrictForm {
  return new FormGroup({
    code: new FormControl<string>('', { nonNullable: true }),
    wording: new FormControl<string>('', { validators: Validators.required, nonNullable: true }),
    level: new FormControl<ElectoralDistrictLevel>('region', {
      validators: Validators.required,
      nonNullable: true,
    }),
    parentId: new FormControl<number | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: false,
    }),
    isActive: new FormControl<boolean>(true, {
      validators: Validators.required,
      nonNullable: true,
    }),
  }) as DistrictForm;
}
