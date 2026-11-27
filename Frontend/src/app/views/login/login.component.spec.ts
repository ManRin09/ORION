import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../../models/auth.models';
import { LoginComponent } from './login.component';

const mockAuthResponse: AuthResponse = {
  accessToken: 'jwt-test-token-123',
  tokenType: 'Bearer',
  expiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
  rememberTerminal: true,
  user: {
    id: '00000000-0000-0000-0000-000000000001',
    firstName: 'Warrior',
    middleName: null,
    email: 'warrior_dev@orion.realm',
    username: 'warrior_dev',
    avatarUrl: null,
    authProvider: 'Local',
    role: 'User',
    planType: 'Free',
    totalXp: 50,
    currentLevel: 1,
    currentStreak: 14,
    longestStreak: 14,
    gems: 5,
    isActive: true,
    acceptedTermsAt: '2026-11-27T00:00:00Z',
    createdAt: '2026-11-27T00:00:00Z',
    updatedAt: '2026-11-27T00:00:00Z',
  },
};

describe('LoginComponent', () => {
  let httpTesting: HttpTestingController;

  beforeEach(async () => {
    window.localStorage.clear();
    window.sessionStorage.clear();

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('should render the login window, Pyxis familiar guide, navbar, and footer', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const text = element.textContent ?? '';

    expect(element.querySelector('app-navbar')).not.toBeNull();
    expect(element.querySelector('app-login-card')).not.toBeNull();
    expect(element.querySelector('app-familiar-guide-card')).not.toBeNull();
    expect(element.querySelector('app-footer')).not.toBeNull();

    expect(text).toContain('[ SLOT 01: LOAD GAME ]');
    expect(text).toContain('SIGN IN TO ORION');
    expect(text).toContain('ADVENTURER NAME OR EMAIL');
    expect(text).toContain('PASSWORD / SECRET SPELL');
    expect(text).toContain('Remember on this magic terminal');
    expect(text).toContain('[ RESUME ADVENTURE ]');
    expect(text).toContain('OR EXTERNAL ARCHIVE');
    expect(text).toContain('[ GITHUB GUILD ]');
    expect(text).toContain('[ CREATE NEW CHARACTER ]');
    expect(text).toContain('PYXIS THE FAMILIAR:');
    expect(text).toContain('Your 14-day streak is safely stored');
  });

  it('should toggle password visibility between [ SHOW ] and [ HIDE ]', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const passwordInput = element.querySelector('#login-password') as HTMLInputElement;
    const toggleBtn = Array.from(element.querySelectorAll('button')).find((b) =>
      b.textContent?.includes('[ SHOW ]'),
    );

    expect(passwordInput.type).toBe('password');
    expect(toggleBtn).toBeTruthy();

    toggleBtn?.click();
    await fixture.whenStable();

    expect(passwordInput.type).toBe('text');
    expect(toggleBtn?.textContent).toContain('[ HIDE ]');
  });

  it('should display validation feedback when submitting an empty form', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const form = element.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    expect(element.textContent).toContain('Enter your adventurer name or email');
    expect(element.textContent).toContain('Both your Adventurer Name/Email');
  });

  it('should POST credentials to /api/auth/login and unlock Slot 01 on success', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const idInput = element.querySelector('#login-identifier') as HTMLInputElement;
    const passInput = element.querySelector('#login-password') as HTMLInputElement;

    idInput.value = 'warrior_dev@orion.realm';
    idInput.dispatchEvent(new Event('input'));

    passInput.value = 'orion_spell_99';
    passInput.dispatchEvent(new Event('input'));

    const form = element.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));

    const req = httpTesting.expectOne(`${environment.apiUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({
      identifier: 'warrior_dev@orion.realm',
      password: 'orion_spell_99',
      rememberTerminal: true,
    });
    req.flush(mockAuthResponse);

    await fixture.whenStable();

    expect(element.textContent).toContain('Slot 01 unlocked for warrior_dev@orion.realm');
    expect(element.textContent).toContain('@warrior_dev');
  });

  it('should show ProblemDetails warning in Pyxis when login fails with 401', async () => {
    const fixture = TestBed.createComponent(LoginComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const idInput = element.querySelector('#login-identifier') as HTMLInputElement;
    const passInput = element.querySelector('#login-password') as HTMLInputElement;

    idInput.value = 'warrior_dev@orion.realm';
    idInput.dispatchEvent(new Event('input'));

    passInput.value = 'wrong_spell_99';
    passInput.dispatchEvent(new Event('input'));

    const form = element.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));

    const req = httpTesting.expectOne(`${environment.apiUrl}/auth/login`);
    req.flush(
      { title: 'Unauthorized', detail: 'Invalid adventurer credentials.' },
      { status: 401, statusText: 'Unauthorized' },
    );

    await fixture.whenStable();

    expect(element.textContent).toContain('Invalid adventurer credentials.');
  });
});
