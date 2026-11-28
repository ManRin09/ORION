import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Protects routes requiring an authenticated Orion adventurer session.
 */
export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  if (authService.isAuthenticated()) {
    return true;
  }

  const router = inject(Router);
  return router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url },
  });
};

/**
 * Protects administrative routes requiring an authenticated Admin session.
 */
export const adminGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login'], {
      queryParams: { returnUrl: state.url },
    });
  }

  if (authService.isAdmin()) {
    return true;
  }

  return router.createUrlTree(['/']);
};

/**
 * Redirects already-authenticated adventurers away from guest-only screens.
 */
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  if (!authService.isAuthenticated()) {
    return true;
  }

  const router = inject(Router);
  return router.createUrlTree(['/']);
};
