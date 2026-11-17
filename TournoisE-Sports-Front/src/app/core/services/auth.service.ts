import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  AuthSession,
  LoginDto,
  RegisterDto,
  AuthResponseDto
} from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storageKey = 'esport_session';

  login(email: string, password: string) {
    const dto: LoginDto = { email, password };
    return this.http
      .post<AuthResponseDto>(`${environment.apiUrl}/auth/login`, dto)
      .pipe(tap((session) => this.storeSession(session)));
  }

  register(email: string, password: string) {
    const dto: RegisterDto = { email, password };
    return this.http.post(`${environment.apiUrl}/auth/register`, dto);
  }

  logout() {
    localStorage.removeItem(this.storageKey);
    this.router.navigate(['/login']);
  }

  private storeSession(session: AuthResponseDto) {
    const authSession: AuthSession = {
      token: session.token,
      expiration: session.expires,
      email: session.email,
      roles: session.roles
    };
    localStorage.setItem(this.storageKey, JSON.stringify(authSession));
  }

  get session(): AuthSession | null {
    const raw = localStorage.getItem(this.storageKey);
    return raw ? (JSON.parse(raw) as AuthSession) : null;
  }

  get token(): string | null {
    return this.session?.token ?? null;
  }

  isLoggedIn(): boolean {
    const session = this.session;
    return !!session && new Date(session.expiration) > new Date();
  }

  hasAnyRole(roles: string[]): boolean {
    return this.session?.roles.some((role) => roles.includes(role)) ?? false;
  }
}