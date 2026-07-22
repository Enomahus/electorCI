import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { PhoneNumberUtil } from 'google-libphonenumber';

const phoneUtil = PhoneNumberUtil.getInstance();

export function passwordMatchValidator(password: string, confirmPassword: string): ValidatorFn {
  return (control: AbstractControl): Record<string, boolean> | null => {
    const passwordValue = control.get(password)?.value;
    const confirmPasswordValue = control.get(confirmPassword)?.value;

    if (!passwordValue || !confirmPasswordValue) return null;

    const isMatch = passwordValue === confirmPasswordValue;
    return isMatch ? null : { passwordMismatch: true };
  };
}

export function phoneNumberValidator() {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;

    try {
      const phoneNumber = phoneUtil.parse(control.value);
      return phoneUtil.isValidNumber(phoneNumber) ? null : { pattern: true };
    } catch (e) {
      return { pattern: true };
    }
  };
}
