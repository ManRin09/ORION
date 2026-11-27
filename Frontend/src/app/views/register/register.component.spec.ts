import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { RegisterComponent } from './register.component';
import { UserService } from '../../services/user.service';
import { UserResponse } from '../../models/user.models';

const mockUser: UserResponse = {
  id: '00000000-0000-0000-0000-000000000001',
  firstName: 'Arthur',
  middleName: null,
  email: 'arthur@orion.realm',
  username: 'arthur',
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

const mockUserService = {
  register: () => of(mockUser),
};

describe('RegisterComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        { provide: UserService, useValue: mockUserService },
      ],
    }).compileComponents();
  });

  it('should render the character creation screen with all expected sections and copy', async () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;
    const text = element.textContent ?? '';

    expect(element.querySelector('app-navbar')).not.toBeNull();
    expect(element.querySelector('app-register-card')).not.toBeNull();
    expect(element.querySelector('app-cipher-requirements')).not.toBeNull();
    expect(element.querySelector('app-footer')).not.toBeNull();

    expect(text).toContain('NEW SAVE: CREATE NEW CHARACTER');
    expect(text).toContain('SPAWN INTO ORION');
    expect(text).toContain('[ GITHUB GUILD : QUICK SPAWN ]');
    expect(text).toContain('OR MANUAL GUILD REGISTRATION');
    expect(text).toContain('FIRST NAME');
    expect(text).toContain('MIDDLE / TITLE');
    expect(text).toContain('SCROLL ADDRESS (EMAIL)');
    expect(text).toContain('SECRET SPELL');
    expect(text).toContain('CONFIRM SPELL');
    expect(text).toContain('[ CIPHER INTEGRITY REQUIREMENTS ]');
    expect(text).toContain('STATUS: 0/4 RUNES');
    expect(text).toContain('MINIMUM 8 RUNES (8+ CHARS)');
    expect(text).toContain('ANCIENT GLYPH (!@#$%^&*)');
    expect(text).toContain('ELDER CAPITAL RUNE (A-Z)');
    expect(text).toContain('SWIFT MINOR RUNE (a-z)');
    expect(text).toContain('[ FORGE CHARACTER & START LEVEL 1 ]');
    expect(text).toContain('[ RESUME ADVENTURE / LOG IN ]');
  });

  it('should update live password rune requirements as the user types', async () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const passwordInput = element.querySelector('#reg-password') as HTMLInputElement;

    const setPassword = async (value: string) => {
      passwordInput.value = value;
      passwordInput.dispatchEvent(new Event('input'));
      await fixture.whenStable();
    };

    // 1/4: lowercase only
    await setPassword('abc');
    expect(element.textContent).toContain('STATUS: 1/4 RUNES');
    expect(element.querySelector('[data-rune="lowercase"]')?.getAttribute('data-met')).toBe('true');
    expect(element.querySelector('[data-rune="uppercase"]')?.getAttribute('data-met')).toBe(
      'false',
    );

    // 2/4: lowercase + uppercase
    await setPassword('Abc');
    expect(element.textContent).toContain('STATUS: 2/4 RUNES');
    expect(element.querySelector('[data-rune="uppercase"]')?.getAttribute('data-met')).toBe('true');

    // 3/4: lowercase + uppercase + special glyph
    await setPassword('Abc!');
    expect(element.textContent).toContain('STATUS: 3/4 RUNES');
    expect(element.querySelector('[data-rune="special-char"]')?.getAttribute('data-met')).toBe(
      'true',
    );

    // 4/4: all runes satisfied (8+ chars, special, upper, lower)
    await setPassword('Orion!2026');
    expect(element.textContent).toContain('STATUS: 4/4 RUNES');
    expect(element.querySelector('[data-rune="min-length"]')?.getAttribute('data-met')).toBe(
      'true',
    );
  });

  it('should show live match check feedback between Secret Spell and Confirm Spell', async () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const passInput = element.querySelector('#reg-password') as HTMLInputElement;
    const confirmInput = element.querySelector('#reg-confirm-password') as HTMLInputElement;

    passInput.value = 'Orion!2026';
    passInput.dispatchEvent(new Event('input'));

    confirmInput.value = 'Orion!Wrong';
    confirmInput.dispatchEvent(new Event('input'));
    await fixture.whenStable();
    expect(element.textContent).toContain('[ MISMATCH');

    confirmInput.value = 'Orion!2026';
    confirmInput.dispatchEvent(new Event('input'));
    await fixture.whenStable();
    expect(element.textContent).toContain('[ MATCHED');
  });

  it('should toggle password visibility and allow forging a character when valid', async () => {
    const fixture = TestBed.createComponent(RegisterComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const firstName = element.querySelector('#reg-first-name') as HTMLInputElement;
    const email = element.querySelector('#reg-email') as HTMLInputElement;
    const password = element.querySelector('#reg-password') as HTMLInputElement;
    const confirmPassword = element.querySelector('#reg-confirm-password') as HTMLInputElement;
    const terms = element.querySelector('input[type="checkbox"]') as HTMLInputElement;

    // Toggle first SHOW button
    const showButtons = Array.from(element.querySelectorAll('button')).filter(
      (b) => b.textContent?.trim() === 'SHOW',
    );
    expect(showButtons.length).toBe(2);
    showButtons[0].click();
    await fixture.whenStable();
    expect(password.type).toBe('text');

    // Fill valid registration form
    firstName.value = 'Arthur';
    firstName.dispatchEvent(new Event('input'));

    email.value = 'arthur@orion.realm';
    email.dispatchEvent(new Event('input'));

    password.value = 'Excalibur!99';
    password.dispatchEvent(new Event('input'));

    confirmPassword.value = 'Excalibur!99';
    confirmPassword.dispatchEvent(new Event('input'));

    terms.click();
    await fixture.whenStable();

    const form = element.querySelector('form') as HTMLFormElement;
    form.dispatchEvent(new Event('submit'));
    await fixture.whenStable();

    expect(element.textContent).toContain('CHARACTER "ARTHUR" FORGED');
  });
});
