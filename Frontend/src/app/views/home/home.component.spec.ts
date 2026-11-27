import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HomeComponent } from './home.component';

describe('HomeComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should compose all 16-bit Orion landing sections', async () => {
    const fixture = TestBed.createComponent(HomeComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance).toBeTruthy();
    expect(element.querySelector('app-navbar')).not.toBeNull();
    expect(element.querySelector('app-hero-section')).not.toBeNull();
    expect(element.querySelector('app-battle-arena')).not.toBeNull();
    expect(element.querySelector('app-quest-map')).not.toBeNull();
    expect(element.querySelectorAll('app-zone-card').length).toBe(3);
    expect(element.querySelector('app-guild-inventory')).not.toBeNull();
    expect(element.querySelector('app-questing-comparison')).not.toBeNull();
    expect(element.querySelector('app-save-point-cta')).not.toBeNull();
    expect(element.querySelector('app-footer')).not.toBeNull();
  });

  it('should render the design copy across every section', async () => {
    const fixture = TestBed.createComponent(HomeComponent);
    await fixture.whenStable();
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';

    expect(text).toContain('[ PRESS START ]');
    expect(text).toContain('QUEST 01 AVAILABLE');
    expect(text).toContain('LEVEL UP YOUR');
    expect(text).toContain('[ START QUEST 1 (FREE) ]');
    expect(text).toContain('[ VIEW SKILL TREE ]');
    expect(text).toContain('12,480 CODERS');
    expect(text).toContain('4.9 / 5 GUILD RATING');
    expect(text).toContain('INFINITE LOOP GOBLIN LV.3');
    expect(text).toContain('slay_dragon.py');
    expect(text).toContain('ORION FAMILIAR SAYS:');
    expect(text).toContain('OVERWORLD QUEST MAP');
    expect(text).toContain('PYTHON PLAINS');
    expect(text).toContain('FULL-STACK METROPOLIS');
    expect(text).toContain('BACKEND CITADEL');
    expect(text).toContain('GUILD INVENTORY &');
    expect(text).toContain('01. IN-BROWSER WASM ARENA');
    expect(text).toContain('02. SOCRATIC AI FAMILIAR');
    expect(text).toContain('03. LOOT & STREAK SHIELDS');
    expect(text).toContain('WHY ACTIVE QUESTING WINS');
    expect(text).toContain('95% OF SESSION TIME');
    expect(text).toContain('SAVE PROGRESS.');
    expect(text).toContain('[ CREATE CHARACTER & START QUEST ]');
    expect(text).toContain('INSERT COIN TO CONTINUE');
  });

  it('should update goblin HP and MP when casting Run Code in the Battle Arena', async () => {
    const fixture = TestBed.createComponent(HomeComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const castButton = Array.from(element.querySelectorAll('button')).find((btn) =>
      btn.textContent?.includes('CAST RUN CODE'),
    );
    expect(castButton).toBeTruthy();

    castButton?.click();
    await fixture.whenStable();

    const updatedText = element.textContent ?? '';
    expect(updatedText).toContain('156/240');
    expect(updatedText).toContain('MP: 70/100');
  });

  it('should link the Save Point CTA and navbar PRESS START to /register', async () => {
    const fixture = TestBed.createComponent(HomeComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const ctaLink = Array.from(element.querySelectorAll('a')).find((anchor) =>
      anchor.textContent?.includes('CREATE CHARACTER & START QUEST'),
    );
    expect(ctaLink).toBeTruthy();
    expect(ctaLink?.getAttribute('href')).toBe('/register');

    const pressStartLink = Array.from(element.querySelectorAll('a')).find((anchor) =>
      anchor.textContent?.includes('[ PRESS START ]'),
    );
    expect(pressStartLink).toBeTruthy();
    expect(pressStartLink?.getAttribute('href')).toBe('/register');
  });

  it('should link the navbar LOG IN button to /login', async () => {
    const fixture = TestBed.createComponent(HomeComponent);
    await fixture.whenStable();
    const element = fixture.nativeElement as HTMLElement;

    const loginLink = Array.from(element.querySelectorAll('a')).find(
      (anchor) => anchor.textContent?.trim() === 'LOG IN',
    );
    expect(loginLink).toBeTruthy();
    expect(loginLink?.getAttribute('href')).toBe('/login');
  });
});
