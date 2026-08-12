import { AsyncPipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  ElementRef,
  forwardRef,
  inject,
  input,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  ControlContainer,
  ControlValueAccessor,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule,
} from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';
import { BehaviorSubject, debounceTime, filter, switchMap, tap } from 'rxjs';
import { CitizensApiService } from '../../services/api/citizens.api.service';
import { SearchCitizenResponse } from '../../services/nswag/api-nswag-client';

@Component({
  selector: 'app-search-citizen-ui',
  imports: [ReactiveFormsModule, AsyncPipe, TranslatePipe],
  templateUrl: './search-citizen-ui.html',
  styleUrl: './search-citizen-ui.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => SearchCitizenUi),
      multi: true,
    },
  ],
})
export class SearchCitizenUi implements OnInit, ControlValueAccessor {
  private readonly citizenService = inject(CitizensApiService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly controlContainer = inject(ControlContainer);
  private readonly elementRef = inject(ElementRef);
  private readonly search$ = new BehaviorSubject<string | undefined>(undefined);
  citizens$ = new BehaviorSubject<SearchCitizenResponse[]>([]);
  citizenId = signal<string>('');

  @ViewChild('autocomplete', { static: false })
  public id = input('searchCitizen');

  onChange: (id: string) => void = () => {};
  onTouched: () => void = () => {};
  disabled = signal(false);
  control?: AbstractControl | null;

  ngOnInit(): void {
    const formControlName = this.elementRef.nativeElement.el.getAttribute('formcontrolname');
    this.control = this.controlContainer?.control?.get(formControlName) || null;

    this.search$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter((c) => !!c),
        debounceTime(300),
        switchMap((c) => this.citizenService.searchCitizen(c!)),
        tap((citizens) => this.citizens$.next(citizens)),
      )
      .subscribe();
  }

  private getExistingCitizen(id: string): void {
    if (!id) return;
    if (!this.citizens$.value.some((c) => c.id === id)) {
      this.citizenService
        .searchCitizenById(id)
        .pipe(tap((c) => this.citizens$.next(c)))
        .subscribe();
    }
  }

  handleFilter(value: string): void {
    if (value.length >= 3) {
      this.search$.next(value);
    } else {
      //this.autocomplete.toggle(false);
      this.citizens$.next([]);
    }
  }

  writeValue(citizenId: string): void {
    this.getExistingCitizen(citizenId);
    this.citizenId.set(citizenId);
  }

  registerOnChange(onChange: (id: string) => void): void {
    this.onChange = onChange;
  }

  registerOnTouched(onTouched: () => void): void {
    this.onTouched = onTouched;
  }
  setDisabledState?(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }
}
