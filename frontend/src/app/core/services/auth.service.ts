// src/app/core/services/auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegisterRequest, AuthResponse, AppUser } from '../models/auth.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl.auth;
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'user_data';

  private currentUserSubject = new BehaviorSubject<AppUser | null>(this.getUserData());
  public currentUser$ = this.currentUserSubject.asObservable();
  public user$ = this.currentUser$; // Alias for compatibility

  private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasToken());
  public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  login(loginData: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/login`, loginData)
      .pipe(
        tap(response => {
          this.setAuthData(response);
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.isAuthenticatedSubject.next(false);
    this.router.navigate(['/auth/sign-in']);
  }

  register(registerData: RegisterRequest): Observable<boolean> {
    return this.http.post<boolean>(`${this.API_URL}/register`, registerData);
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.API_URL}/forgot-password`, { email });
  }

  resetPassword(email: string, token: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.API_URL}/reset-password`, { 
      email, 
      token, 
      newPassword 
    });
  }

  

  isAuthenticated(): boolean {
    const hasToken = this.hasToken();
    const tokenValid = !this.isTokenExpired();
    const isAuth = hasToken && tokenValid;
        
    return isAuth;
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getUserData(): AppUser | null {
    const userData = localStorage.getItem(this.USER_KEY);
    if (!userData) return null;
    
    const user = JSON.parse(userData);
    
    // If user doesn't have ID, try to get it from JWT token
    if (!user.id) {
      const token = this.getToken();
      if (token) {
        try {
          const payload = JSON.parse(atob(token.split('.')[1]));
          user.id = payload.nameid; // JWT'de user ID 'nameid' field'inde
        } catch (error) {
          console.error('Failed to decode JWT token:', error);
        }
      }
    }
    
    return user;
  }

  getCurrentUser(): AppUser | null {
    // Get from BehaviorSubject first, but ensure ID is populated from JWT if missing
    let user = this.currentUserSubject.value;
    
    // If user exists but no ID, get it from localStorage which now includes JWT decode
    if (user && !user.id) {
      user = this.getUserData();
      if (user) {
        this.currentUserSubject.next(user); // Update BehaviorSubject with complete user data
      }
    }
    
    return user;
  }

  updateUserData(userData: Partial<AppUser>): void {
    const currentUser = this.getUserData();
    if (currentUser) {
      const updatedUser: AppUser = { ...currentUser, ...userData };
      localStorage.setItem(this.USER_KEY, JSON.stringify(updatedUser));
      this.currentUserSubject.next(updatedUser); // Değişikliği yayınla
    }
  }

  uploadAvatar(file: File): Observable<AppUser> {
    const formData = new FormData();
    formData.append('file', file);
    
    // Backend'den dönen güncel kullanıcı bilgisini yakala ve güncelle
    return this.http.post<AppUser>(`${this.API_URL}/upload-avatar`, formData).pipe(
      tap(updatedUser => {
        this.updateUserData(updatedUser); // Kullanıcı verisini güncelle ve yayınla
      })
    );
  }

  private setAuthData(authResponse: AuthResponse): void {
    const appUser: AppUser = {
      id: authResponse.id,
      username: authResponse.username,
      email: authResponse.email,
      fullName: authResponse.fullName,
      avatarUrl: authResponse.avatarUrl
    };
    
    localStorage.setItem(this.TOKEN_KEY, authResponse.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(appUser));
    this.isAuthenticatedSubject.next(true);
    this.currentUserSubject.next(appUser); // Yeni kullanıcıyı yayınla
  }

  private hasToken(): boolean {
    return !!localStorage.getItem(this.TOKEN_KEY);
  }

  private isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const now = Math.floor(Date.now() / 1000);
      return payload.exp < now;
    } catch {
      return true;
    }
  }
}