import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { UserResponse } from '../../../models/user.models';
import { PixelIconComponent } from '../../shared/pixel-icon/pixel-icon.component';

@Component({
  selector: 'app-dashboard-topbar',
  imports: [RouterLink, PixelIconComponent],
  templateUrl: './dashboard-topbar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardTopbarComponent {
  readonly user = input<UserResponse | null>(null);

  readonly menuToggled = output<void>();
  readonly profileSelected = output<void>();

  protected readonly formattedLevel = computed(() => {
    const level = this.user()?.currentLevel ?? 1;
    return level.toString().padStart(2, '0');
  });

  protected readonly streakText = computed(() => {
    const streak = this.user()?.currentStreak ?? 0;
    return `${streak} ${streak === 1 ? 'DAY' : 'DAYS'}`;
  });

  protected readonly gemsCount = computed(() => this.user()?.gems ?? 0);

  protected readonly displayHandle = computed(() => {
    const username = this.user()?.username;
    return username ? `@${username}` : '@adventurer';
  });
}
