import { ChangeDetectionStrategy, Component } from '@angular/core';
import { HERO_GUILD_STATS } from '../../../data/quest-landing.data';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';
import { RetroButtonComponent } from '../../shared/retro-button/retro-button.component';
import { BattleArenaComponent } from '../battle-arena/battle-arena.component';

@Component({
  selector: 'app-hero-section',
  imports: [PixelIconComponent, RetroButtonComponent, BattleArenaComponent],
  templateUrl: './hero-section.component.html',
  styleUrl: './hero-section.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeroSectionComponent {
  protected readonly guildStats = HERO_GUILD_STATS;
}
