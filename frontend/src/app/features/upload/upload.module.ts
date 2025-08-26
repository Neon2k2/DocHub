import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';

import { UploadComponent } from './upload.component';

const routes = [
  {
    path: '',
    component: UploadComponent
  }
];

@NgModule({
  imports: [
    RouterModule.forChild(routes)
  ]
})
export class UploadModule { }
