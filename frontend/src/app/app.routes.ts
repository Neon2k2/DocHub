import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'templates',
    loadComponent: () => import('./features/templates/templates.component').then(m => m.TemplatesComponent)
  },
  {
    path: 'employees',
    loadComponent: () => import('./features/employees/employees.component').then(m => m.EmployeesComponent)
  },
  {
    path: 'generate',
    loadComponent: () => import('./features/generate/generate.component').then(m => m.GenerateComponent)
  },
  {
    path: 'history',
    loadComponent: () => import('./features/history/history.component').then(m => m.HistoryComponent)
  },
  {
    path: 'signatures',
    loadComponent: () => import('./features/signatures/signatures.component').then(m => m.SignaturesComponent)
  },
  {
    path: 'upload',
    loadComponent: () => import('./features/upload/upload.component').then(m => m.UploadComponent)
  },
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent)
  },
  {
    path: 'proxkey',
    loadComponent: () => import('./features/proxkey/proxkey.component').then(m => m.PROXKeyComponent)
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];
