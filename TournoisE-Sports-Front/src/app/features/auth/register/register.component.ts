import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="register-page">
      <div class="register-card card">
        <h1>Inscription</h1>
        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="register-form">
          <div class="form-group">
            <label for="email">Email</label>
            <input
              type="email"
              id="email"
              formControlName="email"
              class="form-control"
              [class.error]="email?.invalid && email?.touched"
              placeholder="joueur@esport.local"
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
              <span class="error-text">Mot de passe requis (6 caractères minimum).</span>
            }
          </div>
          @if (errorMessage) {
            <div class="alert alert--error">{{ errorMessage }}</div>
          }
          @if (successMessage) {
            <div class="alert alert--success">{{ successMessage }}</div>
          }
          <button type="submit" class="btn btn--primary" [disabled]="form.invalid || loading">
            {{ loading ? 'Création...' : 'Créer le compte' }}
          </button>
        </form>
        <p class="register-footer">
          Déjà un compte ? <a routerLink="/login">Se connecter</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .register-page {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: calc(100vh - 120px);
      padding: 2rem 1rem;
    }
    .register-card {
      width: 100%;
      max-width: 400px;
      padding: 2rem;
    }
    .register-card h1 {
      text-align: center;
      margin-bottom: 1.5rem;
      color: var(--accent-2);
    }
    .register-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .register-footer {
      margin-top: 1rem;
      text-align: center;
      font-size: 0.9rem;
      color: var(--muted);
    }
    .register-footer a {
      color: var(--accent);
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  loading = false;
  errorMessage = '';
  successMessage = '';

  get email() { return this.form.get('email'); }
  get password() { return this.form.get('password'); }

  async onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    try {
      await firstValueFrom(this.auth.register(this.email!.value, this.password!.value));
      this.successMessage = 'Compte créé. Vous pouvez maintenant vous connecter.';
      this.form.reset();
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
    return 'Échec de l\'inscription. Vérifiez que le backend est démarré et que le certificat HTTPS est approuvé.';
  }
}