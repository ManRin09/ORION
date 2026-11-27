import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [provideRouter([])],
    }).compileComponents();
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

  it('should unlock Slot 01 when valid credentials are submitted', async () => {
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
    await fixture.whenStable();

    expect(element.textContent).toContain('Slot 01 unlocked for warrior_dev@orion.realm');
  });
});
