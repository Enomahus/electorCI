import { inject, Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { TitleStrategy } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { take } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CustomTitleStrategy extends TitleStrategy {
  private readonly title = inject(Title);
  private readonly translateService = inject(TranslateService);

  constructor() {
    super();
  }

  override updateTitle(snapshot: import('@angular/router').RouterStateSnapshot): void {
    const routeTitle = this.buildTitle(snapshot);
    if (routeTitle) {
      this.translateService
        .get(routeTitle)
        .pipe(take(1))
        .subscribe((translatedTitle) => {
          const fullTitle = this.translateService.instant('global.titleTemplate', {
            title: translatedTitle,
          });
          this.title.setTitle(fullTitle);
        });
    } else {
      const fallbackTitle = this.translateService.instant('global.noTitleTemplate');
      this.title.setTitle(fallbackTitle);
    }
  }
}
