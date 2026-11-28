import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FooterComponent } from '../../components/landing/footer/footer.component';
import { NavbarComponent } from '../../components/landing/navbar/navbar.component';
import { PixelIconComponent } from '../../components/shared/pixel-icon/pixel-icon.component';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-auth-callback',
  imports: [RouterLink, NavbarComponent, FooterComponent, PixelIconComponent],
  template: `
    <div class="min-h-screen bg-[#0b0d1c] text-[#e1e1f7] flex flex-col">
      <app-navbar />
      <main class="flex-1 retro-crt-backdrop px-5 py-16 flex items-center justify-center">
        <section
          class="bg-[#181a2a] border-2 border-[#3e4a3e] pixel-shadow-lg max-w-md w-full p-8 text-center space-y-4"
        >
          <app-pixel-icon name="sparkle" [size]="24" class="mx-auto text-[#63de86]" />
          <h1 class="font-display text-[15px] text-[#f0bf69]">[ RESTORING GUILD SESSION ]</h1>

          @if (errorMessage(); as error) {
            <p role="alert" class="font-code text-[12px] text-[#ffb4ab]">
              {{ error }}
            </p>
            <a
              routerLink="/login"
              class="inline-block bg-[#272939] border-2 border-[#63de86] px-4 py-2 font-pixel text-[11px] text-[#63de86]"
            >
              [ RETURN TO LOGIN ]
            </a>
          } @else {
            <p class="font-code text-[12px] text-[#bdcabb]">
              Validating your Orion bearer token with the guild archive...
            </p>
          }
        </section>
      </main>
      <app-footer />
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthCallbackComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    const query = this.route.snapshot.queryParamMap;
    const error = query.get('error');
    if (error) {
      this.errorMessage.set(`Authentication could not be completed (${error}).`);
      return;
    }

    const token = query.get('token');
    if (!token) {
      this.errorMessage.set('Missing authentication token in callback URL.');
      return;
    }

    this.authService.completeExternalLogin(token, true).subscribe({
      next: () => {
        void this.router.navigate(['/adventurers']);
      },
      error: () => {
        this.authService.clearSession();
        this.errorMessage.set('Unable to verify your session token. Please sign in again.');
      },
    });
  }
}
