import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FamiliarGuideCardComponent } from '../../components/auth/familiar-guide-card/familiar-guide-card.component';
import { LoginCardComponent } from '../../components/auth/login-card/login-card.component';
import { FooterComponent } from '../../components/landing/footer/footer.component';
import { NavbarComponent } from '../../components/landing/navbar/navbar.component';
import { FamiliarGuideNotice, LoginCredentials } from '../../models/auth.models';
import { AuthService } from '../../services/auth.service';

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
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly isSubmitting = signal<boolean>(false);
  protected readonly familiarNotice = signal<FamiliarGuideNotice>(DEFAULT_GUIDE_NOTICE);

  protected handleLoginSubmit(credentials: LoginCredentials): void {
    this.isSubmitting.set(true);
    this.familiarNotice.set({
      message: 'Verifying your cipher runes against the Orion guild archives...',
      tone: 'default',
    });

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        const persistenceText = credentials.rememberTerminal
          ? 'This magic terminal is now bound to your guild key.'
          : 'Session rune active for this visit only.';

        this.familiarNotice.set({
          message: `Slot 01 unlocked for ${credentials.identifier}! Welcome back @${response.user.username} (LVL ${response.user.currentLevel}). ${persistenceText}`,
          tone: 'success',
        });

        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        if (returnUrl && returnUrl.startsWith('/')) {
          void this.router.navigateByUrl(returnUrl);
        }
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const detail =
          err.error?.detail ||
          err.error?.title ||
          (err.status === 0
            ? 'Cannot reach Orion API server. Check your connection or backend status.'
            : 'Invalid adventurer name/email or Secret Spell. Please check your runes and try again.');

        this.familiarNotice.set({
          message: detail,
          tone: 'warning',
        });
      },
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
        'Connecting to the GitHub Guild archive... Redirecting to verify your repository crest.',
      tone: 'default',
    });

    if (typeof window !== 'undefined') {
      window.location.assign(this.authService.getGitHubLoginUrl());
    }
  }
}
