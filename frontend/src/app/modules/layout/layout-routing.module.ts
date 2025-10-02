import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './layout.component';

const routes: Routes = [
  {
    path: '',
    component: LayoutComponent,
    children: [
      {
        path: 'todos',
        loadComponent: () => import('../../features/todos/pages/todos-dashboard/todos-dashboard.component').then(c => c.TodosDashboardComponent),
      },
      {
        path: 'settings',
        loadChildren: () => import('../settings/settings.module').then((m) => m.SettingsModule),
      },
      {
        path: 'notifications',
        loadChildren: () => import('../notifications/notifications.module').then((m) => m.NotificationsModule),
      },
      {
        path: 'components',
        loadChildren: () => import('../uikit/uikit.module').then((m) => m.UikitModule),
      },
      { path: '', redirectTo: 'todos', pathMatch: 'full' },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class LayoutRoutingModule {}