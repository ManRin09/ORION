import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { PixelIconName } from '../../../models/quest-landing.models';

@Component({
  selector: 'app-pixel-icon',
  templateUrl: './pixel-icon.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'inline-flex shrink-0 items-center justify-center leading-none',
    'aria-hidden': 'true',
  },
})
export class PixelIconComponent {
  readonly name = input.required<PixelIconName>();
  readonly size = input<number>(20);
}
