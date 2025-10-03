import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { Subject, takeUntil } from 'rxjs';
import { MenuService } from '../../services/menu.service';
import { NavbarMenuComponent } from './navbar-menu/navbar-menu.component';
import { NavbarMobileComponent } from './navbar-mobile/navbar-mobilecomponent';
import { ProfileMenuComponent } from './profile-menu/profile-menu.component';
import { ThemeService } from '../../../../core/services/theme.service';
import { NotificationService, AppNotification, NotificationType } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
  imports: [CommonModule, RouterModule, AngularSvgIconModule, NavbarMenuComponent, ProfileMenuComponent, NavbarMobileComponent],
})
export class NavbarComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  showNotificationDropdown = false;
  unreadCount = 0;
  recentNotifications: AppNotification[] = [];
  currentUserId: string | null = null;

  constructor(
    private menuService: MenuService, 
    public themeService: ThemeService,
    private notificationService: NotificationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    // Get current user
    this.authService.user$.pipe(takeUntil(this.destroy$)).subscribe(user => {
      this.currentUserId = user?.id || null;
    });

    // Subscribe to unread count updates
    this.notificationService.getUnreadCount()
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.unreadCount = count;
      });

    // Subscribe to notifications updates for recent notifications
    this.notificationService.getNotifications()
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        // Show only the 5 most recent notifications
        this.recentNotifications = notifications.slice(0, 5);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.relative')) {
      this.showNotificationDropdown = false;
    }
  }

  public toggleMobileMenu(): void {
    this.menuService.showMobileMenu = true;
  }

  public toggleNotificationDropdown(): void {
    this.showNotificationDropdown = !this.showNotificationDropdown;
  }

  public closeNotificationDropdown(): void {
    this.showNotificationDropdown = false;
  }

  public markAllAsRead(): void {
    if (!this.currentUserId) return;
    
    this.notificationService.markAllAsRead(this.currentUserId).subscribe({
      next: () => {
        // Update all notifications in state
        this.recentNotifications.forEach(notification => {
          if (!notification.isRead) {
            this.notificationService.updateNotificationInState(notification.id, { isRead: true, readAt: new Date() });
          }
        });
      },
      error: (error) => {
        console.error('Error marking all notifications as read:', error);
      }
    });
  }

  public onNotificationClick(notification: AppNotification): void {
    // Mark as read if unread
    if (!notification.isRead) {
      this.notificationService.markAsRead(notification.id).subscribe({
        next: () => {
          this.notificationService.updateNotificationInState(notification.id, { isRead: true, readAt: new Date() });
        },
        error: (error) => {
          console.error('Error marking notification as read:', error);
        }
      });
    }

    // Close dropdown
    this.closeNotificationDropdown();
  }

  public trackByNotificationId(index: number, notification: AppNotification): number {
    return notification.id;
  }

  public getNotificationTypeIcon(type: NotificationType): string {
    return this.notificationService.getNotificationTypeIcon(type);
  }

  public getNotificationTypeClass(type: NotificationType): string {
    switch (type) {
      case NotificationType.TodoCompleted:
        return 'bg-green-100 text-green-600 dark:bg-green-900/20 dark:text-green-400';
      case NotificationType.TodoCreated:
        return 'bg-blue-100 text-blue-600 dark:bg-blue-900/20 dark:text-blue-400';
      case NotificationType.TodoDeleted:
        return 'bg-red-100 text-red-600 dark:bg-red-900/20 dark:text-red-400';
      case NotificationType.System:
        return 'bg-purple-100 text-purple-600 dark:bg-purple-900/20 dark:text-purple-400';
      default:
        return 'bg-gray-100 text-gray-600 dark:bg-gray-900/20 dark:text-gray-400';
    }
  }

  public formatNotificationTime(createdAt: Date): string {
    return this.notificationService.formatNotificationTime(createdAt);
  }

  get logoSrc(): string {
    return this.themeService.isDark ? 'assets/icons/white_logo.png' : 'assets/icons/dark_logo.png';
  }
}
