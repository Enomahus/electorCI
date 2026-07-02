import { AsyncPipe } from '@angular/common';
import { Component, inject, Renderer2, signal, ViewContainerRef } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { map, Observable } from 'rxjs';
import { ThemeService } from './services/theme.service';
import { LoaderUi } from './shared/loader/loader';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, LoaderUi, AsyncPipe],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private readonly translate = inject(TranslateService);
  private readonly themeService = inject(ThemeService);
  private readonly renderer = inject(Renderer2);
  private readonly verwRef = inject(ViewContainerRef);

  protected readonly title = signal('web-client');
  currentTheme?: string;
  translationsLoaded$: Observable<boolean>;

  constructor() {
    this.translate.addLangs(['fr']);
    this.translate.use('fr');
    this.translationsLoaded$ = this.translate.use('fr').pipe(map(() => true));

    this.themeService.currentTheme$.subscribe((theme) => {
      if (this.currentTheme) {
        this.renderer.removeClass(document.body, this.currentTheme);
      }
      this.currentTheme = theme.toString();
      this.renderer.addClass(document.body, this.currentTheme);
    });
  }
}
