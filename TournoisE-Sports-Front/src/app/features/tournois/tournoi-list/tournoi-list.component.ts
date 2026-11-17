import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TournoiService } from '../../../core/services/tournoi.service';
import { AuthService } from '../../../core/services/auth.service';
import { Tournoi } from '../../../core/models/tournoi.models';
import { PaginatorComponent } from '../../../shared/paginator.component';
import { StatutBadgeComponent } from '../../../shared/statut-badge.component';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-tournoi-list',
  standalone: true,
  imports: [PaginatorComponent, StatutBadgeComponent, RouterLink],
  template: `
    <div class="page-header">
      <h1>Tournois</h1>
      <div class="page-actions">
        @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
          <a routerLink="/tournois/new" class="btn btn--primary">Nouveau tournoi</a>
        }
      </div>
    </div>

    @if (errorMessage) {
      <div class="alert alert--error">{{ errorMessage }}</div>
    }

    <div class="tournoi-grid">
      @for (t of tournois; track t.id) {
        <div class="card tournoi-card">
          <div class="tournoi-header">
            <h2>{{ t.nom }}</h2>
            <app-statut-badge [statut]="t.statut"></app-statut-badge>
          </div>
          <p class="tournoi-jeu">{{ t.nomJeu }}</p>
          <div class="tournoi-dates">
            <span><strong>Début :</strong> {{ formatDate(t.dateDebut) }}</span>
            <span><strong>Fin :</strong> {{ formatDate(t.dateFin) }}</span>
          </div>
          <div class="tournoi-meta">
            <span><strong>Dotation :</strong> {{ t.dotation }} €</span>
            <span><strong>Équipes :</strong> {{ t.nombreEquipesInscrites }}</span>
            <span><strong>Matchs :</strong> {{ t.nombreMatchs }}</span>
          </div>
          <div class="tournoi-actions">
            <a [routerLink]="['/tournois', t.id]" class="btn btn--ghost btn--sm">Détail</a>
            @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
              <a [routerLink]="['/tournois', t.id, 'edit']" class="btn btn--ghost btn--sm">Modifier</a>
            }
            @if (auth.hasAnyRole(['Admin'])) {
              <button class="btn btn--danger btn--sm" (click)="onDelete(t.id)">Supprimer</button>
            }
          </div>
        </div>
      } @empty {
        <div class="empty-state">Aucun tournoi trouvé.</div>
      }
    </div>

    <app-paginator [page]="page" [totalPages]="totalPages" (pageChange)="loadPage($event)" />
  `,
  styles: [`
    .tournoi-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 1.5rem;
    }
    .tournoi-card {
      display: flex;
      flex-direction: column;
    }
    .tournoi-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 0.5rem;
    }
    .tournoi-header h2 {
      font-size: 1.25rem;
      margin: 0;
    }
    .tournoi-jeu {
      color: var(--muted);
      font-size: 0.9rem;
      margin-bottom: 0.75rem;
    }
    .tournoi-dates {
      display: flex;
      gap: 1rem;
      font-size: 0.85rem;
      color: var(--muted);
      margin-bottom: 0.75rem;
    }
    .tournoi-meta {
      display: flex;
      gap: 1rem;
      font-size: 0.85rem;
      color: var(--muted);
      margin-bottom: 1rem;
      flex-wrap: wrap;
    }
    .tournoi-actions {
      display: flex;
      gap: 0.5rem;
      margin-top: auto;
    }
  `]
})
export class TournoiListComponent implements OnInit {
  private readonly tournoiService = inject(TournoiService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  tournois: Tournoi[] = [];
  page = 1;
  pageSize = 10;
  totalPages = 0;
  errorMessage = '';

  ngOnInit() {
    this.loadPage(1);
  }

  loadPage(page: number) {
    this.tournoiService.getAll(page, this.pageSize).subscribe({
      next: (result: { items: Tournoi[]; page: number; pageSize: number; totalCount: number; totalPages: number }) => {
        this.tournois = result.items;
        this.page = result.page;
        this.totalPages = result.totalPages;
        this.errorMessage = '';
      },
      error: (err: any) => {
        this.errorMessage = 'Impossible de charger les tournois.';
        console.error(err);
      }
    });
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString('fr-FR');
  }

  async onDelete(id: number) {
    if (!confirm('Êtes-vous sûr de vouloir supprimer ce tournoi ?')) return;
    try {
      await firstValueFrom(this.tournoiService.delete(id));
      this.loadPage(this.page);
    } catch (err: any) {
      this.errorMessage = err?.error?.message || 'Impossible de supprimer ce tournoi.';
    }
  }
}