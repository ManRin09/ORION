export type CourseTier = 'foundation' | 'intermediate' | 'elective';

export type CourseAccent = 'green' | 'blue' | 'purple' | 'cyan' | 'red';

export type CourseEmblemKind = 'python' | 'web' | 'gate';

export type DashboardNavTab = 'cursos' | 'market' | 'perfil' | 'admin';

export interface CourseDefinition {
  readonly id: string;
  readonly letterCode?: string;
  readonly title: string;
  readonly rightBadge: string;
  readonly tier: CourseTier;
  readonly accent: CourseAccent;
  readonly emblemKind: CourseEmblemKind;
  readonly emblemBadge?: string;
  readonly description: string;
  readonly topics: readonly string[];
  readonly requiredLevel: number;
  readonly totalQuests: number;
  readonly ctaLabel: string;
}

export interface CourseCardState {
  readonly course: CourseDefinition;
  readonly isUnlocked: boolean;
  readonly completedQuests: number;
  readonly progressPercent: number;
  readonly segments: readonly boolean[];
}
