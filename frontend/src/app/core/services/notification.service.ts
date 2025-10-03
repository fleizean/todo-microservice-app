import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface AppNotification {
  id: number;
  userId: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: Date;
  readAt?: Date;
  data?: string;
}

export enum NotificationType {
  TodoCreated = 1,
  TodoCompleted = 2,
  TodoDeleted = 3,
  System = 4
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly API_URL = `${environment.apiUrl.notification}/notification`;
  
  private notificationsSubject = new BehaviorSubject<AppNotification[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();
  
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  constructor(private http: HttpClient) {
    // Initialize will be called when user is authenticated
  }

  initializeForUser(userId: string): void {
    this.loadUserNotifications(userId);
    this.loadUnreadCount(userId);
  }

  loadUserNotifications(userId: string, page: number = 1, pageSize: number = 20, isRead?: boolean, type?: NotificationType): Observable<AppNotification[]> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    
    if (isRead !== undefined) {
      params = params.set('isRead', isRead.toString());
    }
    
    if (type !== undefined) {
      params = params.set('type', type.toString());
    }

    return this.http.get<AppNotification[]>(`${this.API_URL}/${userId}`, { params });
  }

  loadUnreadCount(userId: string): void {
    this.http.get<{count: number}>(`${this.API_URL}/${userId}/unread-count`)
      .subscribe(response => {
        this.unreadCountSubject.next(response.count);
      });
  }

  getNotifications(): Observable<AppNotification[]> {
    return this.notifications$;
  }

  getUnreadCount(): Observable<number> {
    return this.unreadCount$;
  }

  markAsRead(notificationId: number): Observable<any> {
    return this.http.post(`${this.API_URL}/${notificationId}/read`, {});
  }

  markAllAsRead(userId: string): Observable<any> {
    return this.http.post(`${this.API_URL}/${userId}/read-all`, {});
  }

  deleteNotification(notificationId: number): Observable<any> {
    return this.http.delete(`${this.API_URL}/${notificationId}`);
  }

  // Local state management methods
  addNotificationToState(notification: AppNotification): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = [notification, ...currentNotifications];
    this.notificationsSubject.next(updatedNotifications);
    
    // Update unread count if notification is unread
    if (!notification.isRead) {
      const currentCount = this.unreadCountSubject.value;
      this.unreadCountSubject.next(currentCount + 1);
    }
  }

  updateNotificationInState(notificationId: number, updates: Partial<AppNotification>): void {
    const currentNotifications = this.notificationsSubject.value;
    const updatedNotifications = currentNotifications.map(notification => 
      notification.id === notificationId 
        ? { ...notification, ...updates }
        : notification
    );
    this.notificationsSubject.next(updatedNotifications);
    
    // Update unread count if read status changed
    if (updates.isRead !== undefined) {
      const wasUnread = currentNotifications.find(n => n.id === notificationId)?.isRead === false;
      const nowRead = updates.isRead === true;
      
      if (wasUnread && nowRead) {
        const currentCount = this.unreadCountSubject.value;
        this.unreadCountSubject.next(Math.max(0, currentCount - 1));
      }
    }
  }

  removeNotificationFromState(notificationId: number): void {
    const currentNotifications = this.notificationsSubject.value;
    const notification = currentNotifications.find(n => n.id === notificationId);
    const filteredNotifications = currentNotifications.filter(n => n.id !== notificationId);
    this.notificationsSubject.next(filteredNotifications);
    
    // Update unread count if deleted notification was unread
    if (notification && !notification.isRead) {
      const currentCount = this.unreadCountSubject.value;
      this.unreadCountSubject.next(Math.max(0, currentCount - 1));
    }
  }

  // Utility methods
  getNotificationTypeLabel(type: NotificationType): string {
    switch (type) {
      case NotificationType.TodoCreated:
        return 'Todo Created';
      case NotificationType.TodoCompleted:
        return 'Todo Completed';
      case NotificationType.TodoDeleted:
        return 'Todo Deleted';
      case NotificationType.System:
        return 'System';
      default:
        return 'Unknown';
    }
  }

  getNotificationTypeIcon(type: NotificationType): string {
    switch (type) {
      case NotificationType.TodoCreated:
        return 'plus';
      case NotificationType.TodoCompleted:
        return 'check';
      case NotificationType.TodoDeleted:
        return 'trash';
      case NotificationType.System:
        return 'info';
      default:
        return 'bell';
    }
  }

  formatNotificationTime(createdAt: Date): string {
    const now = new Date();
    const notificationDate = new Date(createdAt);
    const timeDiff = now.getTime() - notificationDate.getTime();
    
    const minutes = Math.floor(timeDiff / (1000 * 60));
    const hours = Math.floor(minutes / 60);
    const days = Math.floor(hours / 24);
    
    if (days > 0) return `${days} day${days > 1 ? 's' : ''} ago`;
    if (hours > 0) return `${hours} hour${hours > 1 ? 's' : ''} ago`;
    if (minutes > 0) return `${minutes} minute${minutes > 1 ? 's' : ''} ago`;
    
    return 'Just now';
  }
}