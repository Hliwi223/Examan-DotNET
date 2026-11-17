import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="login-page">
      <div class="login-card card">
        <h1>Connexion</h1>
        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="login-form">
          <div class="form-group">
            <label for="email">Email</label>
            <input
              type="email"
              id="email"
              formControlName="email"
              class="form-control"
              [class.error]="email?.invalid && email?.touched"
              placeholder="admin@esport.local"
            />
            @if (email?.invalid && email?.touched) {
              <span class="error-text">Email invalide.</span>
            }
          </div>
          <div class="form-group">
            <label for="password">Mot de passe</label>
            <input
              type="password"
              id="password"
              formControlName="password"
              class="form-control"
              [class.error]="password?.invalid && password?.touched"
              placeholder="••••••••"
            />
            @if (password?.invalid && password?.touched) {
              <span class="error-text">Mot de passe requis.</span>
            }
          </div>
          @if (errorMessage) {
            <div class="alert alert--error">{{ errorMessage }}</div>
          }
          <button type="submit" class="btn btn--primary" [disabled]="form.invalid || loading">
            {{ loading ? 'Connexion...' : 'Se connecter' }}
          </button>
        </form>
        <p class="login-footer">
          Pas de compte ? <a routerLink="/register">S'inscrire</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .login-page {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: calc(100vh - 120px);
      padding: 2rem 1rem;
    }
    .login-card {
      width: 100%;
      max-width: 400px;
      padding: 2rem;
    }
    .login-card h1 {
      text-align: center;
      margin-bottom: 1.5rem;
      color: var(--accent-2);
    }
    .login-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .login-footer {
      margin-top: 1rem;
      text-align: center;
      font-size: 0.9rem;
      color: var(--muted);
    }
    .login-footer a {
      color: var(--accent);
      text-decoration: underline;
    }
  `]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  loading = false;
  errorMessage = '';

  get email() { return this.form.get('email'); }
  get password() { return this.form.get('password'); }

  async onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';

    try {
      await firstValueFrom(this.auth.login(this.email!.value, this.password!.value));
      this.router.navigateByUrl('/tournois');
    } catch (err: any) {
      this.errorMessage = this.extractErrorMessage(err);
    } finally {
      this.loading = false;
    }
  }

  private extractErrorMessage(err: any): string {
    const body = err?.error;
    if (body?.message) return body.message;
    if (body?.errors) {
      const msgs = Object.values(body.errors as Record<string, string[]>).flat();
      return msgs.join(' ');
    }
    if (err?.message) return err.message;
    if (typeof err === 'string') return err;
    return 'Échec de la connexion. Vérifiez que le backend est démarré et que le certificat HTTPS est approuvé.';
  }
}