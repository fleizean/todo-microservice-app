import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NotificationsComponent } from './notifications.component';
import { NotificationsPageComponent } from './pages/notifications/notifications-page.component';

const routes: Routes = [
  {
    path: '',
    component: NotificationsComponent,
    children: [
      { path: '', component: NotificationsPageComponent },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class NotificationsRoutingModule { }