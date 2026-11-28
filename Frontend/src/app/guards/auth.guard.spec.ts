import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  provideRouter,
  RouterStateSnapshot,
  UrlTree,
} from '@angular/router';
import { beforeEach, describe, expect, it } from 'vitest';
import { AuthService } from '../services/auth.service';
import { adminGuard, authGuard, guestGuard } from './auth.guard';

describe('authGuard & adminGuard', () => {
  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
  });

  it('should redirect unauthenticated users to /login with returnUrl', () => {
    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/adventurers' } as RouterStateSnapshot),
    );

    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/login?returnUrl=%2Fadventurers');
  });

  it('should allow authenticated users to proceed through authGuard', () => {
    window.localStorage.setItem(
      'orion.auth.session',
      JSON.stringify({
        accessToken: 'jwt-token',
        expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
        rememberTerminal: true,
        user: null,
      }),
    );

    TestBed.inject(AuthService);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, { url: '/adventurers' } as RouterStateSnapshot),
    );

    expect(result).toBe(true);
  });

  it('should block non-Admin authenticated users in adminGuard and redirect to /', () => {
    window.localStorage.setItem(
      'orion.auth.session',
      JSON.stringify({
        accessToken: 'jwt-token',
        expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
        rememberTerminal: true,
        user: { role: 'User' },
      }),
    );

    TestBed.inject(AuthService);

    const result = TestBed.runInInjectionContext(() =>
      adminGuard({} as ActivatedRouteSnapshot, { url: '/adventurers' } as RouterStateSnapshot),
    );

    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/');
  });

  it('should allow authenticated Admin users to proceed through adminGuard', () => {
    window.localStorage.setItem(
      'orion.auth.session',
      JSON.stringify({
        accessToken: 'jwt-token',
        expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
        rememberTerminal: true,
        user: { role: 'Admin' },
      }),
    );

    TestBed.inject(AuthService);

    const result = TestBed.runInInjectionContext(() =>
      adminGuard({} as ActivatedRouteSnapshot, { url: '/adventurers' } as RouterStateSnapshot),
    );

    expect(result).toBe(true);
  });

  it('should redirect authenticated users from guestGuard to /home', () => {
    window.localStorage.setItem(
      'orion.auth.session',
      JSON.stringify({
        accessToken: 'jwt-token',
        expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
        rememberTerminal: true,
        user: null,
      }),
    );

    TestBed.inject(AuthService);

    const result = TestBed.runInInjectionContext(() =>
      guestGuard({} as ActivatedRouteSnapshot, { url: '/login' } as RouterStateSnapshot),
    );

    expect(result).toBeInstanceOf(UrlTree);
    expect((result as UrlTree).toString()).toBe('/home');
  });
});
