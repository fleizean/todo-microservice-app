import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { NotificationService, AppNotification, NotificationType } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-notifications-page',
  standalone: true,
  imports: [CommonModule, FormsModule, AngularSvgIconModule],
  templateUrl: './notifications-page.component.html',
  styleUrls: ['./notifications-page.component.scss']
})
export class NotificationsPageComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  notifications: AppNotification[] = [];
  filter: 'all' | 'unread' | 'read' = 'all';
  typeFilter: NotificationType | 'all' = 'all';
  loading = false;
  unreadCount = 0;
  currentUserId: string | null = null;
  
  readonly NotificationType = NotificationType;
  
  constructor(
    private notificationService: NotificationService,
    private authService: AuthService,
    private router: Router
  ) {}
  
  ngOnInit() {
    // Get current user
    this.authService.user$.pipe(takeUntil(this.destroy$)).subscribe(user => {
      if (user?.id) {
        this.currentUserId = user.id;
        this.notificationService.initializeForUser(user.id);
        this.loadNotifications();
      }
    });

    // Subscribe to notifications updates
    this.notificationService.getNotifications()
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications;
        // Calculate unread count from loaded notifications for accuracy
        this.updateUnreadCountFromNotifications();
      });

    // Subscribe to unread count updates
    this.notificationService.getUnreadCount()
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => {
        this.unreadCount = count;
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadNotifications() {
    if (!this.currentUserId) return;
    
    this.loading = true;
    const isRead = this.filter === 'all' ? undefined : this.filter === 'read';
    const type = this.typeFilter === 'all' ? undefined : this.typeFilter;
    
    this.notificationService.loadUserNotifications(this.currentUserId, 1, 50, isRead, type)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notifications) => {
          this.notifications = notifications;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading notifications:', error);
          this.loading = false;
        }
      });
  }

  get filteredNotifications() {
    return this.notifications; // Filtering is now done server-side
  }

  onFilterChange() {
    this.loadNotifications();
  }

  onTypeFilterChange() {
    this.loadNotifications();
  }

  markAsRead(notification: AppNotification) {
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
  }

  markAllAsRead() {
    if (!this.currentUserId) return;
    
    this.notificationService.markAllAsRead(this.currentUserId).subscribe({
      next: () => {
        // Update all notifications in state
        this.notifications.forEach(notification => {
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

  deleteNotification(id: number) {
    this.notificationService.deleteNotification(id).subscribe({
      next: () => {
        this.notificationService.removeNotificationFromState(id);
      },
      error: (error) => {
        console.error('Error deleting notification:', error);
      }
    });
  }

  getTypeIcon(type: NotificationType): string {
    return this.notificationService.getNotificationTypeIcon(type);
  }

  getTypeLabel(type: NotificationType): string {
    return this.notificationService.getNotificationTypeLabel(type);
  }

  getTypeClass(type: NotificationType): string {
    switch (type) {
      case NotificationType.TodoCompleted:
        return 'bg-green-100 text-green-800 dark:bg-green-900/20 dark:text-green-400';
      case NotificationType.TodoCreated:
        return 'bg-blue-100 text-blue-800 dark:bg-blue-900/20 dark:text-blue-400';
      case NotificationType.TodoDeleted:
        return 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400';
      case NotificationType.System:
        return 'bg-purple-100 text-purple-800 dark:bg-purple-900/20 dark:text-purple-400';
      default:
        return 'bg-gray-100 text-gray-800 dark:bg-gray-900/20 dark:text-gray-400';
    }
  }

  trackByNotificationId(index: number, notification: AppNotification): number {
    return notification.id;
  }

  private updateUnreadCountFromNotifications(): void {
    const unreadCount = this.notifications.filter(notification => !notification.isRead).length;
    this.unreadCount = unreadCount;
  }

  formatTimestamp(createdAt: Date): string {
    return this.notificationService.formatNotificationTime(createdAt);
  }

  getNotificationTypes() {
    return [
      { value: 'all', label: 'All Types' },
      { value: NotificationType.TodoCreated, label: this.getTypeLabel(NotificationType.TodoCreated) },
      { value: NotificationType.TodoCompleted, label: this.getTypeLabel(NotificationType.TodoCompleted) },
      { value: NotificationType.TodoDeleted, label: this.getTypeLabel(NotificationType.TodoDeleted) },
      { value: NotificationType.System, label: this.getTypeLabel(NotificationType.System) }
    ];
  }
}