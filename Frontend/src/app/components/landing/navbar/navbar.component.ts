import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter, map, startWith } from 'rxjs';
import { NAV_LINKS } from '../../../data/quest-landing.data';
import { AuthService } from '../../../services/auth.service';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';
import { RetroButtonComponent } from '../../shared/retro-button/retro-button.component';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, PixelIconComponent, RetroButtonComponent],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavbarComponent {
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  protected readonly navLinks = NAV_LINKS;
  protected readonly activeNavId = signal<string>('quest-map');
  protected readonly mobileMenuOpen = signal<boolean>(false);
  protected readonly isAuthenticated = this.authService.isAuthenticated;
  protected readonly currentUser = this.authService.currentUser;

  private readonly currentUrl = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map((event) => event.urlAfterRedirects),
      startWith(this.router.url),
    ),
    { initialValue: this.router.url },
  );

  protected readonly isLoginRoute = computed(() => this.currentUrl().startsWith('/login'));
  protected readonly isAdventurersRoute = computed(() =>
    this.currentUrl().startsWith('/adventurers'),
  );
  protected readonly isAuthRoute = computed(
    () =>
      this.isLoginRoute() ||
      this.isAdventurersRoute() ||
      this.currentUrl().startsWith('/register'),
  );

  protected selectNav(id: string): void {
    this.activeNavId.set(id);
    this.mobileMenuOpen.set(false);
  }

  protected isLinkActive(id: string): boolean {
    return !this.isAuthRoute() && this.activeNavId() === id;
  }

  protected toggleMobileMenu(): void {
    this.mobileMenuOpen.update((open) => !open);
  }

  protected logout(): void {
    this.mobileMenuOpen.set(false);
    this.authService.logout().subscribe(() => {
      void this.router.navigate(['/login']);
    });
  }
}
