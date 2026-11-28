import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { DashboardNavTab } from '../../../models/course-catalog.models';

interface SidebarNavItem {
  readonly id: DashboardNavTab;
  readonly label: string;
  readonly shortLabel: string;
}

const SIDEBAR_ITEMS: readonly SidebarNavItem[] = [
  { id: 'cursos', label: '[ CURSOS ]', shortLabel: 'C' },
  { id: 'market', label: '[ MARKET ]', shortLabel: 'M' },
  { id: 'perfil', label: '[ PERFIL ]', shortLabel: 'P' },
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

  readonly collapseToggled = output<void>();
  readonly tabSelected = output<DashboardNavTab>();
  readonly logoutRequested = output<void>();

  protected readonly navItems = SIDEBAR_ITEMS;
}
