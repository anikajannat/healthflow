import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  standalone: true, imports: [ReactiveFormsModule], templateUrl: './register.component.html'
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  sent = false; error = ''; message = ''; loading = false;
  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    otp: ['']
  });

  sendOtp() {
    const email = this.form.value.email;
    if (!email) return;
    this.auth.sendOtp(email, 'Registration').subscribe({
      next: () => { this.sent = true; this.message = 'OTP sent. Check email (or API console in local dev).'; },
      error: e => this.error = e.error?.message || 'Could not send OTP.'
    });
  }

  submit() {
    if (!this.sent || this.form.invalid || !this.form.value.otp) return;
    this.loading = true; this.error = '';
    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => this.router.navigateByUrl('/patient'),
      error: e => { this.error = e.error?.message || 'Registration failed.'; this.loading = false; }
    });
  }
}
