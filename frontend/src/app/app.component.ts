import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NgxSonnerToaster } from 'ngx-sonner';
import { Subject, takeUntil } from 'rxjs';
import { ThemeService } from './core/services/theme.service';
import { AuthService } from './core/services/auth.service';
import { SignalRService } from './core/services/signalr.service';
import { NotificationService } from './core/services/notification.service';
import { ResponsiveHelperComponent } from './shared/components/responsive-helper/responsive-helper.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  imports: [RouterOutlet, ResponsiveHelperComponent, NgxSonnerToaster],
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'tasky';
  private destroy$ = new Subject<void>();

  constructor(
    public themeService: ThemeService,
    private authService: AuthService,
    private signalRService: SignalRService,
    private notificationService: NotificationService
  ) {}

  ngOnInit() {
    // Initialize SignalR connection when user is authenticated
    this.authService.isAuthenticated$
      .pipe(takeUntil(this.destroy$))
      .subscribe(async (isAuthenticated) => {
        if (isAuthenticated) {
          try {
            await this.signalRService.startConnection();
            
            // Initialize notifications for current user
            const user = this.authService.getCurrentUser();
            if (user?.id) {
              this.notificationService.initializeForUser(user.id);
              await this.signalRService.onUserLogin(user.id);
            }

            // Request notification permissions
            await this.signalRService.requestNotificationPermission();
          } catch (error) {
            console.error('Failed to start SignalR connection:', error);
          }
        } else {
          // Stop SignalR connection when user logs out
          try {
            const user = this.authService.getCurrentUser();
            if (user?.id) {
              await this.signalRService.onUserLogout(user.id);
            }
            await this.signalRService.stopConnection();
          } catch (error) {
            console.error('Failed to stop SignalR connection:', error);
          }
        }
      });

    // Monitor connection state
    this.signalRService.getConnectionState()
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        console.log('SignalR connection state:', state);
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
