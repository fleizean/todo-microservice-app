import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AppNotification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  timestamp: Date;
  read: boolean;
  actionable?: boolean;
  actionText?: string;
  actionUrl?: string;
  relatedEntityId?: string;
  relatedEntityType?: 'todo' | 'user' | 'system';
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly API_URL = environment.apiUrl.auth;
  private readonly NOTIFICATIONS_KEY = 'app_notifications';
  private readonly MAX_NOTIFICATIONS = 100;
  
  private notificationsSubject = new BehaviorSubject<AppNotification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadNotifications();
    this.generateInitialNotifications();
  }

  private loadNotifications(): void {
    try {
      const stored = localStorage.getItem(this.NOTIFICATIONS_KEY);
      if (stored) {
        const notifications = JSON.parse(stored);
        // Convert timestamp strings back to Date objects
        notifications.forEach((n: any) => {
          n.timestamp = new Date(n.timestamp);
        });
        this.notificationsSubject.next(notifications);
      }
    } catch (error) {
      console.error('Error loading notifications:', error);
    }
  }

  private saveNotifications(notifications: AppNotification[]): void {
    try {
      localStorage.setItem(this.NOTIFICATIONS_KEY, JSON.stringify(notifications));
    } catch (error) {
      console.error('Error saving notifications:', error);
    }
  }

  private generateInitialNotifications(): void {
    const currentNotifications = this.notificationsSubject.value;
    if (currentNotifications.length > 0) return;

    const initialNotifications: AppNotification[] = [
      {
        id: this.generateId(),
        title: 'Welcome to Tasky!',
        message: 'Start organizing your tasks efficiently with our powerful task management system.',
        type: 'info',
        timestamp: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
        read: false,
        actionable: true,
        actionText: 'Create First Task',
        actionUrl: '/dashboard/todos'
      },
      {
        id: this.generateId(),
        title: 'Task Reminder',
        message: 'You have 3 tasks due today. Don\'t forget to complete them!',
        type: 'warning',
        timestamp: new Date(Date.now() - 1000 * 60 * 60 * 2), // 2 hours ago
        read: false,
        actionable: true,
        actionText: 'View Tasks',
        actionUrl: '/dashboard/todos'
      },
      {
        id: this.generateId(),
        title: 'Profile Updated',
        message: 'Your profile information has been successfully updated.',
        type: 'success',
        timestamp: new Date(Date.now() - 1000 * 60 * 60 * 24), // 1 day ago
        read: true
      },
      {
        id: this.generateId(),
        title: 'Weekly Summary',
        message: 'Great job! You completed 15 out of 18 tasks this week.',
        type: 'info',
        timestamp: new Date(Date.now() - 1000 * 60 * 60 * 24 * 2), // 2 days ago
        read: true
      },
      {
        id: this.generateId(),
        title: 'New Feature Available',
        message: 'Check out the new dark mode option in Settings!',
        type: 'info',
        timestamp: new Date(Date.now() - 1000 * 60 * 60 * 24 * 3), // 3 days ago
        read: false,
        actionable: true,
        actionText: 'Go to Settings',
        actionUrl: '/settings'
      }
    ];

    this.notificationsSubject.next(initialNotifications);
    this.saveNotifications(initialNotifications);
  }

  private generateId(): string {
    return Date.now().toString(36) + Math.random().toString(36).substr(2);
  }

  getNotifications(): Observable<AppNotification[]> {
    return this.notifications$;
  }

  getCurrentNotifications(): AppNotification[] {
    return this.notificationsSubject.value;
  }

  getUnreadCount(): number {
    return this.getCurrentNotifications().filter(n => !n.read).length;
  }

  getTodayCount(): number {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    return this.getCurrentNotifications().filter(n => {
      const notificationDate = new Date(n.timestamp);
      return notificationDate >= today && notificationDate < tomorrow;
    }).length;
  }

  addNotification(notification: Omit<AppNotification, 'id' | 'timestamp'>): void {
    const newNotification: AppNotification = {
      ...notification,
      id: this.generateId(),
      timestamp: new Date()
    };

    const currentNotifications = this.getCurrentNotifications();
    const updatedNotifications = [newNotification, ...currentNotifications];
    
    // Keep only the latest MAX_NOTIFICATIONS
    if (updatedNotifications.length > this.MAX_NOTIFICATIONS) {
      updatedNotifications.splice(this.MAX_NOTIFICATIONS);
    }

    this.notificationsSubject.next(updatedNotifications);
    this.saveNotifications(updatedNotifications);
  }

  markAsRead(notificationId: string): void {
    const currentNotifications = this.getCurrentNotifications();
    const notification = currentNotifications.find(n => n.id === notificationId);
    
    if (notification && !notification.read) {
      notification.read = true;
      this.notificationsSubject.next([...currentNotifications]);
      this.saveNotifications(currentNotifications);
    }
  }

  markAllAsRead(): void {
    const currentNotifications = this.getCurrentNotifications();
    const updated = currentNotifications.map(n => ({ ...n, read: true }));
    this.notificationsSubject.next(updated);
    this.saveNotifications(updated);
  }

  deleteNotification(notificationId: string): void {
    const currentNotifications = this.getCurrentNotifications();
    const filtered = currentNotifications.filter(n => n.id !== notificationId);
    this.notificationsSubject.next(filtered);
    this.saveNotifications(filtered);
  }

  clearAllRead(): void {
    const currentNotifications = this.getCurrentNotifications();
    const filtered = currentNotifications.filter(n => !n.read);
    this.notificationsSubject.next(filtered);
    this.saveNotifications(filtered);
  }

  clearAll(): void {
    this.notificationsSubject.next([]);
    this.saveNotifications([]);
  }

  // Notification generators for specific events
  notifyTaskCompleted(taskTitle: string): void {
    this.addNotification({
      title: 'Task Completed',
      message: `Great job! You've completed "${taskTitle}".`,
      type: 'success',
      read: false
    });
  }

  notifyTaskOverdue(taskTitle: string, taskId: string): void {
    this.addNotification({
      title: 'Task Overdue',
      message: `The task "${taskTitle}" is now overdue.`,
      type: 'error',
      read: false,
      actionable: true,
      actionText: 'Update Task',
      actionUrl: `/dashboard/todos`,
      relatedEntityId: taskId,
      relatedEntityType: 'todo'
    });
  }

  notifyTaskDueSoon(taskTitle: string, dueDate: Date, taskId: string): void {
    const timeLeft = this.getTimeUntilDue(dueDate);
    this.addNotification({
      title: 'Task Due Soon',
      message: `"${taskTitle}" is due ${timeLeft}.`,
      type: 'warning',
      read: false,
      actionable: true,
      actionText: 'View Task',
      actionUrl: `/dashboard/todos`,
      relatedEntityId: taskId,
      relatedEntityType: 'todo'
    });
  }

  notifyWeeklySummary(completedCount: number, totalCount: number): void {
    const percentage = Math.round((completedCount / totalCount) * 100);
    this.addNotification({
      title: 'Weekly Summary',
      message: `You completed ${completedCount} out of ${totalCount} tasks this week (${percentage}%).`,
      type: 'info',
      read: false,
      actionable: true,
      actionText: 'View Dashboard',
      actionUrl: '/dashboard/home'
    });
  }

  notifySettingsChanged(settingType: string): void {
    this.addNotification({
      title: 'Settings Updated',
      message: `Your ${settingType} settings have been successfully updated.`,
      type: 'success',
      read: false
    });
  }

  private getTimeUntilDue(dueDate: Date): string {
    const now = new Date();
    const timeDiff = dueDate.getTime() - now.getTime();
    
    if (timeDiff <= 0) return 'now';
    
    const hours = Math.floor(timeDiff / (1000 * 60 * 60));
    const days = Math.floor(hours / 24);
    
    if (days > 0) return `in ${days} day${days > 1 ? 's' : ''}`;
    if (hours > 0) return `in ${hours} hour${hours > 1 ? 's' : ''}`;
    
    const minutes = Math.floor(timeDiff / (1000 * 60));
    return `in ${minutes} minute${minutes > 1 ? 's' : ''}`;
  }
}