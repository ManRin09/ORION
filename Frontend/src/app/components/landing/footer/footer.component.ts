import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FOOTER_LINKS } from '../../../data/quest-landing.data';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';

export interface FooterNavLink {
  readonly label: string;
  readonly route?: string;
  readonly fragment?: string;
  readonly externalUrl?: string;
  readonly badge?: string;
}

export interface FooterColumn {
  readonly id: string;
  readonly title: string;
  readonly links: readonly FooterNavLink[];
}

@Component({
  selector: 'app-footer',
  imports: [RouterLink, PixelIconComponent],
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FooterComponent {
  protected readonly quickLinks = FOOTER_LINKS;
  protected readonly currentYear = new Date().getFullYear();

  protected readonly columns: readonly FooterColumn[] = [
    {
      id: 'quest-realms',
      title: 'QUEST REALMS',
      links: [
        { label: 'Overworld Quest Map', route: '/', fragment: 'quest-map' },
        { label: 'Python Plains', route: '/', fragment: 'quest-map', badge: 'ZONE 01' },
        { label: 'Full-Stack Metropolis', route: '/', fragment: 'quest-map', badge: 'ZONE 02' },
        { label: 'Backend Citadel', route: '/', fragment: 'quest-map', badge: 'BOSS' },
      ],
    },
    {
      id: 'guild-arsenal',
      title: 'GUILD ARSENAL',
      links: [
        { label: 'In-Browser WASM Arena', route: '/', fragment: 'guild-inventory' },
        { label: 'Pyxis Socratic AI', route: '/', fragment: 'guild-inventory' },
        { label: 'Loot & Streak Shields', route: '/', fragment: 'guild-inventory' },
        { label: 'Active Questing Comparison', route: '/', fragment: 'comparison' },
      ],
    },
    {
      id: 'adventurer-portal',
      title: 'ADVENTURER PORTAL',
      links: [
        { label: 'Load Save Slot (Log In)', route: '/login' },
        { label: 'Create Character (Register)', route: '/register', badge: 'FREE' },
        { label: 'Save Point & Guild Tiers', route: '/', fragment: 'save-progress' },
        {
          label: 'GitHub Guild Repository',
          externalUrl: 'https://github.com/ManRin09/ORION',
        },
      ],
    },
  ];

  protected readonly legalLinks: readonly FooterNavLink[] = [
    { label: 'Terms of Space', route: '/register' },
    { label: 'Privacy Scroll', route: '/', fragment: 'guild-inventory' },
    { label: 'Code of Conduct', route: '/register' },
    { label: 'Security & Cipher Runes', route: '/register' },
  ];

  protected scrollToTop(): void {
    if (typeof window !== 'undefined') {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }
}
