import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { environment } from '../../../environments/environment';
import { UserResponse } from '../../models/user.models';
import { AuthService } from '../../services/auth.service';
import { AuthCallbackComponent } from './auth-callback.component';

const mockGitHubUser: UserResponse = {
  id: '00000000-0000-0000-0000-000000000099',
  firstName: 'Ada',
  middleName: 'Lovelace',
  email: 'ada@orion.realm',
  username: 'ada_lovelace',
  avatarUrl: 'https://avatars.githubusercontent.com/u/1?v=4',
  authProvider: 'GitHub',
  role: 'User',
  planType: 'Free',
  totalXp: 0,
  currentLevel: 1,
  currentStreak: 0,
  longestStreak: 0,
  gems: 0,
  isActive: true,
  acceptedTermsAt: '2026-11-27T00:00:00Z',
  createdAt: '2026-11-27T00:00:00Z',
  updatedAt: '2026-11-27T00:00:00Z',
};

describe('AuthCallbackComponent', () => {
  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();
  });

  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('should persist token from query param, fetch GET /api/auth/me, and navigate to /home', async () => {
    await TestBed.configureTestingModule({
      imports: [AuthCallbackComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParamMap: convertToParamMap({ token: 'github-jwt-token' }) },
          },
        },
      ],
    }).compileComponents();

    const httpTesting = TestBed.inject(HttpTestingController);
    const router = TestBed.inject(Router);
    const authService = TestBed.inject(AuthService);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const fixture = TestBed.createComponent(AuthCallbackComponent);
    fixture.detectChanges();

    const req = httpTesting.expectOne(`${environment.apiUrl}/auth/me`);
    expect(req.request.method).toBe('GET');
    req.flush(mockGitHubUser);
    httpTesting.verify();

    expect(authService.isAuthenticated()).toBe(true);
    expect(authService.getToken()).toBe('github-jwt-token');
    expect(authService.currentUser()).toEqual(mockGitHubUser);
    expect(navigateSpy).toHaveBeenCalledWith(['/home']);
  });

  it('should show error alert when callback contains error query param', async () => {
    await TestBed.configureTestingModule({
      imports: [AuthCallbackComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: { queryParamMap: convertToParamMap({ error: 'invalid_state' }) },
          },
        },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(AuthCallbackComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('invalid_state');
    expect(text).toContain('[ RETURN TO LOGIN ]');
  });
});
