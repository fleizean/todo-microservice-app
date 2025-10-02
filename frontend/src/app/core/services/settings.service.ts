import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserProfile {
  id: string;
  username: string;
  email: string;
  fullName: string;
  avatar?: string;
}

export interface NotificationSettings {
  emailNotifications: boolean;
  pushNotifications: boolean;
  reminderNotifications: boolean;
  weeklyDigest: boolean;
  dailyDigest: boolean;
  taskAssignments: boolean;
  taskDeadlines: boolean;
}

export interface ThemeSettings {
  darkMode: boolean;
  compactMode: boolean;
  primaryColor: string;
  fontSize: 'small' | 'medium' | 'large';
}

export interface AppSettings {
  userProfile: UserProfile;
  notifications: NotificationSettings;
  theme: ThemeSettings;
}

@Injectable({
  providedIn: 'root'
})
export class SettingsService {
  private readonly API_URL = environment.apiUrl.auth;
  private readonly SETTINGS_KEY = 'app_settings';
  
  private settingsSubject = new BehaviorSubject<AppSettings>(this.getDefaultSettings());
  public settings$ = this.settingsSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadSettings();
  }

  private getDefaultSettings(): AppSettings {
    return {
      userProfile: {
        id: '',
        username: '',
        email: '',
        fullName: '',
      },
      notifications: {
        emailNotifications: true,
        pushNotifications: false,
        reminderNotifications: true,
        weeklyDigest: true,
        dailyDigest: false,
        taskAssignments: true,
        taskDeadlines: true
      },
      theme: {
        darkMode: false,
        compactMode: false,
        primaryColor: '#3B82F6',
        fontSize: 'medium'
      }
    };
  }

  loadSettings(): void {
    // First try to load from localStorage
    const storedSettings = localStorage.getItem(this.SETTINGS_KEY);
    if (storedSettings) {
      try {
        const settings = JSON.parse(storedSettings);
        this.settingsSubject.next(settings);
        return;
      } catch (error) {
        console.error('Error parsing stored settings:', error);
      }
    }

    // Load user data from auth service if available
    const userData = localStorage.getItem('user_data');
    if (userData) {
      try {
        const user = JSON.parse(userData);
        const settings = this.getDefaultSettings();
        settings.userProfile = {
          ...settings.userProfile,
          username: user.username || '',
          email: user.email || '',
          fullName: user.fullName || user.username || '',
          id: user.id || ''
        };
        this.settingsSubject.next(settings);
        this.saveSettings(settings);
      } catch (error) {
        console.error('Error loading user data:', error);
      }
    }
  }

  getCurrentSettings(): AppSettings {
    return this.settingsSubject.value;
  }

  updateUserProfile(profile: Partial<UserProfile>): Observable<boolean> {
    const currentSettings = this.getCurrentSettings();
    const updatedSettings = {
      ...currentSettings,
      userProfile: { ...currentSettings.userProfile, ...profile }
    };
    
    return new Observable(observer => {
      try {
        // In a real app, this would make an HTTP request to update the backend
        // return this.http.put<boolean>(`${this.API_URL}/profile`, profile);
        
        this.settingsSubject.next(updatedSettings);
        this.saveSettings(updatedSettings);
        
        // Also update user data in localStorage for auth service
        const userData = {
          ...JSON.parse(localStorage.getItem('user_data') || '{}'),
          username: updatedSettings.userProfile.username,
          email: updatedSettings.userProfile.email,
          fullName: updatedSettings.userProfile.fullName
        };
        localStorage.setItem('user_data', JSON.stringify(userData));
        
        observer.next(true);
        observer.complete();
      } catch (error) {
        observer.error(error);
      }
    });
  }

  updateNotificationSettings(notifications: Partial<NotificationSettings>): Observable<boolean> {
    const currentSettings = this.getCurrentSettings();
    const updatedSettings = {
      ...currentSettings,
      notifications: { ...currentSettings.notifications, ...notifications }
    };
    
    return new Observable(observer => {
      try {
        this.settingsSubject.next(updatedSettings);
        this.saveSettings(updatedSettings);
        observer.next(true);
        observer.complete();
      } catch (error) {
        observer.error(error);
      }
    });
  }

  updateThemeSettings(theme: Partial<ThemeSettings>): Observable<boolean> {
    const currentSettings = this.getCurrentSettings();
    const updatedSettings = {
      ...currentSettings,
      theme: { ...currentSettings.theme, ...theme }
    };
    
    return new Observable(observer => {
      try {
        this.settingsSubject.next(updatedSettings);
        this.saveSettings(updatedSettings);
        this.applyTheme(updatedSettings.theme);
        observer.next(true);
        observer.complete();
      } catch (error) {
        observer.error(error);
      }
    });
  }

  private saveSettings(settings: AppSettings): void {
    try {
      localStorage.setItem(this.SETTINGS_KEY, JSON.stringify(settings));
    } catch (error) {
      console.error('Error saving settings:', error);
    }
  }

  private applyTheme(theme: ThemeSettings): void {
    // Apply dark mode
    if (theme.darkMode) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }

    // Apply compact mode
    if (theme.compactMode) {
      document.documentElement.classList.add('compact');
    } else {
      document.documentElement.classList.remove('compact');
    }

    // Apply font size
    document.documentElement.classList.remove('text-sm', 'text-base', 'text-lg');
    switch (theme.fontSize) {
      case 'small':
        document.documentElement.classList.add('text-sm');
        break;
      case 'large':
        document.documentElement.classList.add('text-lg');
        break;
      default:
        document.documentElement.classList.add('text-base');
    }
  }

  exportSettings(): string {
    const settings = this.getCurrentSettings();
    return JSON.stringify(settings, null, 2);
  }

  importSettings(settingsJson: string): Observable<boolean> {
    return new Observable(observer => {
      try {
        const settings = JSON.parse(settingsJson);
        this.settingsSubject.next(settings);
        this.saveSettings(settings);
        this.applyTheme(settings.theme);
        observer.next(true);
        observer.complete();
      } catch (error) {
        observer.error(error);
      }
    });
  }

  resetToDefaults(): Observable<boolean> {
    return new Observable(observer => {
      try {
        const defaultSettings = this.getDefaultSettings();
        // Keep user profile data
        const currentSettings = this.getCurrentSettings();
        defaultSettings.userProfile = currentSettings.userProfile;
        
        this.settingsSubject.next(defaultSettings);
        this.saveSettings(defaultSettings);
        this.applyTheme(defaultSettings.theme);
        observer.next(true);
        observer.complete();
      } catch (error) {
        observer.error(error);
      }
    });
  }
}