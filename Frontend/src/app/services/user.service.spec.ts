import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { UserService } from './user.service';
import { environment } from '../../environments/environment';
import {
  PagedResponse,
  RegisterUserRequest,
  UpdateUserProfileRequest,
  UserResponse,
} from '../models/user.models';
import { RegisterCharacterPayload } from '../models/auth.models';

describe('UserService', () => {
  let service: UserService;
  let httpTesting: HttpTestingController;
  const baseUrl = `${environment.apiUrl}/users`;

  const mockUser: UserResponse = {
    id: 'f81d4fae-7dec-11d0-a765-00a0c91e6bf6',
    firstName: 'Arthur',
    middleName: 'Pendragon',
    email: 'arthur.pendragon@orion.realm',
    username: 'arthur_pendragon',
    avatarUrl: null,
    authProvider: 'Local',
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

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [UserService, provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(UserService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should register a new adventurer via POST /api/users/register', () => {
    const payload: RegisterUserRequest = {
      firstName: 'Arthur',
      middleName: 'Pendragon',
      email: 'arthur.pendragon@orion.realm',
      username: 'arthur_pendragon',
      password: 'Orion!Spell2026',
      confirmPassword: 'Orion!Spell2026',
      acceptTerms: true,
    };

    let result: UserResponse | undefined;
    service.register(payload).subscribe((res) => {
      result = res;
    });

    const req = httpTesting.expectOne(`${baseUrl}/register`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);

    req.flush(mockUser);
    expect(result).toEqual(mockUser);
  });

  it('should map RegisterCharacterPayload and call register endpoint', () => {
    const charPayload: RegisterCharacterPayload = {
      firstName: 'Arthur',
      middleTitle: 'High King',
      email: 'arthur@orion.realm',
      password: 'Orion!Spell2026',
      acceptedTerms: true,
    };

    let result: UserResponse | undefined;
    service.registerFromCharacterPayload(charPayload).subscribe((res) => {
      result = res;
    });

    const req = httpTesting.expectOne(`${baseUrl}/register`);
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
    expect(result).toEqual(mockUser);
  });

  it('should get an adventurer by ID via GET /api/users/{id}', () => {
    const userId = mockUser.id;
    let result: UserResponse | undefined;

    service.getById(userId).subscribe((res) => {
      result = res;
    });

    const req = httpTesting.expectOne(`${baseUrl}/${userId}`);
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
    service.getPaged(1, 10).subscribe((res) => {
      result = res;
    });

    const req = httpTesting.expectOne(`${baseUrl}?pageNumber=1&pageSize=10`);
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
      middleName: 'Emperor',
      username: 'arthur_king',
      avatarUrl: 'https://orion.realm/avatars/arthur.png',
    };

    let result: UserResponse | undefined;
    service.updateProfile(mockUser.id, updateRequest).subscribe((res) => {
      result = res;
    });

    const req = httpTesting.expectOne(`${baseUrl}/${mockUser.id}/profile`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updateRequest);

    req.flush(updatedUser);
    expect(result).toEqual(updatedUser);
  });

  it('should soft delete adventurer via DELETE /api/users/{id}', () => {
    let completed = false;
    service.softDelete(mockUser.id).subscribe(() => {
      completed = true;
    });

    const req = httpTesting.expectOne(`${baseUrl}/${mockUser.id}`);
    expect(req.request.method).toBe('DELETE');

    req.flush(null, { status: 204, statusText: 'No Content' });
    expect(completed).toBe(true);
  });
});
