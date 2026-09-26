import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

declare global {
  interface Window { __env?: { API_URL?: string }; }
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = window.__env?.API_URL || 'http://localhost:5080/api/v1';

  constructor(private http: HttpClient) {}

  get<T>(path: string, params?: Record<string, string>): Observable<T> {
    let p = new HttpParams();
    Object.entries(params || {}).forEach(([k, v]) => { if (v) p = p.set(k, v); });
    return this.http.get<T>(`${this.base}${path}`, { params: p });
  }
  post<T>(path: string, body: unknown = {}): Observable<T> {
    return this.http.post<T>(`${this.base}${path}`, body);
  }
  put<T>(path: string, body: unknown): Observable<T> {
    return this.http.put<T>(`${this.base}${path}`, body);
  }
  patch<T>(path: string, body: unknown): Observable<T> {
    return this.http.patch<T>(`${this.base}${path}`, body);
  }
}
