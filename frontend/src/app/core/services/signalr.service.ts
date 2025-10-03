import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { NotificationService, AppNotification } from './notification.service';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private connectionState = new BehaviorSubject<string>('Disconnected');
  public connectionState$ = this.connectionState.asObservable();

  constructor(
    private notificationService: NotificationService,
    private authService: AuthService
  ) {}

  public async startConnection(): Promise<void> {
    try {
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(environment.signalR.notificationHub, {
          withCredentials: false
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

      this.setupEventHandlers();

      await this.hubConnection.start();
      this.connectionState.next('Connected');
      console.log('SignalR connection started successfully');

      // Join user group after connection
      const user = this.authService.getCurrentUser();
      if (user?.id) {
        await this.joinUserGroup(user.id);
      }

    } catch (error) {
      console.error('Error starting SignalR connection:', error);
      this.connectionState.next('Failed');
      throw error;
    }
  }

  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        this.connectionState.next('Disconnected');
        console.log('SignalR connection stopped');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    // Handle new notifications
    this.hubConnection.on('ReceiveNotification', (notification: AppNotification) => {
      console.log('Received new notification:', notification);
      this.notificationService.addNotificationToState(notification);
      
      // Show browser notification if permission granted
      this.showBrowserNotification(notification);
      
      // Play notification sound
      this.playNotificationSound();
    });

    // Handle notification count updates
    this.hubConnection.on('NotificationCountUpdate', (count: number) => {
      console.log('Notification count updated:', count);
      // The notification service will handle this update
    });

    // Handle connection events
    this.hubConnection.onreconnecting(() => {
      this.connectionState.next('Reconnecting');
      console.log('SignalR connection lost, attempting to reconnect...');
    });

    this.hubConnection.onreconnected(() => {
      this.connectionState.next('Connected');
      console.log('SignalR connection re-established');
      
      // Rejoin user group after reconnection
      const user = this.authService.getCurrentUser();
      if (user?.id) {
        this.joinUserGroup(user.id);
      }
    });

    this.hubConnection.onclose(() => {
      this.connectionState.next('Disconnected');
      console.log('SignalR connection closed');
    });
  }

  private async joinUserGroup(userId: string): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === 'Connected') {
      try {
        await this.hubConnection.invoke('JoinUserGroup', userId);
        console.log(`Joined user group: ${userId}`);
      } catch (error) {
        console.error('Error joining user group:', error);
      }
    }
  }

  private async leaveUserGroup(userId: string): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === 'Connected') {
      try {
        await this.hubConnection.invoke('LeaveUserGroup', userId);
        console.log(`Left user group: ${userId}`);
      } catch (error) {
        console.error('Error leaving user group:', error);
      }
    }
  }

  private showBrowserNotification(notification: AppNotification): void {
    if ('Notification' in window && Notification.permission === 'granted') {
      const browserNotification = new Notification(notification.title, {
        body: notification.message,
        icon: '/assets/icons/logo.svg',
        badge: '/assets/icons/logo.svg',
        tag: notification.id.toString(),
        requireInteraction: false,
        silent: false
      });

      browserNotification.onclick = () => {
        window.focus();
        browserNotification.close();
        // Navigate to notifications page or relevant page
        // You can add navigation logic here
      };

      // Auto close after 5 seconds
      setTimeout(() => {
        browserNotification.close();
      }, 5000);
    }
  }

  private playNotificationSound(): void {
    try {
      const audio = new Audio('/assets/sounds/notification.mp3');
      audio.volume = 0.3; // Set volume to 30%
      audio.play().catch(error => {
        console.log('Could not play notification sound:', error);
      });
    } catch (error) {
      console.log('Audio not supported or file not found:', error);
    }
  }

  public async requestNotificationPermission(): Promise<NotificationPermission> {
    if ('Notification' in window) {
      if (Notification.permission === 'default') {
        const permission = await Notification.requestPermission();
        return permission;
      }
      return Notification.permission;
    }
    return 'denied';
  }

  public getConnectionState(): Observable<string> {
    return this.connectionState$;
  }

  public isConnected(): boolean {
    return this.hubConnection?.state === 'Connected';
  }

  // Method to handle user login
  public async onUserLogin(userId: string): Promise<void> {
    if (this.isConnected()) {
      await this.joinUserGroup(userId);
    }
  }

  // Method to handle user logout
  public async onUserLogout(userId: string): Promise<void> {
    if (this.isConnected()) {
      await this.leaveUserGroup(userId);
    }
  }
}