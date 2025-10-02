import { animate, style, transition, trigger } from '@angular/animations';
import { NgClass, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { AppUser } from '../../../../../core/models/auth.model';
import { AuthService } from '../../../../../core/services/auth.service';
import { ThemeService } from '../../../../../core/services/theme.service';
import { ClickOutsideDirective } from '../../../../../shared/directives/click-outside.directive';
import { Subscription } from 'rxjs';
@Component({
  selector: 'app-profile-menu',
  standalone: true,
  templateUrl: './profile-menu.component.html',
  styleUrls: ['./profile-menu.component.css'],
  imports: [ClickOutsideDirective, NgClass, NgIf, RouterLink, AngularSvgIconModule],
  animations: [
    trigger('openClose', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-20px)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0, transform: 'translateY(-20px)' })),
      ]),
    ]),
  ],
})
export class ProfileMenuComponent implements OnInit {
  public isOpen = false;
  public user: AppUser | null = null;
  private userSubscription: Subscription | undefined; // Subscription'ı tutmak için

  // Template'de kullanılan menü ve tema verilerini burada tanımlayın
  public profileMenu = [
    { title: 'Your Profile', icon: 'assets/icons/heroicons/outline/user-circle.svg', link: '/dashboard/profile' },
    { title: 'Settings', icon: 'assets/icons/heroicons/outline/cog.svg', link: '/dashboard/settings' },
    { title: 'Notifications', icon: 'assets/icons/heroicons/outline/bell.svg', link: '/dashboard/notifications' },
    { title: 'Log out', icon: 'assets/icons/heroicons/outline/logout.svg', link: '/auth' },
  ];

  public themeColors = [
    { name: 'base', code: 'hsl(222.2 47.4% 11.2%)' },
    { name: 'yellow', code: '#f59e0b' },
    { name: 'green', code: '#22c55e' },
    { name: 'blue', code: '#3b82f6' },
    { name: 'orange', code: '#ea580c' },
    { name: 'red', code: '#cc0022' },
    { name: 'violet', code: '#6d28d9' },
  ];

  public themeMode = ['light', 'dark'];

  constructor(
    private authService: AuthService,
    public themeService: ThemeService, // Template'de kullanıldığı için public olmalı
    private router: Router
  ) {}

  ngOnInit(): void {
    // Kullanıcı verisini localStorage'dan senkron olarak al
    this.userSubscription = this.authService.currentUser$.subscribe(user => {
      this.user = user;
    });
  }

  ngOnDestroy(): void {
    // Component yok olduğunda memory leak önlemek için aboneliği sonlandır
    this.userSubscription?.unsubscribe();
  }

  public toggleMenu(event: Event): void {
    event.stopPropagation(); // Tıklama olayının yayılmasını engelle
    this.isOpen = !this.isOpen;
  }

  public closeMenu(): void {
    this.isOpen = false;
  }

  public logout(): void {
    this.authService.logout();
    this.closeMenu();
  }

  public getUserData(): AppUser | null {
    const userString = localStorage.getItem('user');
    if (userString) {
      return JSON.parse(userString) as AppUser;
    }
    return null;
  }

  public getUserInitial(): string {
    return this.user?.username ? this.user.username.charAt(0).toUpperCase() : 'U';
  }

  public toggleThemeColor(color: string): void {
    this.themeService.setThemeColor(color);
  }

  public toggleThemeMode(): void {
    this.themeService.toggleThemeMode();
  }
}