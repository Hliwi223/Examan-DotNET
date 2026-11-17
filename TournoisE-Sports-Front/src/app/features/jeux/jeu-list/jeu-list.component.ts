import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { JeuService } from '../../../core/services/jeu.service';
import { AuthService } from '../../../core/services/auth.service';
import { Jeu } from '../../../core/models/jeu.models';
import { PaginatorComponent } from '../../../shared/paginator.component';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-jeu-list',
  standalone: true,
  imports: [PaginatorComponent, RouterLink],
  template: `
    <div class="page-header">
      <h1>Jeux</h1>
      <div class="page-actions">
        @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
          <a routerLink="/jeux/new" class="btn btn--primary">Nouveau jeu</a>
        }
      </div>
    </div>

    @if (errorMessage) {
      <div class="alert alert--error">{{ errorMessage }}</div>
    }

    <div class="jeu-grid">
      @for (j of jeux; track j.id) {
        <div class="card jeu-card">
          <h2>{{ j.nom }}</h2>
          @if (j.editeur) {
            <p><strong>Éditeur :</strong> {{ j.editeur }}</p>
          }
          @if (j.genre) {
            <p><strong>Genre :</strong> {{ j.genre }}</p>
          }
          <div class="jeu-actions" style="margin-top: auto;">
            <a [routerLink]="['/jeux', j.id]" class="btn btn--ghost btn--sm">Détail</a>
            @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
              <a [routerLink]="['/jeux', j.id, 'edit']" class="btn btn--ghost btn--sm">Modifier</a>
            }
            @if (auth.hasAnyRole(['Admin'])) {
              <button class="btn btn--danger btn--sm" (click)="onDelete(j.id)">Supprimer</button>
            }
          </div>
        </div>
      } @empty {
        <div class="empty-state">Aucun jeu trouvé.</div>
      }
    </div>

    <app-paginator [page]="page" [totalPages]="totalPages" (pageChange)="loadPage($event)" />
  `,
  styles: [`
    .jeu-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 1.5rem;
    }
    .jeu-card h2 {
      margin-top: 0;
      color: var(--accent-2);
    }
    .jeu-actions {
      display: flex;
      gap: 0.5rem;
      margin-top: auto;
    }
  `]
})
export class JeuListComponent implements OnInit {
  private readonly jeuService = inject(JeuService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  jeux: Jeu[] = [];
  page = 1;
  pageSize = 10;
  totalPages = 0;
  errorMessage = '';

  ngOnInit() {
    this.loadPage(1);
  }

  loadPage(page: number) {
    this.jeuService.getAll(page, this.pageSize).subscribe({
      next: (result: { items: Jeu[]; page: number; pageSize: number; totalCount: number; totalPages: number }) => {
        this.jeux = result.items;
        this.page = result.page;
        this.totalPages = result.totalPages;
        this.errorMessage = '';
      },
      error: () => {
        this.errorMessage = 'Impossible de charger les jeux.';
      }
    });
  }

  async onDelete(id: number) {
    if (!confirm('Êtes-vous sûr de vouloir supprimer ce jeu ?')) return;
    try {
      await firstValueFrom(this.jeuService.delete(id));
      this.loadPage(this.page);
    } catch (err: any) {
      this.errorMessage = err?.error?.message || 'Impossible de supprimer ce jeu.';
    }
  }
}