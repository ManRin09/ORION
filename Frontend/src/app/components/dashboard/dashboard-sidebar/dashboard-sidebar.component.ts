import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { DashboardNavTab } from '../../../models/course-catalog.models';

interface SidebarNavItem {
  readonly id: DashboardNavTab;
  readonly label: string;
  readonly shortLabel: string;
  readonly adminOnly?: boolean;
}

const SIDEBAR_ITEMS: readonly SidebarNavItem[] = [
  { id: 'courses', label: '[ COURSES ]', shortLabel: '[ C ]' },
  { id: 'market', label: '[ MARKET ]', shortLabel: '[ M ]' },
  { id: 'profile', label: '[ PROFILE ]', shortLabel: '[ P ]' },
  { id: 'admin', label: '[ ADMIN PANEL ]', shortLabel: '[ A ]', adminOnly: true },
];

@Component({
  selector: 'app-dashboard-sidebar',
  templateUrl: './dashboard-sidebar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'block h-full',
  },
})
export class DashboardSidebarComponent {
  readonly activeTab = input.required<DashboardNavTab>();
  readonly collapsed = input<boolean>(false);
  readonly loggingOut = input<boolean>(false);
  readonly isAdmin = input<boolean>(false);

  readonly collapseToggled = output<void>();
  readonly tabSelected = output<DashboardNavTab>();
  readonly logoutRequested = output<void>();

  protected readonly navItems = computed(() =>
    SIDEBAR_ITEMS.filter((item) => !item.adminOnly || this.isAdmin()),
  );
}
