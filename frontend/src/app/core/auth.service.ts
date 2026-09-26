import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { ApiService } from './api.service';
import { AuthResponse, User } from './models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  user = signal<User | null>(this.readUser());

  constructor(private api: ApiService, private router: Router) {}

  login(email: string, password: string) {
    return this.api.post<AuthResponse>('/auth/login', { email, password }).pipe(tap(r => this.save(r)));
  }

  register(payload: unknown) {
    return this.api.post<AuthResponse>('/auth/register', payload).pipe(tap(r => this.save(r)));
  }

  sendOtp(email: string, purpose: string) {
    return this.api.post('/auth/send-otp', { email, purpose });
  }

  loadMe() {
    return this.api.get<User>('/auth/me').pipe(tap(u => {
      this.user.set(u); localStorage.setItem('hf_user', JSON.stringify(u));
    }));
  }

  token() { return localStorage.getItem('hf_access'); }
  role() { return this.user()?.role; }
  isLoggedIn() { return !!this.token(); }

  logout() {
    localStorage.removeItem('hf_access'); localStorage.removeItem('hf_refresh'); localStorage.removeItem('hf_user');
    this.user.set(null); this.router.navigateByUrl('/');
  }

  private save(r: AuthResponse) {
    localStorage.setItem('hf_access', r.accessToken);
    localStorage.setItem('hf_refresh', r.refreshToken);
    localStorage.setItem('hf_user', JSON.stringify(r.user));
    this.user.set(r.user);
  }

  private readUser(): User | null {
    try { return JSON.parse(localStorage.getItem('hf_user') || 'null'); } catch { return null; }
  }
}
