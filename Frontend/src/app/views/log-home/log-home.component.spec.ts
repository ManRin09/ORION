import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { environment } from '../../../environments/environment';
import { UserResponse } from '../../models/user.models';
import { AuthService } from '../../services/auth.service';
import { LogHomeComponent } from './log-home.component';

function createMockUser(overrides: Partial<UserResponse> = {}): UserResponse {
  return {
    id: '00000000-0000-0000-0000-000000000042',
    firstName: 'Elena',
    middleName: 'Vance',
    email: 'elena@orion.realm',
    username: 'elena_dev',
    avatarUrl: null,
    authProvider: 'Local',
    role: 'User',
    planType: 'Free',
    totalXp: 120,
    currentLevel: 2,
    currentStreak: 3,
    longestStreak: 7,
    gems: 45,
    isActive: true,
    acceptedTermsAt: '2026-11-27T00:00:00Z',
    createdAt: '2026-11-27T00:00:00Z',
    updatedAt: '2026-11-27T00:00:00Z',
    ...overrides,
  };
}

describe('LogHomeComponent', () => {
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    window.localStorage.clear();
    window.sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [LogHomeComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should hydrate real user stats from GET /api/auth/me without hardcoded mockup values', async () => {
    const fixture = TestBed.createComponent(LogHomeComponent);
    fixture.detectChanges();

    const req = httpTesting.expectOne(`${environment.apiUrl}/auth/me`);
    expect(req.request.method).toBe('GET');
    req.flush(createMockUser());

    await fixture.whenStable();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(text).toContain('3 DAYS');
    expect(text).toContain('45 GEMS');
    expect(text).toContain('LVL 02');
    expect(text).toContain('@elena_dev');

    // Ensure no static mockup data from LogHome.png leaked into the UI
    expect(text).not.toContain('14 DAYS');
    expect(text).not.toContain('420 GEMS');
    expect(text).not.toContain('@ManRin09');
    expect(text).not.toContain('72% COMPLETE');
    expect(text).not.toContain('18/25');
    expect(text).not.toContain('+180 XP');
    expect(text).not.toContain('0/32 QUESTS');
  });

  it('should render all 5 real courses with honest zero-quest progress', async () => {
    const fixture = TestBed.createComponent(LogHomeComponent);
    fixture.detectChanges();

    httpTesting.expectOne(`${environment.apiUrl}/auth/me`).flush(createMockUser());
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    const text = element.textContent ?? '';

    expect(text).toContain('CHOOSE YOUR QUEST');
    expect(text).toContain('[ PYTHON FUNDAMENTALS ]');
    expect(text).toContain('[ FULLSTACK APP DESIGN ]');
    expect(text).toContain('[ ASP.NET CORE ]');
    expect(text).toContain('[ NEXT.JS ]');
    expect(text).toContain('[ NESTJS ]');
    expect(text).toContain('0% COMPLETE (0 QUESTS AVAILABLE)');
    expect(text).toContain('READY TO EMBARK (0 QUESTS AVAILABLE)');
    expect(text).toContain('QUESTS COMING SOON');

    const progressBars = element.querySelectorAll('[role="progressbar"]');
    expect(progressBars.length).toBe(2);
    progressBars.forEach((bar) => {
      expect(bar.getAttribute('aria-valuenow')).toBe('0');
    });
  });

  it('should lock elective guilds below level 5 and unlock them dynamically at level 5+', async () => {
    const lockedFixture = TestBed.createComponent(LogHomeComponent);
    lockedFixture.detectChanges();
    httpTesting
      .expectOne(`${environment.apiUrl}/auth/me`)
      .flush(createMockUser({ currentLevel: 1 }));
    await lockedFixture.whenStable();

    const lockedText = (lockedFixture.nativeElement as HTMLElement).textContent ?? '';
    expect(lockedText).toContain('REQUIRES LEVEL 05 ADVENTURER PASS');
    expect(lockedText).toContain('REACH LVL 05');

    const unlockedFixture = TestBed.createComponent(LogHomeComponent);
    unlockedFixture.detectChanges();
    httpTesting
      .expectOne(`${environment.apiUrl}/auth/me`)
      .flush(createMockUser({ currentLevel: 6 }));
    await unlockedFixture.whenStable();

    const unlockedText = (unlockedFixture.nativeElement as HTMLElement).textContent ?? '';
    expect(unlockedText).toContain('LEVEL 05+ PASS ACTIVE');
    expect(unlockedText).toContain('STATUS: UNLOCKED · LEVEL 05+');
    expect(unlockedText).not.toContain('REQUIRES LEVEL 05 ADVENTURER PASS');
  });

  it('should toggle sidebar collapse and switch between CURSOS, PERFIL, and MARKET tabs', async () => {
    const fixture = TestBed.createComponent(LogHomeComponent);
    fixture.detectChanges();
    httpTesting.expectOne(`${environment.apiUrl}/auth/me`).flush(createMockUser());
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;

    const collapseBtn = Array.from(element.querySelectorAll('button')).find((b) =>
      b.textContent?.includes('COLLAPSE MENU'),
    );
    expect(collapseBtn).toBeTruthy();
    collapseBtn?.click();
    await fixture.whenStable();
    expect(element.textContent).toContain('[ ▸ ]');

    const expandBtn = Array.from(element.querySelectorAll('button')).find((b) =>
      b.textContent?.includes('[ ▸ ]'),
    );
    expandBtn?.click();
    await fixture.whenStable();
    expect(element.textContent).toContain('[ ◂ COLLAPSE MENU ]');

    const perfilBtn = Array.from(element.querySelectorAll('button')).find((b) =>
      b.textContent?.includes('[ PERFIL ]'),
    );
    perfilBtn?.click();
    await fixture.whenStable();

    expect(element.textContent).toContain('[ ADVENTURER PROFILE ]');
    expect(element.textContent).toContain('Elena Vance');
    expect(element.textContent).toContain('elena@orion.realm');
    expect(element.textContent).toContain('120 XP');
    expect(element.textContent).toContain('7 DAYS');

    const marketBtn = Array.from(element.querySelectorAll('button')).find((b) =>
      b.textContent?.includes('[ MARKET ]'),
    );
    marketBtn?.click();
    await fixture.whenStable();

    expect(element.textContent).toContain('[ GUILD MARKET ]');
    expect(element.textContent).toContain('BALANCE: 45 GEMS');
    expect(element.textContent).toContain('MARKET INVENTORY COMING SOON');
  });

  it('should open and close the syllabus preview modal with an honest no-quests notice', async () => {
    const fixture = TestBed.createComponent(LogHomeComponent);
    fixture.detectChanges();
    httpTesting.expectOne(`${environment.apiUrl}/auth/me`).flush(createMockUser());
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    const previewBtn = Array.from(element.querySelectorAll('button')).find((b) =>
      b.textContent?.includes('PREVIEW SYLLABUS'),
    );
    expect(previewBtn).toBeTruthy();
    previewBtn?.click();
    await fixture.whenStable();

    const dialog = element.querySelector('[role="dialog"]');
    expect(dialog).not.toBeNull();
    expect(dialog?.textContent).toContain('[ ASP.NET CORE ]');
    expect(dialog?.textContent).toContain('NO QUESTS AVAILABLE YET');

    const closeBtn = Array.from(element.querySelectorAll('button')).find((b) =>
      b.textContent?.includes('[ CLOSE ]'),
    );
    closeBtn?.click();
    await fixture.whenStable();

    expect(element.querySelector('[role="dialog"]')).toBeNull();
  });

  it('should call DELETE /api/auth/session and navigate to /login when clicking LOG OUT', async () => {
    window.localStorage.setItem(
      'orion.auth.session',
      JSON.stringify({
        accessToken: 'jwt-valid-token',
        expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
        rememberTerminal: true,
        user: createMockUser(),
      }),
    );

    const authService = TestBed.inject(AuthService);
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    const fixture = TestBed.createComponent(LogHomeComponent);
    fixture.detectChanges();
    httpTesting.expectOne(`${environment.apiUrl}/auth/me`).flush(createMockUser());
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    const logoutBtn = Array.from(element.querySelectorAll('button')).find((b) =>
      b.textContent?.includes('[ LOG OUT ]'),
    );
    expect(logoutBtn).toBeTruthy();
    logoutBtn?.click();

    const logoutReq = httpTesting.expectOne(`${environment.apiUrl}/auth/session`);
    expect(logoutReq.request.method).toBe('DELETE');
    logoutReq.flush(null, { status: 204, statusText: 'No Content' });
    await fixture.whenStable();

    expect(authService.isAuthenticated()).toBe(false);
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });
});
