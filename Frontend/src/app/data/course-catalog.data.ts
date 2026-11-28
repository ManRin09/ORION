import { CourseDefinition } from '../models/course-catalog.models';

export const FOUNDATION_COURSES: readonly CourseDefinition[] = [
  {
    id: 'python-fundamentals',
    letterCode: 'A',
    title: 'PYTHON FUNDAMENTALS',
    rightBadge: 'PROGRAMMING LOGIC WITH PYTHON',
    tier: 'foundation',
    accent: 'green',
    emblemKind: 'python',
    emblemBadge: 'PY 3',
    description:
      'MASTER VARIABLES, LOOPS, FUNCTIONS, AND DATA STRUCTURES THROUGH 5-MINUTE CODING BATTLES AND SYNTAX DUNGEONS.',
    topics: ['LOOPS & LOGIC', 'OOP & MODULES', 'RECURSION'],
    requiredLevel: 1,
    totalQuests: 0,
    ctaLabel: 'START JOURNEY',
  },
];

export const INTERMEDIATE_COURSES: readonly CourseDefinition[] = [
  {
    id: 'fullstack-app-design',
    letterCode: 'B',
    title: 'FULLSTACK APP DESIGN',
    rightBadge: 'HTML · CSS · JS · REACT · NODE',
    tier: 'intermediate',
    accent: 'blue',
    emblemKind: 'web',
    emblemBadge: '[ CHROME / WEB ]',
    description:
      'BUILD INTERACTIVE INTERFACES, RESPONSIVE DESIGN SYSTEMS, REACTIVE STATE ENGINES, DOM LOGIC, AND PRODUCTION CLIENT APPLICATIONS.',
    topics: ['HTML5 & CSS3', 'RESPONSIVE UI', 'REACT & NODE APIS'],
    requiredLevel: 1,
    totalQuests: 0,
    ctaLabel: '[ PREVIEW & START ]',
  },
];

export const ELECTIVE_GUILD_COURSES: readonly CourseDefinition[] = [
  {
    id: 'aspnet-core',
    title: 'ASP.NET CORE',
    rightBadge: 'LVL 05 GATE',
    tier: 'elective',
    accent: 'purple',
    emblemKind: 'gate',
    description:
      'CLEAN ARCHITECTURE, HIGH-PERFORMANCE C# RESTFUL APIS, EF CORE, AND CONCURRENCY PATTERNS.',
    topics: ['CLEAN ARCHITECTURE', 'EF CORE & POSTGRESQL', 'JWT & CONCURRENCY'],
    requiredLevel: 5,
    totalQuests: 0,
    ctaLabel: 'PREVIEW SYLLABUS',
  },
  {
    id: 'nextjs',
    title: 'NEXT.JS',
    rightBadge: 'LVL 05 GATE',
    tier: 'elective',
    accent: 'cyan',
    emblemKind: 'gate',
    description:
      'REACT SERVER COMPONENTS, APP ROUTER 15, SSR STREAMING, CACHING ARCHITECTURES, AND VERCEL EDGE RUNTIME.',
    topics: ['SERVER COMPONENTS', 'APP ROUTER & SSR', 'EDGE CACHING'],
    requiredLevel: 5,
    totalQuests: 0,
    ctaLabel: 'PREVIEW SYLLABUS',
  },
  {
    id: 'nestjs',
    title: 'NESTJS',
    rightBadge: 'LVL 05 GATE',
    tier: 'elective',
    accent: 'red',
    emblemKind: 'gate',
    description:
      'ENTERPRISE TYPESCRIPT ARCHITECTURE, DEPENDENCY INJECTION, MICROSERVICES, GRPC, AND GRAPHQL APIS.',
    topics: ['DEPENDENCY INJECTION', 'MICROSERVICES & GRPC', 'GRAPHQL APIS'],
    requiredLevel: 5,
    totalQuests: 0,
    ctaLabel: 'PREVIEW SYLLABUS',
  },
];

export const ELECTIVE_REQUIRED_LEVEL = 5;
export const PROGRESS_SEGMENT_COUNT = 10;
