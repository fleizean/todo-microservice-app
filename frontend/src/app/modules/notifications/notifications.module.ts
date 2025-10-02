import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AngularSvgIconModule } from 'angular-svg-icon';

import { NotificationsRoutingModule } from './notifications-routing.module';
import { NotificationsComponent } from './notifications.component';
import { NotificationsPageComponent } from './pages/notifications/notifications-page.component';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    AngularSvgIconModule,
    NotificationsRoutingModule,
    NotificationsComponent,
    NotificationsPageComponent
  ]
})
export class NotificationsModule { }