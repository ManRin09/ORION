import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { environment } from '../../environments/environment';
import { AuthService } from '../services/auth.service';
import { authInterceptor } from './auth.interceptor';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpTesting: HttpTestingController;
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });

    http = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should attach Authorization Bearer header to Orion API requests when authenticated', () => {
    authService.completeExternalLogin('valid-bearer-token').subscribe();
    const meReq = httpTesting.expectOne(`${environment.apiUrl}/auth/me`);
    expect(meReq.request.headers.get('Authorization')).toBe('Bearer valid-bearer-token');
    meReq.flush({ id: '1', username: 'arthur' });
  });

  it('should not attach Authorization header to /auth/login or external non-API URLs', () => {
    window.localStorage.setItem(
      'orion.auth.session',
      JSON.stringify({
        accessToken: 'existing-token',
        expiresAtUtc: null,
        rememberTerminal: true,
        user: null,
      }),
    );

    http.post(`${environment.apiUrl}/auth/login`, {}).subscribe();
    const loginReq = httpTesting.expectOne(`${environment.apiUrl}/auth/login`);
    expect(loginReq.request.headers.has('Authorization')).toBe(false);
    loginReq.flush({});

    http.get('https://api.github.com/zen').subscribe();
    const extReq = httpTesting.expectOne('https://api.github.com/zen');
    expect(extReq.request.headers.has('Authorization')).toBe(false);
    extReq.flush('ok');
  });

  it('should clear session and navigate to /login when an authenticated API call returns 401', () => {
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    authService.completeExternalLogin('expired-bearer-token').subscribe({
      error: () => {
        // expected 401
      },
    });

    const req = httpTesting.expectOne(`${environment.apiUrl}/auth/me`);
    req.flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });

    expect(authService.isAuthenticated()).toBe(false);
    expect(authService.getToken()).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/' } });
  });
});
