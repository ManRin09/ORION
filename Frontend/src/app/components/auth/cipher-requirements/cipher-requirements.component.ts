import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { evaluatePasswordRunes } from '../../../models/auth.models';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';

@Component({
  selector: 'app-cipher-requirements',
  imports: [PixelIconComponent],
  templateUrl: './cipher-requirements.component.html',
  styleUrl: './cipher-requirements.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block',
  },
})
export class CipherRequirementsComponent {
  readonly password = input.required<string>();

  protected readonly runes = computed(() => evaluatePasswordRunes(this.password()));
  protected readonly metCount = computed(() => this.runes().filter((rune) => rune.met).length);
  protected readonly totalRunes = computed(() => this.runes().length);
  protected readonly isComplete = computed(() => this.metCount() === this.totalRunes());
  protected readonly hasProgress = computed(() => this.metCount() > 0 && !this.isComplete());
}
