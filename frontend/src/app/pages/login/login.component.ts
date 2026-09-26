import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  standalone: true, imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);
  error = ''; loading = false;
  form = this.fb.nonNullable.group({ email: ['', [Validators.required, Validators.email]], password: ['', Validators.required] });

  submit() {
    if (this.form.invalid) return;
    this.loading = true; this.error = '';
    this.auth.login(this.form.value.email!, this.form.value.password!).subscribe({
      next: r => {
        const role = r.user.role;
        this.router.navigateByUrl(role === 'Patient' ? '/patient' : role === 'Doctor' ? '/doctor' : '/admin');
      },
      error: e => { this.error = e.error?.message || 'Login failed.'; this.loading = false; }
    });
  }
}
