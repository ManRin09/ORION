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

  protected readonly torchCoreColor = computed(() => {
    switch (this.state().course.accent) {
      case 'purple':
        return '#f3e8ff';
      case 'cyan':
        return '#e0f2fe';
      case 'red':
        return '#fee2e2';
      default:
        return '#dcfce7';
    }
  });

  protected readonly wallTintClass = computed(() => {
    switch (this.state().course.accent) {
      case 'purple':
        return 'bg-[#17162e]';
      case 'cyan':
        return 'bg-[#121e36]';
      case 'red':
        return 'bg-[#28131a]';
      default:
        return 'bg-[#141e24]';
    }
  });

  protected readonly cardAccentBorderClass = computed(() => {
    switch (this.state().course.accent) {
      case 'purple':
        return 'border-[#332a57] hover:border-[#a855f7]';
      case 'cyan':
        return 'border-[#253b61] hover:border-[#38bdf8]';
      case 'red':
        return 'border-[#4a232d] hover:border-[#f87171]';
      default:
        return 'border-[#2d334d] hover:border-[#63de86]';
    }
  });

  protected readonly topicPillClass = computed(() => {
    switch (this.state().course.accent) {
      case 'purple':
        return 'border-[#3c2d69] text-[#d8b4fe]';
      case 'cyan':
        return 'border-[#24436e] text-[#7dd3fc]';
      case 'red':
        return 'border-[#572631] text-[#fca5a5]';
      default:
        return 'border-[#2e4a38] text-[#86efac]';
    }
  });

  protected readonly topAccentBarClass = computed(() => {
    switch (this.state().course.accent) {
      case 'purple':
        return 'bg-[#9333ea]';
      case 'cyan':
        return 'bg-[#0ea5e9]';
      case 'red':
        return 'bg-[#dc2626]';
      default:
        return 'bg-[#38b764]';
    }
  });

  protected readonly requiredLevelLabel = computed(() =>
    this.state().course.requiredLevel.toString().padStart(2, '0'),
  );
}
