import {
  BattleCodeLine,
  ComparisonRow,
  FooterLinkItem,
  GuildStatItem,
  InventoryMechanic,
  NavLinkItem,
  RealmZone,
} from '../models/quest-landing.models';

export const NAV_LINKS: readonly NavLinkItem[] = [
  { id: 'quest-map', label: 'QUEST MAP', fragment: 'quest-map', isActive: true },
  { id: 'ai-familiar', label: 'AI FAMILIAR', fragment: 'guild-inventory' },
  { id: 'item-shop', label: 'ITEM SHOP', fragment: 'comparison' },
];

export const HERO_GUILD_STATS: readonly GuildStatItem[] = [
  {
    id: 'active-coders',
    icon: 'swords',
    value: '12,480 CODERS',
    caption: 'QUESTING RIGHT NOW',
    accent: 'green',
  },
  {
    id: 'guild-rating',
    icon: 'star',
    value: '4.9 / 5 GUILD RATING',
    caption: 'FROM 18,200 REVIEWS',
    accent: 'gold',
  },
];

export const BATTLE_CODE_LINES: readonly BattleCodeLine[] = [
  {
    lineNumber: 1,
    tokens: [{ text: '# Defeat the Infinite Loop Bug!', tone: 'comment-ice' }],
  },
  {
    lineNumber: 2,
    tokens: [
      { text: 'def ', tone: 'keyword' },
      { text: 'slay_dragon', tone: 'fn' },
      { text: '(mana_pool, attacks):', tone: 'plain' },
    ],
  },
  {
    lineNumber: 3,
    tokens: [
      { text: '    damage = ', tone: 'plain' },
      { text: '0', tone: 'keyword' },
    ],
  },
  {
    lineNumber: 4,
    tokens: [
      { text: '    ', tone: 'plain' },
      { text: 'while ', tone: 'keyword' },
      { text: 'mana_pool > ', tone: 'plain' },
      { text: '0', tone: 'keyword' },
      { text: ':', tone: 'plain' },
    ],
  },
  {
    lineNumber: 5,
    tokens: [{ text: '        damage += attacks.pop()', tone: 'plain' }],
  },
  {
    lineNumber: 6,
    tokens: [{ text: '        # BUG: mana_pool never decreases!', tone: 'comment-coral' }],
  },
  {
    lineNumber: 7,
    isFixLine: true,
    tokens: [{ text: '        # FIX: mana_pool -= 10', tone: 'comment-green' }],
  },
  {
    lineNumber: 8,
    tokens: [
      { text: '    ', tone: 'plain' },
      { text: 'return ', tone: 'keyword' },
      { text: 'damage', tone: 'plain' },
    ],
  },
];

export const OVERWORLD_ZONES: readonly RealmZone[] = [
  {
    id: 'zone-01',
    zoneCode: 'ZONE 01',
    badgeLabel: '[ UNLOCKED ]',
    title: 'PYTHON PLAINS',
    description:
      'Master the fundamental dialects of indentation, dynamic typing, and list-comprehension spellcasting.',
    accent: 'green',
    icon: 'pine-tree',
    objectiveKind: 'checklist',
    objectives: [
      { id: 'py-vars', label: 'Variables & Data Types', status: 'completed', icon: 'check-box' },
      { id: 'py-loops', label: 'Loop Dungeons', status: 'completed', icon: 'check-box' },
      { id: 'py-funcs', label: 'Function Scrolls', status: 'completed', icon: 'check-box' },
      { id: 'py-arrays', label: 'Data Structure Arrays', status: 'in-progress', icon: 'minus-box' },
    ],
    lootLabel: 'LOOT: WEB WAND',
    progressLabel: '14/24 SHRINES',
  },
  {
    id: 'zone-02',
    zoneCode: 'ZONE 02',
    badgeLabel: '[ IN PROGRESS · LV.8 ]',
    title: 'FULL-STACK METROPOLIS',
    description:
      'Scale DOM spires, tame reactive state hydras, and summon asynchronous network queries across cyberspace.',
    accent: 'gold',
    icon: 'bolt',
    objectiveKind: 'checklist',
    objectives: [
      { id: 'fs-html', label: 'Semantic HTML Bastion', status: 'completed', icon: 'check-box' },
      { id: 'fs-grid', label: 'CSS Grid Mazes', status: 'completed', icon: 'check-box' },
      { id: 'fs-async', label: 'Async JavaScript Runes', status: 'in-progress', icon: 'minus-box' },
      { id: 'fs-react', label: 'React Component Forge', status: 'locked', icon: 'empty-box' },
    ],
    lootLabel: 'LOOT: DB KEY',
    progressLabel: '08/32 SHRINES',
  },
  {
    id: 'zone-03',
    zoneCode: 'ZONE 03',
    badgeLabel: '[ BOSS REALM ]',
    title: 'BACKEND CITADEL',
    description:
      'Equip your heavy architectural subclass to defend relational data fortresses against concurrency raids.',
    accent: 'muted',
    icon: 'castle',
    objectiveKind: 'subclass',
    objectives: [
      { id: 'be-node', label: 'Knight of Node / NestJS', status: 'active', icon: 'shield' },
      { id: 'be-next', label: 'Sorcerer of Next.js SSR', status: 'active', icon: 'wand' },
      { id: 'be-dotnet', label: 'Paladin of ASP.NET Core', status: 'inactive', icon: 'paladin' },
      { id: 'be-spring', label: 'Ranger of Spring Boot', status: 'inactive', icon: 'ranger' },
    ],
    lootLabel: 'LOOT: ARCHITECT RELIC',
    progressLabel: 'LOCKED · LV.15 REQ',
  },
];

export const GUILD_INVENTORY_ITEMS: readonly InventoryMechanic[] = [
  {
    id: 'wasm-arena',
    title: '01. IN-BROWSER WASM ARENA',
    description:
      'Every quest compiles in an isolated WebAssembly sandbox right in your client tab. Zero local environment setup, zero Docker bloat, and sub-50ms execution speed.',
    icon: 'bolt',
    iconTone: 'green',
    widgetKind: 'assertion',
  },
  {
    id: 'socratic-familiar',
    title: '02. SOCRATIC AI FAMILIAR',
    description:
      'Never feeds you raw solutions or boilerplate copy-paste text. Orion prompts you with insightful questions about execution order and memory frames, guiding you to solve bugs yourself.',
    icon: 'crystal-ball',
    iconTone: 'gold',
    widgetKind: 'clue',
  },
  {
    id: 'loot-shields',
    title: '03. LOOT & STREAK SHIELDS',
    description:
      'Earn gold coins for every test passed. Equip retro pixel CRT IDE themes, custom monospace font scrolls, and deploy Streak Freeze potions when life summons unexpected side-quests.',
    icon: 'steel-shield',
    iconTone: 'slate',
    widgetKind: 'loot',
  },
];

export const QUESTING_COMPARISON_ROWS: readonly ComparisonRow[] = [
  {
    id: 'typing-ratio',
    criteria: 'Active Typing Ratio',
    orionValue: '95% OF SESSION TIME',
    orionTone: 'green',
    passiveValue: '< 10% (Tough to follow along)',
    bootcampValue: '~40% (Heavy lectures)',
    bootcampTone: 'muted',
  },
  {
    id: 'feedback-speed',
    criteria: 'Feedback Loop Speed',
    orionValue: '< 50ms (INSTANT WASM)',
    orionTone: 'green',
    passiveValue: 'None (Zero verification)',
    bootcampValue: '24-48 Hours for TA review',
    bootcampTone: 'muted',
  },
  {
    id: 'ai-mentor',
    criteria: 'AI Mentor Dynamic',
    orionValue: 'SOCRATIC (NO SPOILERS)',
    orionTone: 'gold',
    passiveValue: 'None',
    bootcampValue: 'Varies by TA mood',
    bootcampTone: 'muted',
  },
  {
    id: 'setup-friction',
    criteria: 'Setup Friction',
    orionValue: 'ZERO (1-CLICK IN BROWSER)',
    orionTone: 'green',
    passiveValue: 'Install nightmares',
    bootcampValue: 'Weeks of terminal setup',
    bootcampTone: 'muted',
  },
  {
    id: 'cost-barrier',
    criteria: 'Cost Barrier',
    orionValue: 'FREE CORE QUESTS',
    orionTone: 'green',
    passiveValue: '$20-$40 / month',
    bootcampValue: '$15,000 - $25,000 debt',
    bootcampTone: 'coral',
  },
];

export const FOOTER_LINKS: readonly FooterLinkItem[] = [
  { label: 'QUEST LOG', fragment: 'quest-map' },
  { label: 'GUILD RULES', fragment: 'guild-inventory' },
  { label: 'SAVE STATES', fragment: 'comparison' },
  { label: 'SYSTEM STATUS', fragment: 'save-progress' },
];
