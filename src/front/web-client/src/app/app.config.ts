import { registerLocaleData } from '@angular/common';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import localeFr from '@angular/common/locales/fr';
import { ApplicationConfig, LOCALE_ID, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, TitleStrategy, withComponentInputBinding } from '@angular/router';
import { provideToastr } from 'ngx-toastr';
import { routes } from './app.routes';
import { provideTranslations } from './config/provideTranslations';
import { CustomTitleStrategy } from './core/custom-title-strategy';
import { authInterceptor } from './services/auth/interceptors/auth.interceptor';
import { langInterceptor } from './services/auth/interceptors/lang-interceptor';
import { ConfigService } from './services/config.service';
import { APP_BASE_URL } from './services/nswag/api-nswag-client';
registerLocaleData(localeFr);

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptors([authInterceptor, langInterceptor])),
    provideRouter(routes, withComponentInputBinding()),
    {
      provide: APP_BASE_URL,
      useFactory: (configService: ConfigService) => configService.getConfig().apiUrl,
      deps: [ConfigService],
    },
    provideTranslations(),
    provideToastr(),
    { provide: LOCALE_ID, useValue: 'fr-FR' },
    { provide: TitleStrategy, useClass: CustomTitleStrategy },
  ],
};
