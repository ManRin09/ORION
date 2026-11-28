import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { CourseCardState, CourseDefinition } from '../../../models/course-catalog.models';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';

@Component({
  selector: 'app-elective-guild-card',
  imports: [PixelIconComponent],
  templateUrl: './elective-guild-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block h-full',
  },
})
export class ElectiveGuildCardComponent {
  readonly state = input.required<CourseCardState>();

  readonly previewRequested = output<CourseDefinition>();

  protected readonly torchOuterColor = computed(() => {
    switch (this.state().course.accent) {
      case 'purple':
        return '#7e22ce';
      case 'cyan':
        return '#0284c7';
      case 'red':
        return '#b91c1c';
      default:
        return '#38b764';
    }
  });

  protected readonly torchInnerColor = computed(() => {
    switch (this.state().course.accent) {
      case 'purple':
        return '#c084fc';
      case 'cyan':
        return '#38bdf8';
      case 'red':
        return '#f87171';
      default:
        return '#63de86';
    }
  });

  protected readonly wallTintClass = computed(() => {
    switch (this.state().course.accent) {
      case 'purple':
        return 'bg-[#17162b]';
      case 'cyan':
        return 'bg-[#131d32]';
      case 'red':
        return 'bg-[#26141a]';
      default:
        return 'bg-[#141e24]';
    }
  });

  protected readonly requiredLevelLabel = computed(() =>
    this.state().course.requiredLevel.toString().padStart(2, '0'),
  );
}
