import { Component, forwardRef, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-input-datepicker-ui',
  imports: [MatFormFieldModule, MatInputModule, MatDatepickerModule],
  templateUrl: './input-datepicker-ui.html',
  styleUrl: './input-datepicker-ui.scss',
  providers: [
    provideNativeDateAdapter(),
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputDatepickerUi),
      multi: true,
    },
  ],
})
export class InputDatepickerUi implements ControlValueAccessor {
  readonly value = signal<string>('');
  readonly id = signal<string>('');
  readonly disabled = signal<boolean>(false);

  onChange: (value: Date | null) => void = () => {};
  onTouched: () => void = () => {};

  writeValue(value: Date | string): void {
    // this.value.set(value || '');
    if (!value) {
      this.value.set('');
      return;
    }

    if (value instanceof Date) {
      if (!isNaN(value.getTime())) {
        this.value.set(value.toISOString().split('T')[0]);
      } else {
        this.value.set('');
      }
    } else if (typeof value === 'string') {
      this.value.set(value.split('T')[0]);
    } else {
      this.value.set('');
    }
  }

  registerOnChange(fn: (value: Date | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  // Méthode appelée à chaque saisie de l'utilisateur
  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.value.set(input.value);
    const newDate = input.value ? new Date(input.value) : null;
    this.onChange(newDate);
  }
}
