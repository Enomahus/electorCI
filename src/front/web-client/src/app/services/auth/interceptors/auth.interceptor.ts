import { HttpEvent, HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, switchMap } from 'rxjs';
import { APP_BASE_URL } from '../../nswag/api-nswag-client';
import { AuthService } from '../auth.service';

/**
 * Endpoints appelés sans jeton : ils servent justement à l'obtenir.
 * Il ne faut pas injecter AuthService pour ces requêtes, sinon on crée une
 * dépendance circulaire (AuthService -> ServerClient -> HttpClient -> intercepteur).
 */
const anonymousPaths = ['/auth', '/user/register'];

const isAnonymous = (url: string): boolean => anonymousPaths.some((path) => url.includes(path));

export function authInterceptor(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  const baseApiUrl = inject(APP_BASE_URL);
  if (!req.url.startsWith(baseApiUrl)) {
    return next(req);
  }

  let pathName = req.url;
  if(req.url.startsWith('http')) {
    const url = new URL(req.url);
    pathName = url.pathname;
  }

  if(pathName.startsWith('/auth') || pathName.startsWith('/user/register')) {
    return next(req);
  }

  return inject(AuthService)
    .getAccessToken()
    .pipe(
      switchMap((authToken) => {
        if (authToken) {
          const headers = req.headers.set('Authorization', `Bearer ${authToken}`);
          req = req.clone({
            headers,
          });
        }
        return next(req);
      }),
    );
}
