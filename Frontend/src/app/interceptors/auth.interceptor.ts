import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';

const PUBLIC_AUTH_PATHS = ['/auth/login', '/users/register'];

function isApiRequest(url: string): boolean {
  const normalizedApiUrl = environment.apiUrl.replace(/\/+$/, '');
  return url.startsWith(normalizedApiUrl) || url.startsWith('/api/');
}

function isPublicAuthEndpoint(url: string): boolean {
  return PUBLIC_AUTH_PATHS.some((path) => url.includes(path));
}

/**
 * Attaches the Orion JWT Bearer token to outgoing API requests and clears expired sessions on 401.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (!isApiRequest(req.url)) {
    return next(req);
  }

  const authService = inject(AuthService);
  const router = inject(Router);

  let outgoingReq = req;
  if (!req.headers.has('Authorization') && !isPublicAuthEndpoint(req.url)) {
    const token = authService.getToken();
    if (token) {
      outgoingReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
        },
      });
    }
  }

  return next(outgoingReq).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !isPublicAuthEndpoint(req.url)
      ) {
        authService.clearSession();
        if (!router.url.startsWith('/login')) {
          void router.navigate(['/login'], {
            queryParams: { returnUrl: router.url || '/' },
          });
        }
      }

      return throwError(() => error);
    }),
  );
};
