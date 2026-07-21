import { Component, ElementRef, HostListener, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { Language } from '../../../enums/language.enum';
import { LanguageService } from '../../../services/language.service';

@Component({
  selector: 'app-language-selector-ui',
  imports: [TranslatePipe],
  templateUrl: './language-selector-ui.html',
  styleUrl: './language-selector-ui.scss',
})
export class LanguageSelectorUi {
  private readonly languageService = inject(LanguageService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly langItems: { lang: Language; displayText: string }[] = [
    { lang: Language.fr, displayText: 'FR' },
    { lang: Language.en, displayText: 'EN' },
  ];

  readonly currentLang = toSignal(this.languageService.getCurrentLanguage(), {
    initialValue: Language.en,
  });
  readonly isOpen = signal(false);

  toggle(): void {
    this.isOpen.update((open) => !open);
  }

  selectLang(lang: Language): void {
    this.languageService.changeLanguage(lang);
    this.isOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.host.nativeElement.contains(event.target as Node)) {
      this.isOpen.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.isOpen.set(false);
  }
}
