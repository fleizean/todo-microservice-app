import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();
  
  const skipAuth = req.url.includes('/auth/login') || req.url.includes('/auth/register');
  
  // Eğer istek gövdesi FormData ise, Content-Type'a dokunma.
  if (req.body instanceof FormData) {
    if (token && !skipAuth) {
      // Sadece Authorization başlığını ekle.
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
  } 
  // Diğer tüm istekler için eski mantığı kullan.
  else {
    if (token && !skipAuth) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });
    } else if (!skipAuth) {
      req = req.clone({
        setHeaders: {
          'Content-Type': 'application/json'
        }
      });
    }
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !skipAuth) {
        authService.logout();
      }
      return throwError(() => error);
    })
  );
};