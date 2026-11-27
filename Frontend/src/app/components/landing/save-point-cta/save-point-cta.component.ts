import { ChangeDetectionStrategy, Component } from '@angular/core';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';
import { RetroButtonComponent } from '../../shared/retro-button/retro-button.component';

@Component({
  selector: 'app-save-point-cta',
  imports: [PixelIconComponent, RetroButtonComponent],
  templateUrl: './save-point-cta.component.html',
  styleUrl: './save-point-cta.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SavePointCtaComponent {}
