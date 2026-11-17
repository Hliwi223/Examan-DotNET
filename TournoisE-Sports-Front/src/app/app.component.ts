import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterLink, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  readonly auth = inject(AuthService);
  readonly router = inject(Router);

  get userRolesDisplay(): string {
    return this.auth.session?.roles.join(', ') ?? '';
  }

  logout(): void {
    this.auth.logout();
  }
}