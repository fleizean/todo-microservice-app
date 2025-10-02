import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { ButtonComponent } from 'src/app/shared/components/button/button.component';
import { AuthService } from 'src/app/core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-new-password',
  templateUrl: './new-password.component.html',
  styleUrls: ['./new-password.component.css'],
  imports: [FormsModule, RouterLink, AngularSvgIconModule, ButtonComponent, CommonModule],
})
export class NewPasswordComponent implements OnInit {
  password = '';
  confirmPassword = '';
  email = '';
  token = '';
  isLoading = false;
  message = '';
  isSuccess = false;
  showPassword = false;
  showConfirmPassword = false;

  constructor(
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      this.token = params['token'] || '';
      
      if (!this.email || !this.token) {
        this.message = 'Invalid reset link. Please request a new password reset.';
        this.isSuccess = false;
      }
    });
  }

  onSubmit(): void {
    if (!this.password || !this.confirmPassword) {
      this.message = 'Please fill in all fields';
      this.isSuccess = false;
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.message = 'Passwords do not match';
      this.isSuccess = false;
      return;
    }

    if (this.password.length < 8) {
      this.message = 'Password must be at least 8 characters long';
      this.isSuccess = false;
      return;
    }

    this.isLoading = true;
    this.message = '';

    this.authService.resetPassword(this.email, this.token, this.password).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.message = response.message || 'Password has been reset successfully';
        this.isSuccess = true;
        setTimeout(() => {
          this.router.navigate(['/auth/sign-in']);
        }, 3000);
      },
      error: (error) => {
        this.isLoading = false;
        this.message = error.error?.message || 'An error occurred. Please try again.';
        this.isSuccess = false;
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }
}
