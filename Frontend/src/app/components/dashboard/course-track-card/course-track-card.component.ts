import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { CourseCardState, CourseDefinition } from '../../../models/course-catalog.models';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';

@Component({
  selector: 'app-course-track-card',
  imports: [PixelIconComponent],
  templateUrl: './course-track-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block',
  },
})
export class CourseTrackCardComponent {
  readonly state = input.required<CourseCardState>();

  readonly actionSelected = output<CourseDefinition>();

  protected readonly isBlue = computed(() => this.state().course.accent === 'blue');

  protected readonly progressStatusLabel = computed(() => {
    const { completedQuests, progressPercent, course } = this.state();
    if (course.totalQuests === 0) {
      return this.isBlue()
        ? 'READY TO EMBARK (0 QUESTS AVAILABLE)'
        : '0% COMPLETE (0 QUESTS AVAILABLE)';
    }

    return `${progressPercent}% COMPLETE (${completedQuests}/${course.totalQuests} QUESTS)`;
  });

  protected readonly rewardStatusLabel = computed(() => {
    const { course } = this.state();
    if (course.totalQuests === 0) {
      return 'QUESTS COMING SOON';
    }

    return `${course.totalQuests} QUESTS`;
  });

  protected readonly primaryButtonLabel = computed(() => {
    const { completedQuests, course } = this.state();
    if (course.tier === 'foundation') {
      return completedQuests > 0 ? 'RESUME JOURNEY' : course.ctaLabel;
    }

    return course.ctaLabel;
  });
}
