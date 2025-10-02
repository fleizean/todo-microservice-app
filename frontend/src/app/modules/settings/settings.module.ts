import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AngularSvgIconModule } from 'angular-svg-icon';

import { SettingsRoutingModule } from './settings-routing.module';
import { SettingsComponent } from './settings.component';
import { SettingsPageComponent } from './pages/settings/settings-page.component';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    FormsModule,
    AngularSvgIconModule,
    SettingsRoutingModule,
    SettingsComponent,
    SettingsPageComponent
  ]
})
export class SettingsModule { }