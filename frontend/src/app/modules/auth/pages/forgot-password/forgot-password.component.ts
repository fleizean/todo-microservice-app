import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ButtonComponent } from 'src/app/shared/components/button/button.component';
import { AuthService } from 'src/app/core/services/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
  imports: [FormsModule, RouterLink, ButtonComponent, CommonModule],
})
export class ForgotPasswordComponent implements OnInit {
  email = '';
  isLoading = false;
  message = '';
  isSuccess = false;

  constructor(private authService: AuthService) {}

  ngOnInit(): void {}

  onSubmit(): void {
    if (!this.email) {
      this.message = 'Please enter your email address';
      this.isSuccess = false;
      return;
    }

    this.isLoading = true;
    this.message = '';

    this.authService.forgotPassword(this.email).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.message = response.message || 'If the email exists, a password reset link has been sent';
        this.isSuccess = true;
      },
      error: (error) => {
        this.isLoading = false;
        this.message = error.error?.message || 'An error occurred. Please try again.';
        this.isSuccess = false;
      }
    });
  }
}
