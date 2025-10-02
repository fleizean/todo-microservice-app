import { NgClass, NgIf } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AngularSvgIconModule } from 'angular-svg-icon';
import { ButtonComponent } from '../../../../shared/components/button/button.component';
import { AuthService } from '../../../../core/services/auth.service';
import { LoginRequest } from '../../../../core/models/auth.model';

@Component({
  selector: 'app-sign-in',
  standalone: true,
  templateUrl: './sign-in.component.html',
  styleUrls: ['./sign-in.component.css'],
  imports: [FormsModule, ReactiveFormsModule, RouterLink, AngularSvgIconModule, NgIf, ButtonComponent, NgClass],
})
export class SignInComponent implements OnInit {
  form!: FormGroup;
  submitted = false;
  passwordTextType = false;
  isLoading = false;
  errorMessage = '';

  constructor(
    private readonly _formBuilder: FormBuilder, 
    private readonly _router: Router,
    private readonly authService: AuthService
  ) {}

  ngOnInit(): void {
    // Form'u her zaman initialize et
    this.form = this._formBuilder.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });

    // Zaten giriş yapmışsa dashboard'a yönlendir
    if (this.authService.isAuthenticated()) {
      this._router.navigate(['/dashboard/todos']);
      return;
    }
  }

  get f() {
    return this.form?.controls || {};
  }

  togglePasswordTextType() {
    this.passwordTextType = !this.passwordTextType;
  }

  onSubmit() {
    this.submitted = true;
    this.errorMessage = '';

    if (!this.form || this.form.invalid) {
      return;
    }

    this.isLoading = true;

    const loginData: LoginRequest = {
      email: this.form.value.email,
      password: this.form.value.password
    };


    this.authService.login(loginData).subscribe({
      next: (response) => {
        console.log('Login successful, response:', response);
        console.log('isAuthenticated after login:', this.authService.isAuthenticated());
        this._router.navigate(['/dashboard/todos']);
      },
      error: (error) => {
        
        if (error.status === 401) {
          this.errorMessage = 'E-posta veya şifre hatalı.';
        } else if (error.status === 0) {
          this.errorMessage = 'Sunucuya bağlanılamıyor.';
        } else if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else {
          this.errorMessage = 'Giriş yapılırken bir hata oluştu.';
        }
        
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
}