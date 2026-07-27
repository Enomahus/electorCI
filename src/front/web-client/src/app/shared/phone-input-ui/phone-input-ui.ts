import {
  Component,
  computed,
  ElementRef,
  forwardRef,
  HostListener,
  input,
  signal,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { PhoneNumberFormat, PhoneNumberUtil } from 'google-libphonenumber';
import { CountryData } from '../../models/country.model';
import { getCountriesList } from '../helpers/form.helper';

const phoneUtil = PhoneNumberUtil.getInstance();

@Component({
  selector: 'app-phone-input-ui',
  imports: [],
  templateUrl: './phone-input-ui.html',
  styleUrl: './phone-input-ui.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => PhoneInputUi),
      multi: true,
    },
  ],
})
export class PhoneInputUi implements ControlValueAccessor {
  phoneNumber = signal('');
  selectedCountry = signal('CI');
  disabled = signal(false);
  required = input.required<boolean>();
  showDropdown = signal(false);
  searchQuery = signal('');
  countries: CountryData[] = [];

  filteredCountries = computed(() => {
    const q = this.searchQuery().toLowerCase().trim();
    if (!q) return this.countries;
    return this.countries.filter(
      (c) =>
        c.name.toLowerCase().includes(q) || c.dial.includes(q) || c.code.toLowerCase().includes(q),
    );
  });

  constructor(private elementRef: ElementRef) {
    const lang = localStorage.getItem('chosenLanguage') || 'fr';
    this.countries = getCountriesList(lang);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.closeDropdown();
    }
  }

  onChange = (value: string) => {};
  onTouched = () => {};

  writeValue(value: string): void {
    if (value) {
      try {
        const parsed = phoneUtil.parseAndKeepRawInput(value);
        const regionCode = phoneUtil.getRegionCodeForNumber(parsed);
        if (regionCode) {
          this.selectedCountry.set(regionCode);
          this.phoneNumber.set(phoneUtil.format(parsed, PhoneNumberFormat.NATIONAL));
        }
      } catch (e) {
        this.phoneNumber.set(value);
      }
    } else {
      this.phoneNumber.set('');
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  toggleDropdown(): void {
    if (!this.disabled()) {
      this.showDropdown.update((v) => !v);
      if (!this.showDropdown()) {
        this.searchQuery.set('');
      }
    }
  }

  closeDropdown(): void {
    this.showDropdown.set(false);
    this.searchQuery.set('');
  }

  selectCountry(code: string): void {
    this.selectedCountry.set(code);
    this.closeDropdown();
    this.triggerChange();
  }

  onSearchInput(event: any): void {
    this.searchQuery.set(event.target.value);
  }

  getSelectedFlag(): string {
    return this.countries.find((c) => c.code === this.selectedCountry())?.flag ?? '';
  }

  onNumberInput(event: any): void {
    const digits = event.target.value.replace(/\D/g, '');
    event.target.value = digits;
    this.phoneNumber.set(digits);
    this.triggerChange();
  }

  private triggerChange(): void {
    const rawInput = this.phoneNumber();
    if (!rawInput) {
      this.onChange('');
      return;
    }

    try {
      const parsed = phoneUtil.parseAndKeepRawInput(rawInput, this.selectedCountry());
      if (phoneUtil.isValidNumberForRegion(parsed, this.selectedCountry())) {
        // On renvoie le format international au parent (+33...)
        const e164 = phoneUtil.format(parsed, PhoneNumberFormat.E164);
        this.onChange(e164);
      } else {
        // Si invalide, on peut renvoyer la valeur brute ou null
        this.onChange(rawInput);
      }
    } catch (e) {
      this.onChange(rawInput);
    }
  }

  getDialCode(): string {
    const country = this.countries.find((c) => c.code === this.selectedCountry());
    return country ? country.dial : '';
  }
}
