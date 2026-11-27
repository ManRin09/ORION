import { ChangeDetectionStrategy, Component } from '@angular/core';
import { GUILD_INVENTORY_ITEMS } from '../../../data/quest-landing.data';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';

@Component({
  selector: 'app-guild-inventory',
  imports: [PixelIconComponent],
  templateUrl: './guild-inventory.component.html',
  styleUrl: './guild-inventory.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GuildInventoryComponent {
  protected readonly items = GUILD_INVENTORY_ITEMS;

  protected iconBadgeClass(tone: 'green' | 'gold' | 'slate'): string {
    switch (tone) {
      case 'green':
        return 'bg-[#38b764] text-[#f0bf69]';
      case 'gold':
        return 'bg-[#775300] text-[#f0bf69]';
      case 'slate':
        return 'bg-[#272939] text-[#c2c3d7]';
    }
  }
}
