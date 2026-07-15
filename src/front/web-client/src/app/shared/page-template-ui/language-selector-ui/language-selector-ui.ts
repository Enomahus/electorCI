import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { Language } from '../../../enums/language.enum';
import { LanguageService } from '../../../services/language.service';

@Component({
  selector: 'app-language-selector-ui',
  imports: [TranslatePipe, AsyncPipe],
  templateUrl: './language-selector-ui.html',
  styleUrl: './language-selector-ui.scss',
})
export class LanguageSelectorUi {
  private readonly languageService = inject(LanguageService);
  langItems: { lang: Language; displayText: string }[];
  currentLang$: Observable<Language>;

  constructor() {
    this.langItems = [
      { lang: Language.fr, displayText: 'FR' },
      { lang: Language.en, displayText: 'EN' },
    ];
    this.currentLang$ = this.languageService.getCurrentLanguage();
  }

  langChange(lang: Language): void {
    this.languageService.changeLanguage(lang);
  }
}
