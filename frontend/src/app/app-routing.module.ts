import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Import components for route definitions
import { HistoryComponent } from './features/history/history.component';
import { UploadComponent } from './features/upload/upload.component';
import { GenerateComponent } from './features/generate/generate.component';
import { TemplatesComponent } from './features/templates/templates.component';
import { SignaturesComponent } from './features/signatures/signatures.component';
import { EmployeesComponent } from './features/employees/employees.component';
import { PROXKeyComponent } from './features/proxkey/proxkey.component';
import { AdminComponent } from './features/admin/admin.component';

export const routes: Routes = [
  { path: '', redirectTo: '/history', pathMatch: 'full' },
  { path: 'history', component: HistoryComponent, data: { title: 'Email History' } },
  { path: 'upload', component: UploadComponent, data: { title: 'Upload Data' } },
  { path: 'generate', component: GenerateComponent, data: { title: 'Generate Letters' } },
  { path: 'generate/:letterType', component: GenerateComponent, data: { title: 'Generate Letter' } },
  { path: 'templates', component: TemplatesComponent, data: { title: 'Letter Templates' } },
  { path: 'signatures', component: SignaturesComponent, data: { title: 'Digital Signatures' } },
  { path: 'employees', component: EmployeesComponent, data: { title: 'Employees' } },
  { path: 'proxkey', component: PROXKeyComponent, data: { title: 'PROXKey Device' } },
  { 
    path: 'admin', 
    children: [
      { path: 'tabs', component: AdminComponent, data: { title: 'Manage Tabs' } },
      { path: 'settings', component: AdminComponent, data: { title: 'System Settings' } },
      { path: '', redirectTo: 'tabs', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: '/history' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
