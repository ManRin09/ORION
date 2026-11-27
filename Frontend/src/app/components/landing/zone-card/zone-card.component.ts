import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RealmZone, ZoneObjectiveStatus } from '../../../models/quest-landing.models';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';

@Component({
  selector: 'app-zone-card',
  imports: [PixelIconComponent],
  templateUrl: './zone-card.component.html',
  styleUrl: './zone-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block h-full',
  },
})
export class ZoneCardComponent {
  readonly zone = input.required<RealmZone>();

  protected readonly cardBorderClass = computed(() => {
    switch (this.zone().accent) {
      case 'green':
        return 'border-[#63de86]';
      case 'gold':
        return 'border-[#f0bf69]';
      case 'muted':
        return 'border-[#3e4a3e]';
    }
  });

  protected readonly zoneCodeClass = computed(() => {
    switch (this.zone().accent) {
      case 'green':
        return 'text-[#63de86]';
      case 'gold':
        return 'text-[#f0bf69]';
      case 'muted':
        return 'text-[#8a8ba0]';
    }
  });

  protected readonly badgeClass = computed(() => {
    switch (this.zone().accent) {
      case 'green':
        return 'bg-[#38b764] text-[#00411c]';
      case 'gold':
        return 'bg-[#775300] text-[#f0bf69]';
      case 'muted':
        return 'bg-[#101222] text-[#99cbff]';
    }
  });

  protected readonly iconFrameClass = computed(() => {
    switch (this.zone().accent) {
      case 'green':
        return 'border-[#63de86] text-[#63de86]';
      case 'gold':
        return 'border-[#f0bf69] text-[#f0bf69]';
      case 'muted':
        return 'border-[#99cbff] text-[#99cbff]';
    }
  });

  protected readonly lootClass = computed(() =>
    this.zone().accent === 'muted' ? 'text-[#99cbff]' : 'text-[#f0bf69]',
  );

  protected readonly progressClass = computed(() => {
    switch (this.zone().accent) {
      case 'green':
        return 'text-[#e1e1f7]';
      case 'gold':
        return 'text-[#f0bf69]';
      case 'muted':
        return 'text-[#8a8ba0]';
    }
  });

  protected objectiveToneClass(status: ZoneObjectiveStatus): string {
    switch (status) {
      case 'completed':
        return 'text-[#63de86]';
      case 'in-progress':
        return 'text-[#f0bf69]';
      case 'active':
        return 'text-[#99cbff]';
      case 'locked':
      case 'inactive':
        return 'text-[#8a8ba0]';
    }
  }
}
