import { AsyncPipe, DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  ElementRef,
  forwardRef,
  HostListener,
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
import { Gender, SearchCitizenResponse } from '../../services/nswag/api-nswag-client';

@Component({
  selector: 'app-search-citizen-ui',
  imports: [ReactiveFormsModule, AsyncPipe, DatePipe, TranslatePipe],
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
  inputText = signal<string>('');
  isOpen = signal(false);
  isLoading = signal(false);
  activeIndex = signal(-1);

  @ViewChild('inputEl', { static: false })
  inputEl?: ElementRef<HTMLInputElement>;

  public id = input('searchCitizen');
  public gender = input.required<Gender>();

  onChange: (id: string) => void = () => {};
  onTouched: () => void = () => {};
  disabled = signal(false);
  control?: AbstractControl | null;

  ngOnInit(): void {
    const formControlName = this.elementRef.nativeElement.getAttribute('formcontrolname');
    this.control = this.controlContainer?.control?.get(formControlName) || null;

    this.search$
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        filter((c) => !!c),
        debounceTime(300),
        tap(() => this.isLoading.set(true)),
        switchMap((c) => this.citizenService.searchCitizen(c!,this.gender())),
        tap((citizens) => {
          this.citizens$.next(citizens);
          this.isLoading.set(false);
        }),
      )
      .subscribe();
  }

  private getExistingCitizen(id: string): void {
    if (!id) return;
    const existing = this.citizens$.value.find((c) => c.id === id);
    if (existing) {
      this.inputText.set(`${existing.firstName} ${existing.lastName}`);
      return;
    }
    this.isLoading.set(true);
    this.citizenService
      .searchCitizenById(id,this.gender())
      .pipe(
        tap((citizens) => {
          this.citizens$.next(citizens);
          this.isLoading.set(false);
          const citizen = citizens.find((c) => c.id === id);
          if (citizen) this.inputText.set(`${citizen.firstName} ${citizen.lastName}`);
        }),
      )
      .subscribe();
  }

  handleFilter(value: string): void {
    if (value.length >= 3) {
      this.search$.next(value);
    } else {
      this.citizens$.next([]);
      this.isLoading.set(false);
    }
  }

  onInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.inputText.set(value);
    if (this.citizenId()) {
      this.citizenId.set('');
      this.onChange('');
    }
    this.activeIndex.set(-1);
    this.isOpen.set(value.length > 0);
    this.handleFilter(value);
  }

  onFocus(): void {
    if (this.citizens$.value.length || this.inputText().length > 0) {
      this.isOpen.set(true);
    }
  }

  onKeydown(event: KeyboardEvent): void {
    const citizens = this.citizens$.value;
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        if (!this.isOpen()) {
          this.isOpen.set(true);
          return;
        }
        this.activeIndex.set(Math.min(this.activeIndex() + 1, citizens.length - 1));
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.activeIndex.set(Math.max(this.activeIndex() - 1, 0));
        break;
      case 'Enter':
        if (this.isOpen() && this.activeIndex() >= 0 && citizens[this.activeIndex()]) {
          event.preventDefault();
          this.selectCitizen(citizens[this.activeIndex()]);
        }
        break;
      case 'Escape':
        this.isOpen.set(false);
        this.activeIndex.set(-1);
        break;
    }
  }

  selectCitizen(citizen: SearchCitizenResponse): void {
    this.citizenId.set(citizen.id);
    this.inputText.set(`${citizen.firstName} ${citizen.lastName}`);
    this.onChange(citizen.id);
    this.onTouched();
    this.isOpen.set(false);
    this.activeIndex.set(-1);
  }

  clearSelection(): void {
    this.citizenId.set('');
    this.inputText.set('');
    this.citizens$.next([]);
    this.onChange('');
    this.onTouched();
    this.isOpen.set(false);
    this.activeIndex.set(-1);
    this.inputEl?.nativeElement.focus();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen.set(false);
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
