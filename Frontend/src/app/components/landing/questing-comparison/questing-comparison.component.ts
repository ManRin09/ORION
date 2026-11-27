import { ChangeDetectionStrategy, Component } from '@angular/core';
import { QUESTING_COMPARISON_ROWS } from '../../../data/quest-landing.data';
import { ComparisonTone } from '../../../models/quest-landing.models';

@Component({
  selector: 'app-questing-comparison',
  templateUrl: './questing-comparison.component.html',
  styleUrl: './questing-comparison.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuestingComparisonComponent {
  protected readonly rows = QUESTING_COMPARISON_ROWS;

  protected valueToneClass(tone: ComparisonTone): string {
    switch (tone) {
      case 'green':
        return 'text-[#63de86]';
      case 'gold':
        return 'text-[#f0bf69]';
      case 'coral':
        return 'text-[#ffb4ab] font-semibold';
      case 'muted':
        return 'text-[#8a8ba0]';
    }
  }
}
