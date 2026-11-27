import { Routes } from '@angular/router';

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
    path: '**',
    redirectTo: '',
  },
];
