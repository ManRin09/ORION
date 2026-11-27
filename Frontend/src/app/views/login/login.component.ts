import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { FamiliarGuideCardComponent } from '../../components/auth/familiar-guide-card/familiar-guide-card.component';
import { LoginCardComponent } from '../../components/auth/login-card/login-card.component';
import { FooterComponent } from '../../components/landing/footer/footer.component';
import { NavbarComponent } from '../../components/landing/navbar/navbar.component';
import { FamiliarGuideNotice, LoginCredentials } from '../../models/auth.models';

const DEFAULT_GUIDE_NOTICE: FamiliarGuideNotice = {
  message:
    'Welcome back, adventurer! Your 14-day streak is safely stored in the guild archives. A 5-minute code sprint awaits you.',
  tone: 'default',
};

@Component({
  selector: 'app-login',
  imports: [NavbarComponent, LoginCardComponent, FamiliarGuideCardComponent, FooterComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent {
  protected readonly familiarNotice = signal<FamiliarGuideNotice>(DEFAULT_GUIDE_NOTICE);

  protected handleLoginSubmit(credentials: LoginCredentials): void {
    const persistenceText = credentials.rememberTerminal
      ? 'This magic terminal is now bound to your guild key.'
      : 'Session rune active for this visit only.';

    this.familiarNotice.set({
      message: `Slot 01 unlocked for ${credentials.identifier}! ${persistenceText} Loading your next 5-minute quest...`,
      tone: 'success',
    });
  }

  protected handleValidationFailed(): void {
    this.familiarNotice.set({
      message:
        'Hold on, adventurer! Both your Adventurer Name/Email and a 6+ character Secret Spell are needed to open Slot 01.',
      tone: 'warning',
    });
  }

  protected handleForgotScroll(): void {
    this.familiarNotice.set({
      message:
        'Misplaced your Secret Spell? Enter your Adventurer Email above and our Guild Archivist will dispatch a recovery rune.',
      tone: 'warning',
    });
  }

  protected handleGithubLogin(): void {
    this.familiarNotice.set({
      message:
        'Connecting to the GitHub Guild archive... Verify your repository crest to import your save state.',
      tone: 'default',
    });
  }
}
