import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { Subject, takeUntil } from 'rxjs';
import { SettingsService, AppSettings, UserProfile, NotificationSettings, ThemeSettings } from '../../../../core/services/settings.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule, FormsModule, AngularSvgIconModule],
  templateUrl: './settings-page.component.html',
  styleUrls: ['./settings-page.component.scss']
})
export class SettingsPageComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  
  loading = false;
  saveSuccess = false;
  saveError = '';

  userProfile: UserProfile = {
    id: '',
    username: '',
    email: '',
    fullName: '',
  };

  notifications: NotificationSettings = {
    emailNotifications: true,
    pushNotifications: false,
    reminderNotifications: true,
    weeklyDigest: true,
    dailyDigest: false,
    taskAssignments: true,
    taskDeadlines: true
  };

  theme: ThemeSettings = {
    darkMode: false,
    compactMode: false,
    primaryColor: '#3B82F6',
    fontSize: 'medium'
  };

  fontSizes = [
    { value: 'small', label: 'Small' },
    { value: 'medium', label: 'Medium' },
    { value: 'large', label: 'Large' }
  ];

  constructor(
    private settingsService: SettingsService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadSettings();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadSettings() {
    this.settingsService.settings$
      .pipe(takeUntil(this.destroy$))
      .subscribe(settings => {
        this.userProfile = { ...settings.userProfile };
        this.notifications = { ...settings.notifications };
        this.theme = { ...settings.theme };
      });

    // Also load avatar from auth service
    const userData = this.authService.getUserData();
    if (userData && userData.avatarUrl) {
      this.userProfile.avatar = userData.avatarUrl;
    }
  }

  onSaveProfile() {
    this.loading = true;
    this.saveError = '';
    
    this.settingsService.updateUserProfile(this.userProfile)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (success) => {
          this.loading = false;
          if (success) {
            this.showSuccess('Profile updated successfully!');
          }
        },
        error: (error) => {
          this.loading = false;
          this.saveError = 'Failed to update profile. Please try again.';
          console.error('Profile update error:', error);
        }
      });
  }

  onSaveNotifications() {
    this.loading = true;
    this.saveError = '';
    
    this.settingsService.updateNotificationSettings(this.notifications)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (success) => {
          this.loading = false;
          if (success) {
            this.showSuccess('Notification settings updated successfully!');
          }
        },
        error: (error) => {
          this.loading = false;
          this.saveError = 'Failed to update notification settings. Please try again.';
          console.error('Notification settings update error:', error);
        }
      });
  }

  onSaveTheme() {
    this.loading = true;
    this.saveError = '';
    
    this.settingsService.updateThemeSettings(this.theme)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (success) => {
          this.loading = false;
          if (success) {
            this.showSuccess('Theme settings updated successfully!');
          }
        },
        error: (error) => {
          this.loading = false;
          this.saveError = 'Failed to update theme settings. Please try again.';
          console.error('Theme settings update error:', error);
        }
      });
  }

  onSaveAllChanges() {
    this.loading = true;
    this.saveError = '';
    
    // Save all settings sequentially
    this.settingsService.updateUserProfile(this.userProfile)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.settingsService.updateNotificationSettings(this.notifications)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => {
                this.settingsService.updateThemeSettings(this.theme)
                  .pipe(takeUntil(this.destroy$))
                  .subscribe({
                    next: () => {
                      this.loading = false;
                      this.showSuccess('All settings updated successfully!');
                    },
                    error: (error) => {
                      this.loading = false;
                      this.saveError = 'Failed to update theme settings.';
                      console.error('Theme update error:', error);
                    }
                  });
              },
              error: (error) => {
                this.loading = false;
                this.saveError = 'Failed to update notification settings.';
                console.error('Notification update error:', error);
              }
            });
        },
        error: (error) => {
          this.loading = false;
          this.saveError = 'Failed to update profile.';
          console.error('Profile update error:', error);
        }
      });
  }

  onExportSettings() {
    try {
      const settingsJson = this.settingsService.exportSettings();
      const blob = new Blob([settingsJson], { type: 'application/json' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `tasky-settings-${new Date().toISOString().split('T')[0]}.json`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
      this.showSuccess('Settings exported successfully!');
    } catch (error) {
      this.saveError = 'Failed to export settings.';
      console.error('Export error:', error);
    }
  }

  onImportSettings(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const settingsJson = e.target?.result as string;
        this.settingsService.importSettings(settingsJson)
          .pipe(takeUntil(this.destroy$))
          .subscribe({
            next: () => {
              this.showSuccess('Settings imported successfully!');
              this.loadSettings();
            },
            error: (error) => {
              this.saveError = 'Failed to import settings. Invalid file format.';
              console.error('Import error:', error);
            }
          });
      } catch (error) {
        this.saveError = 'Failed to read settings file.';
        console.error('File read error:', error);
      }
    };
    reader.readAsText(file);
  }

  onResetToDefaults() {
    if (confirm('Are you sure you want to reset all settings to defaults? This action cannot be undone.')) {
      this.loading = true;
      this.settingsService.resetToDefaults()
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loading = false;
            this.showSuccess('Settings reset to defaults successfully!');
            this.loadSettings();
          },
          error: (error) => {
            this.loading = false;
            this.saveError = 'Failed to reset settings.';
            console.error('Reset error:', error);
          }
        });
    }
  }

  onAvatarFileSelected(event: any) {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
    if (!allowedTypes.includes(file.type.toLowerCase())) {
      this.saveError = 'Only image files (JPEG, PNG, GIF) are allowed';
      return;
    }

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      this.saveError = 'File size cannot exceed 5MB';
      return;
    }

    this.loading = true;
    this.saveError = '';

    this.authService.uploadAvatar(file)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.loading = false;
          if (response.avatarUrl) {
            this.userProfile.avatar = response.avatarUrl;
            this.authService.updateUserData({ avatarUrl: response.avatarUrl });
            this.showSuccess('Avatar uploaded successfully!');
          }
        },
        error: (error) => {
          this.loading = false;
          this.saveError = error.error?.message || 'Failed to upload avatar. Please try again.';
          console.error('Avatar upload error:', error);
        }
      });
  }

  getUserInitial(): string {
    return (this.userProfile?.fullName && this.userProfile.fullName.length > 0) 
      ? this.userProfile.fullName.charAt(0).toUpperCase() 
      : (this.userProfile?.username && this.userProfile.username.length > 0)
        ? this.userProfile.username.charAt(0).toUpperCase()
        : 'U';
  }

  private showSuccess(message: string) {
    this.saveSuccess = true;
    this.saveError = '';
    setTimeout(() => {
      this.saveSuccess = false;
    }, 3000);
  }
}