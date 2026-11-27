import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { environment } from '../../environments/environment';
import {
  AuthResponse,
  ChangePasswordRequest,
  LoginCredentials,
  RegisterCharacterPayload,
} from '../models/auth.models';
import {
  PagedResponse,
  RegisterUserRequest,
  UpdateUserProfileRequest,
  UserResponse,
} from '../models/user.models';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpTesting: HttpTestingController;
  const authUrl = `${environment.apiUrl}/auth`;
  const usersUrl = `${environment.apiUrl}/users`;

  const mockUser: UserResponse = {
    id: 'f81d4fae-7dec-11d0-a765-00a0c91e6bf6',
    firstName: 'Arthur',
    middleName: 'Pendragon',
    email: 'arthur@orion.realm',
    username: 'arthur_pendragon',
    avatarUrl: null,
    authProvider: 'Local',
    role: 'User',
    planType: 'Free',
    totalXp: 100,
    currentLevel: 2,
    currentStreak: 3,
    longestStreak: 5,
    gems: 15,
    isActive: true,
    acceptedTermsAt: '2026-11-27T00:00:00Z',
    createdAt: '2026-11-27T00:00:00Z',
    updatedAt: '2026-11-27T00:00:00Z',
  };

  const mockAuthResponse: AuthResponse = {
    accessToken: 'eyJhbGciOiJIUzI1NiJ9.payload.sig',
    tokenType: 'Bearer',
    expiresAtUtc: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString(),
    rememberTerminal: true,
    user: mockUser,
  };

  beforeEach(() => {
    window.localStorage.clear();
    window.sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [AuthService, provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(AuthService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should authenticate via POST /api/auth/login and persist session in localStorage when rememberTerminal is true', () => {
    const credentials: LoginCredentials = {
      identifier: 'arthur@orion.realm',
      password: 'Orion!Spell2026',
      rememberTerminal: true,
    };

    let result: AuthResponse | undefined;
    service.login(credentials).subscribe((res) => (result = res));

    const req = httpTesting.expectOne(`${authUrl}/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(credentials);
    req.flush(mockAuthResponse);

    expect(result).toEqual(mockAuthResponse);
    expect(service.isAuthenticated()).toBe(true);
    expect(service.getToken()).toBe(mockAuthResponse.accessToken);
    expect(service.currentUser()).toEqual(mockUser);
    expect(window.localStorage.getItem('orion.auth.session')).toContain(
      mockAuthResponse.accessToken,
    );
  });

  it('should fetch authenticated profile via GET /api/auth/me', () => {
    let result: UserResponse | undefined;
    service.getCurrentUser().subscribe((res) => (result = res));

    const req = httpTesting.expectOne(`${authUrl}/me`);
    expect(req.request.method).toBe('GET');
    req.flush(mockUser);

    expect(result).toEqual(mockUser);
    expect(service.currentUser()).toEqual(mockUser);
  });

  it('should update password via PUT /api/auth/password', () => {
    const payload: ChangePasswordRequest = {
      currentPassword: 'Old!Password1',
      newPassword: 'New!Password2',
      confirmNewPassword: 'New!Password2',
    };

    let completed = false;
    service.changePassword(payload).subscribe(() => (completed = true));

    const req = httpTesting.expectOne(`${authUrl}/password`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(payload);
    req.flush(null, { status: 204, statusText: 'No Content' });

    expect(completed).toBe(true);
  });

  it('should end session via DELETE /api/auth/session and clear storage', () => {
    service
      .login({ identifier: 'arthur', password: 'Orion!Spell2026', rememberTerminal: false })
      .subscribe();
    httpTesting
      .expectOne(`${authUrl}/login`)
      .flush({ ...mockAuthResponse, rememberTerminal: false });

    expect(service.isAuthenticated()).toBe(true);

    let completed = false;
    service.logout().subscribe(() => (completed = true));

    const req = httpTesting.expectOne(`${authUrl}/session`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null, { status: 204, statusText: 'No Content' });

    expect(completed).toBe(true);
    expect(service.isAuthenticated()).toBe(false);
    expect(service.getToken()).toBeNull();
    expect(window.sessionStorage.getItem('orion.auth.session')).toBeNull();
  });

  it('should complete external OAuth login by saving token and calling GET /api/auth/me', () => {
    let profile: UserResponse | undefined;
    service.completeExternalLogin('external-jwt-token').subscribe((u) => (profile = u));

    const req = httpTesting.expectOne(`${authUrl}/me`);
    expect(req.request.method).toBe('GET');
    req.flush(mockUser);

    expect(profile).toEqual(mockUser);
    expect(service.getToken()).toBe('external-jwt-token');
    expect(service.isAuthenticated()).toBe(true);
  });

  it('should register a new adventurer via POST /api/users/register', () => {
    const payload: RegisterUserRequest = {
      firstName: 'Arthur',
      middleName: 'Pendragon',
      email: 'arthur@orion.realm',
      username: 'arthur_pendragon',
      password: 'Orion!Spell2026',
      confirmPassword: 'Orion!Spell2026',
      acceptTerms: true,
    };

    let result: UserResponse | undefined;
    service.register(payload).subscribe((res) => (result = res));

    const req = httpTesting.expectOne(`${usersUrl}/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush(mockUser);

    expect(result).toEqual(mockUser);
  });

  it('should map RegisterCharacterPayload and call POST /api/users/register', () => {
    const charPayload: RegisterCharacterPayload = {
      firstName: 'Arthur',
      middleTitle: 'High King',
      email: 'arthur@orion.realm',
      password: 'Orion!Spell2026',
      acceptedTerms: true,
    };

    service.registerFromCharacterPayload(charPayload).subscribe();

    const req = httpTesting.expectOne(`${usersUrl}/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      firstName: 'Arthur',
      middleName: 'High King',
      email: 'arthur@orion.realm',
      username: null,
      password: 'Orion!Spell2026',
      confirmPassword: 'Orion!Spell2026',
      acceptTerms: true,
    });
    req.flush(mockUser);
  });

  it('should get adventurer by ID via GET /api/users/{id}', () => {
    let result: UserResponse | undefined;
    service.getById(mockUser.id).subscribe((res) => (result = res));

    const req = httpTesting.expectOne(`${usersUrl}/${mockUser.id}`);
    expect(req.request.method).toBe('GET');
    req.flush(mockUser);

    expect(result).toEqual(mockUser);
  });

  it('should get paginated adventurers via GET /api/users', () => {
    const mockPaged: PagedResponse<UserResponse> = {
      items: [mockUser],
      pageNumber: 1,
      pageSize: 10,
      totalCount: 1,
      totalPages: 1,
    };

    let result: PagedResponse<UserResponse> | undefined;
    service.getPaged(1, 10).subscribe((res) => (result = res));

    const req = httpTesting.expectOne(`${usersUrl}?pageNumber=1&pageSize=10`);
    expect(req.request.method).toBe('GET');
    req.flush(mockPaged);

    expect(result).toEqual(mockPaged);
  });

  it('should update adventurer profile via PUT /api/users/{id}/profile', () => {
    const updateRequest: UpdateUserProfileRequest = {
      firstName: 'Arthur',
      middleName: 'Emperor',
      username: 'arthur_king',
      avatarUrl: 'https://orion.realm/avatars/arthur.png',
    };
    const updatedUser: UserResponse = {
      ...mockUser,
      firstName: 'Arthur',
      middleName: 'Emperor',
      username: 'arthur_king',
      avatarUrl: 'https://orion.realm/avatars/arthur.png',
    };

    let result: UserResponse | undefined;
    service.updateProfile(mockUser.id, updateRequest).subscribe((res) => (result = res));

    const req = httpTesting.expectOne(`${usersUrl}/${mockUser.id}/profile`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updateRequest);
    req.flush(updatedUser);

    expect(result).toEqual(updatedUser);
  });

  it('should soft delete adventurer via DELETE /api/users/{id}', () => {
    let completed = false;
    service.softDelete(mockUser.id).subscribe(() => (completed = true));

    const req = httpTesting.expectOne(`${usersUrl}/${mockUser.id}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null, { status: 204, statusText: 'No Content' });

    expect(completed).toBe(true);
  });
});
