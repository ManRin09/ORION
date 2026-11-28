import { DatePipe } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CourseTrackCardComponent } from '../../components/dashboard/course-track-card/course-track-card.component';
import { DashboardSidebarComponent } from '../../components/dashboard/dashboard-sidebar/dashboard-sidebar.component';
import { DashboardTopbarComponent } from '../../components/dashboard/dashboard-topbar/dashboard-topbar.component';
import { ElectiveGuildCardComponent } from '../../components/dashboard/elective-guild-card/elective-guild-card.component';
import { PixelIconComponent } from '../../components/shared/pixel-icon/pixel-icon.component';
import {
  ELECTIVE_GUILD_COURSES,
  ELECTIVE_REQUIRED_LEVEL,
  FOUNDATION_COURSES,
  INTERMEDIATE_COURSES,
  PROGRESS_SEGMENT_COUNT,
} from '../../data/course-catalog.data';
import {
  CourseCardState,
  CourseDefinition,
  DashboardNavTab,
} from '../../models/course-catalog.models';
import { AuthService } from '../../services/auth.service';

function buildCourseCardState(course: CourseDefinition, userLevel: number): CourseCardState {
  const completedQuests = 0;
  const progressPercent =
    course.totalQuests > 0
      ? Math.min(100, Math.round((completedQuests / course.totalQuests) * 100))
      : 0;
  const filledSegments = Math.round((progressPercent / 100) * PROGRESS_SEGMENT_COUNT);

  return {
    course,
    isUnlocked: userLevel >= course.requiredLevel,
    completedQuests,
    progressPercent,
    segments: Array.from({ length: PROGRESS_SEGMENT_COUNT }, (_, idx) => idx < filledSegments),
  };
}

@Component({
  selector: 'app-log-home',
  imports: [
    DatePipe,
    RouterLink,
    DashboardTopbarComponent,
    DashboardSidebarComponent,
    CourseTrackCardComponent,
    ElectiveGuildCardComponent,
    PixelIconComponent,
  ],
  templateUrl: './log-home.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    '(document:keydown.escape)': 'handleEscape()',
  },
})
export class LogHomeComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly currentUser = this.authService.currentUser;
  protected readonly isAdmin = this.authService.isAdmin;

  protected readonly sidebarCollapsed = signal<boolean>(false);
  protected readonly mobileSidebarOpen = signal<boolean>(false);
  protected readonly activeTab = signal<DashboardNavTab>('cursos');
  protected readonly loggingOut = signal<boolean>(false);
  protected readonly selectedCourse = signal<CourseDefinition | null>(null);

  protected readonly electiveRequiredLevelLabel = ELECTIVE_REQUIRED_LEVEL.toString().padStart(
    2,
    '0',
  );

  protected readonly userLevel = computed(() => this.currentUser()?.currentLevel ?? 1);

  protected readonly electivesUnlocked = computed(
    () => this.userLevel() >= ELECTIVE_REQUIRED_LEVEL,
  );

  protected readonly foundationCards = computed<readonly CourseCardState[]>(() =>
    FOUNDATION_COURSES.map((course) => buildCourseCardState(course, this.userLevel())),
  );

  protected readonly intermediateCards = computed<readonly CourseCardState[]>(() =>
    INTERMEDIATE_COURSES.map((course) => buildCourseCardState(course, this.userLevel())),
  );

  protected readonly electiveCards = computed<readonly CourseCardState[]>(() =>
    ELECTIVE_GUILD_COURSES.map((course) => buildCourseCardState(course, this.userLevel())),
  );

  protected readonly selectedCourseUnlocked = computed(() => {
    const course = this.selectedCourse();
    return course ? this.userLevel() >= course.requiredLevel : false;
  });

  protected readonly fullDisplayName = computed(() => {
    const user = this.currentUser();
    if (!user) {
      return 'Adventurer';
    }

    return [user.firstName, user.middleName].filter(Boolean).join(' ');
  });

  protected readonly mobileStreakText = computed(() => {
    const streak = this.currentUser()?.currentStreak ?? 0;
    return `${streak} ${streak === 1 ? 'DAY' : 'DAYS'}`;
  });

  ngOnInit(): void {
    this.authService.getCurrentUser().subscribe({
      error: () => {
        // Keep cached session user if available; 401 is handled by authInterceptor.
      },
    });
  }

  protected toggleMenu(): void {
    if (typeof window !== 'undefined' && window.innerWidth < 1024) {
      this.mobileSidebarOpen.update((open) => !open);
      return;
    }

    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  protected toggleDesktopCollapse(): void {
    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  protected closeMobileSidebar(): void {
    this.mobileSidebarOpen.set(false);
  }

  protected selectTab(tab: DashboardNavTab): void {
    this.activeTab.set(tab);
    this.mobileSidebarOpen.set(false);
  }

  protected openCoursePreview(course: CourseDefinition): void {
    this.selectedCourse.set(course);
  }

  protected closeCoursePreview(): void {
    this.selectedCourse.set(null);
  }

  protected handleEscape(): void {
    if (this.selectedCourse()) {
      this.selectedCourse.set(null);
      return;
    }

    if (this.mobileSidebarOpen()) {
      this.mobileSidebarOpen.set(false);
    }
  }

  protected handleLogout(): void {
    if (this.loggingOut()) {
      return;
    }

    this.loggingOut.set(true);
    this.authService.logout().subscribe({
      next: () => {
        this.loggingOut.set(false);
        void this.router.navigate(['/login']);
      },
      error: () => {
        this.loggingOut.set(false);
        void this.router.navigate(['/login']);
      },
    });
  }
}
