import { FormControl, FormGroup, Validators } from '@angular/forms';

export type PollingStationForm = FormGroup<{
  stationNumber: FormControl<string | undefined>;
  wording: FormControl<string | undefined>;
  districtId: FormControl<number | undefined>;
  isActive: FormControl<boolean>;
}>;

export function createPollingStationForm(isEditMode: boolean): PollingStationForm {
  const stationNumberValidator = isEditMode ? [Validators.required] : [];

  const form = new FormGroup({
    stationNumber: new FormControl<string | undefined>(undefined, {
      validators: stationNumberValidator,
      nonNullable: true,
    }),
    wording: new FormControl<string | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    districtId: new FormControl<number | undefined>(undefined, {
      validators: Validators.required,
      nonNullable: true,
    }),
    isActive: new FormControl<boolean>(false, {
      validators: Validators.required,
      nonNullable: true,
    }),
  }) as PollingStationForm;

  return form;
}
