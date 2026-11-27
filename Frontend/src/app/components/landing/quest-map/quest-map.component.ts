import { ChangeDetectionStrategy, Component } from '@angular/core';
import { OVERWORLD_ZONES } from '../../../data/quest-landing.data';
import { ZoneCardComponent } from '../zone-card/zone-card.component';

@Component({
  selector: 'app-quest-map',
  imports: [ZoneCardComponent],
  templateUrl: './quest-map.component.html',
  styleUrl: './quest-map.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuestMapComponent {
  protected readonly zones = OVERWORLD_ZONES;
}
