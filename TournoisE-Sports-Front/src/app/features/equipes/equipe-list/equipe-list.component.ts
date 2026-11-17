import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { EquipeService } from '../../../core/services/equipe.service';
import { AuthService } from '../../../core/services/auth.service';
import { Equipe } from '../../../core/models/equipe.models';
import { PaginatorComponent } from '../../../shared/paginator.component';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-equipe-list',
  standalone: true,
  imports: [PaginatorComponent, RouterLink],
  template: `
    <div class="page-header">
      <h1>Équipes</h1>
      <div class="page-actions">
        @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
          <a routerLink="/equipes/new" class="btn btn--primary">Nouvelle équipe</a>
        }
      </div>
    </div>

    @if (errorMessage) {
      <div class="alert alert--error">{{ errorMessage }}</div>
    }

    <div class="equipe-grid">
      @for (e of equipes; track e.id) {
        <div class="card equipe-card">
          <div class="equipe-header">
            <h2>{{ e.nom }}</h2>
            <span class="badge badge--avenir">{{ e.tag || '—' }}</span>
          </div>
          <p><strong>Joueurs :</strong> {{ e.nombreJoueurs }}</p>
          <p><strong>Créée le :</strong> {{ formatDate(e.dateCreation) }}</p>
          <div class="equipe-actions" style="margin-top: auto;">
            <a [routerLink]="['/equipes', e.id]" class="btn btn--ghost btn--sm">Détail</a>
            @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
              <a [routerLink]="['/joueurs', e.id]" class="btn btn--ghost btn--sm">Gérer les joueurs</a>
            }
            @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
              <a [routerLink]="['/equipes', e.id, 'edit']" class="btn btn--ghost btn--sm">Modifier</a>
            }
            @if (auth.hasAnyRole(['Admin'])) {
              <button class="btn btn--danger btn--sm" (click)="onDelete(e.id)">Supprimer</button>
            }
          </div>
        </div>
      } @empty {
        <div class="empty-state">Aucune équipe trouvée.</div>
      }
    </div>

    <app-paginator [page]="page" [totalPages]="totalPages" (pageChange)="loadPage($event)" />
  `,
  styles: [`
    .equipe-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
      gap: 1.5rem;
    }
    .equipe-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }
    .equipe-header h2 {
      margin: 0;
      font-size: 1.25rem;
    }
    .equipe-actions {
      display: flex;
      gap: 0.5rem;
    }
  `]
})
export class EquipeListComponent implements OnInit {
  private readonly equipeService = inject(EquipeService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  equipes: Equipe[] = [];
  page = 1;
  pageSize = 10;
  totalPages = 0;
  errorMessage = '';

  ngOnInit() {
    this.loadPage(1);
  }

  loadPage(page: number) {
    this.equipeService.getAll(page, this.pageSize).subscribe({
      next: (result: { items: Equipe[]; page: number; pageSize: number; totalCount: number; totalPages: number }) => {
        this.equipes = result.items;
        this.page = result.page;
        this.totalPages = result.totalPages;
        this.errorMessage = '';
      },
      error: (err: any) => {
        this.errorMessage = 'Impossible de charger les équipes.';
      }
    });
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString('fr-FR');
  }

  async onDelete(id: number) {
    if (!confirm('Êtes-vous sûr de vouloir supprimer cette équipe ?')) return;
    try {
      await firstValueFrom(this.equipeService.delete(id));
      this.loadPage(this.page);
    } catch (err: any) {
      this.errorMessage = err?.error?.message || 'Impossible de supprimer cette équipe.';
    }
  }
}