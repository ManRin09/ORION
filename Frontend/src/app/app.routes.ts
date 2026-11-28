import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    title: 'ORION — Level Up Your Code in 5-Minute Quests',
    loadComponent: () => import('./views/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'login',
    title: 'Sign In — ORION 16-Bit Realm',
    loadComponent: () => import('./views/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'register',
    title: 'Create Character — ORION 16-Bit Realm',
    loadComponent: () =>
      import('./views/register/register.component').then((m) => m.RegisterComponent),
  },
  {
    path: 'auth/callback',
    title: 'Authenticating — ORION 16-Bit Realm',
    loadComponent: () =>
      import('./views/auth-callback/auth-callback.component').then((m) => m.AuthCallbackComponent),
  },
  {
    path: 'home',
    title: 'Choose Your Quest — ORION',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./views/log-home/log-home.component').then((m) => m.LogHomeComponent),
  },
  {
    path: 'adventurers',
    title: 'Adventurers Roster — ORION 16-Bit Realm',
    canActivate: [adminGuard],
    loadComponent: () =>
      import('./views/adventurers/adventurers.component').then((m) => m.AdventurersComponent),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
