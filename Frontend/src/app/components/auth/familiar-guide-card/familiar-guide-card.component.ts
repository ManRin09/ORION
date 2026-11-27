import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { FamiliarTone } from '../../../models/auth.models';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';

@Component({
  selector: 'app-familiar-guide-card',
  imports: [PixelIconComponent],
  templateUrl: './familiar-guide-card.component.html',
  styleUrl: './familiar-guide-card.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block',
  },
})
export class FamiliarGuideCardComponent {
  readonly message = input.required<string>();
  readonly tone = input<FamiliarTone>('default');
}
