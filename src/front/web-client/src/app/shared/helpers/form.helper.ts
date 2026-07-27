import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import * as lpn from 'google-libphonenumber';
import { PhoneNumberUtil } from 'google-libphonenumber';
import { CountryData } from '../../models/country.model';

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

export function getCountriesList(lang: string): CountryData[] {
  const phoneUtil = lpn.PhoneNumberUtil.getInstance();
  const regions = phoneUtil.getSupportedRegions();
  const countryList: CountryData[] = [];

  const regionNames = new Intl.DisplayNames([lang], { type: 'region' });

  regions.forEach((regionCode) => {
    try {
      const dialCode = phoneUtil.getCountryCodeForRegion(regionCode).toString();
      const countryName = regionNames.of(regionCode) || regionCode;

      countryList.push({
        name: countryName,
        code: regionCode,
        dial: dialCode,
        flag: regionCode.toLowerCase(),
      });
    } catch (e) {}
  });
  return countryList;
}
