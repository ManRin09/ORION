import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { AdventurersComponent } from './adventurers.component';
import { UserService } from '../../services/user.service';
import { PagedResponse, UserResponse } from '../../models/user.models';

const mockUser: UserResponse = {
  id: '00000000-0000-0000-0000-000000000001',
  firstName: 'Arthur',
  middleName: 'Pendragon',
  email: 'arthur@orion.realm',
  username: 'arthur_pendragon',
  avatarUrl: null,
  authProvider: 'Local',
  role: 'User',
  planType: 'Free',
  totalXp: 120,
  currentLevel: 2,
  currentStreak: 3,
  longestStreak: 5,
  gems: 10,
  isActive: true,
  acceptedTermsAt: '2026-11-27T00:00:00Z',
  createdAt: '2026-11-27T00:00:00Z',
  updatedAt: '2026-11-27T00:00:00Z',
};

const mockPaged: PagedResponse<UserResponse> = {
  items: [mockUser],
  pageNumber: 1,
  pageSize: 8,
  totalCount: 1,
  totalPages: 1,
};

const mockUserService = {
  getPaged: () => of(mockPaged),
  getById: () => of(mockUser),
  updateProfile: () => of({ ...mockUser, firstName: 'Arthur II' }),
  softDelete: () => of(void 0),
};

describe('AdventurersComponent', () => {
  beforeEach(async () => {
    window.localStorage.clear();
    window.sessionStorage.clear();
    await TestBed.configureTestingModule({
      imports: [AdventurersComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: UserService, useValue: mockUserService },
      ],
    }).compileComponents();
  });

  it('should create the component and load adventurers roster', async () => {
    const fixture = TestBed.createComponent(AdventurersComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('ADVENTURERS ROSTER');
    expect(element.textContent).toContain('Arthur Pendragon');
    expect(element.textContent).toContain('@arthur_pendragon');
    expect(element.textContent).toContain('LVL 2');
  });

  it('should inspect an adventurer when VIEW button is clicked', async () => {
    const fixture = TestBed.createComponent(AdventurersComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const viewButton = Array.from(fixture.nativeElement.querySelectorAll('button')).find(
      (b: any) => b.textContent?.includes('[ VIEW ]'),
    ) as HTMLButtonElement;

    expect(viewButton).toBeTruthy();
    viewButton.click();
    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.textContent).toContain('INSPECT ADVENTURER ARCHIVE');
  });
});
