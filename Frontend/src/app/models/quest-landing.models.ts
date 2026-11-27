export type PixelIconName =
  | 'sparkle'
  | 'swords'
  | 'star'
  | 'bolt'
  | 'owl'
  | 'pine-tree'
  | 'castle'
  | 'shield'
  | 'wand'
  | 'paladin'
  | 'ranger'
  | 'crystal-ball'
  | 'steel-shield'
  | 'gold-bag'
  | 'potion'
  | 'diamond'
  | 'user'
  | 'flame'
  | 'check-box'
  | 'minus-box'
  | 'empty-box'
  | 'save-disk'
  | 'id-badge'
  | 'key'
  | 'play-outline'
  | 'github'
  | 'pyxis'
  | 'mail'
  | 'lock'
  | 'shield-check';

export interface NavLinkItem {
  readonly id: string;
  readonly label: string;
  readonly fragment: string;
  readonly isActive?: boolean;
}

export interface GuildStatItem {
  readonly id: string;
  readonly icon: PixelIconName;
  readonly value: string;
  readonly caption: string;
  readonly accent: 'green' | 'gold';
}

export type CodeTokenTone = 'comment-ice' | 'comment-coral' | 'comment-green' | 'keyword' | 'fn' | 'plain';

export interface BattleCodeToken {
  readonly text: string;
  readonly tone: CodeTokenTone;
}

export interface BattleCodeLine {
  readonly lineNumber: number;
  readonly tokens: readonly BattleCodeToken[];
  readonly isFixLine?: boolean;
}

export type ZoneAccent = 'green' | 'gold' | 'muted';

export type ZoneObjectiveKind = 'checklist' | 'subclass';

export type ZoneObjectiveStatus = 'completed' | 'in-progress' | 'locked' | 'active' | 'inactive';

export interface ZoneObjective {
  readonly id: string;
  readonly label: string;
  readonly status: ZoneObjectiveStatus;
  readonly icon: PixelIconName;
}

export interface RealmZone {
  readonly id: string;
  readonly zoneCode: string;
  readonly badgeLabel: string;
  readonly title: string;
  readonly description: string;
  readonly accent: ZoneAccent;
  readonly icon: PixelIconName;
  readonly objectiveKind: ZoneObjectiveKind;
  readonly objectives: readonly ZoneObjective[];
  readonly lootLabel: string;
  readonly progressLabel: string;
}

export type InventoryWidgetKind = 'assertion' | 'clue' | 'loot';

export interface InventoryMechanic {
  readonly id: string;
  readonly title: string;
  readonly description: string;
  readonly icon: PixelIconName;
  readonly iconTone: 'green' | 'gold' | 'slate';
  readonly widgetKind: InventoryWidgetKind;
}

export type ComparisonTone = 'green' | 'gold' | 'coral' | 'muted';

export interface ComparisonRow {
  readonly id: string;
  readonly criteria: string;
  readonly orionValue: string;
  readonly orionTone: ComparisonTone;
  readonly passiveValue: string;
  readonly bootcampValue: string;
  readonly bootcampTone: ComparisonTone;
}

export interface FooterLinkItem {
  readonly label: string;
  readonly fragment: string;
}
