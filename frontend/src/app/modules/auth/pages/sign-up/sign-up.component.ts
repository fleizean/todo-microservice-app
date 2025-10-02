import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../../core/services/auth.service';
import { RegisterRequest } from '../../../../core/models/auth.model';

// Button ve SVG icon component'lerini import edin
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { AngularSvgIconModule } from 'angular-svg-icon';

@Component({
  selector: 'app-sign-up',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    ButtonComponent,      // app-button için
    AngularSvgIconModule  // svg-icon için
  ],
  templateUrl: './sign-up.component.html',
  styleUrls: ['./sign-up.component.css']
})
export class SignUpComponent implements OnInit {
  signUpForm!: FormGroup;
  isLoading = false;
  showPassword = false;
  showConfirmPassword = false;
  errorMessage = '';
  successMessage = '';

  showTermsModal = false;
  showPrivacyModal = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Form'u her zaman initialize et
    this.initForm();

    // Zaten giriş yapmışsa dashboard'a yönlendir
    const isAuth = this.authService.isAuthenticated();
    console.log('Sign-up: isAuthenticated =', isAuth);
    if (isAuth) {
      this.router.navigate(['/dashboard/todos']);
      return;
    }
  }

  private initForm(): void {
    this.signUpForm = this.fb.group({
      fullname: ['', [Validators.required, Validators.minLength(3)]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      acceptTerms: [false, [Validators.requiredTrue]]
    }, { validators: this.passwordMatchValidator });
  }

  private passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      return { passwordMismatch: true };
    }
    return null;
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  getPasswordStrength(): number {
    if (!this.signUpForm) return 0;
    const password = this.signUpForm.get('password')?.value || '';
    let strength = 0;
    
    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^A-Za-z0-9]/.test(password)) strength++;

    return Math.min(strength, 4);
  }

  onSubmit(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.signUpForm || this.signUpForm.invalid) {
      this.markFormGroupTouched();
      this.errorMessage = 'Lütfen tüm alanları doğru şekilde doldurun.';
      return;
    }

    this.isLoading = true;

    const registerData: RegisterRequest = {
      fullName: this.signUpForm.value.fullname,
      username: this.signUpForm.value.username,
      email: this.signUpForm.value.email,
      password: this.signUpForm.value.password
    };


    this.authService.register(registerData).subscribe({
      next: (response) => {
        this.successMessage = 'Kayıt başarılı! Giriş sayfasına yönlendiriliyorsunuz...';
        
        setTimeout(() => {
          this.router.navigate(['/auth/sign-in']);
        }, 2000);
      },
      error: (error) => {
        
        if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else if (error.status === 0) {
          this.errorMessage = 'Sunucuya bağlanılamıyor.';
        } else {
          this.errorMessage = 'Kayıt sırasında bir hata oluştu.';
        }
        
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  private markFormGroupTouched(): void {
    if (!this.signUpForm) return;
    Object.keys(this.signUpForm.controls).forEach(key => {
      const control = this.signUpForm.get(key);
      control?.markAsTouched();
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    if (!this.signUpForm) return false;
    const field = this.signUpForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(fieldName: string): string {
    if (!this.signUpForm) return '';
    const field = this.signUpForm.get(fieldName);
    if (field?.errors) {
      if (field.errors['required']) return `Bu alan gereklidir`;
      if (field.errors['email']) return 'Geçerli bir email adresi girin';
      if (field.errors['minlength']) return `En az ${field.errors['minlength'].requiredLength} karakter`;
      if (field.errors['requiredTrue']) return 'Şartları kabul etmelisiniz';
    }
    return '';
  }

  openTermsModal(): void {
    this.showTermsModal = true;
  }

  closeTermsModal(): void {
    this.showTermsModal = false;
  }

  openPrivacyModal(): void {
    this.showPrivacyModal = true;
  }

  closePrivacyModal(): void {
    this.showPrivacyModal = false;
  }

  onBackdropClick(event: Event): void {
    if (event.target === event.currentTarget) {
      this.closeTermsModal();
      this.closePrivacyModal();
    }
  }
}