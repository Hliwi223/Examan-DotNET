import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EquipeService } from '../../../core/services/equipe.service';
import { AuthService } from '../../../core/services/auth.service';
import { Equipe } from '../../../core/models/equipe.models';
import { JoueurService } from '../../../core/services/joueur.service';
import { Joueur } from '../../../core/models/joueur.models';
import { firstValueFrom } from 'rxjs';

interface PagedJoueurResponse {
  items: Joueur[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Component({
  selector: 'app-equipe-detail',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="page-header">
      <h1>{{ equipe?.nom }}</h1>
      <a routerLink="/equipes" class="btn btn--ghost">Retour à la liste</a>
    </div>

    @if (equipe) {
      <div class="card" style="max-width: 500px; margin: 0 auto 2rem auto;">
        <h2>{{ equipe.nom }}</h2>
        <p><strong>Tag :</strong> {{ equipe.tag || '—' }}</p>
        <p><strong>Date de création :</strong> {{ formatDate(equipe.dateCreation) }}</p>
        <p><strong>Nombre de joueurs :</strong> {{ equipe.nombreJoueurs }}</p>
      </div>

      <h3>Joueurs</h3>
      <table class="table">
        <thead>
          <tr>
            <th>Pseudo</th>
            <th>Nom</th>
            <th>Prénom</th>
            <th>Email</th>
            @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
              <th>Actions</th>
            }
          </tr>
        </thead>
        <tbody>
          @for (j of joueurs; track j.id) {
            <tr>
              <td><strong>{{ j.pseudo }}</strong></td>
              <td>{{ j.nom || '-' }}</td>
              <td>{{ j.prenom || '-' }}</td>
              <td>{{ j.email || '-' }}</td>
              @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
                <td>
                  <a [routerLink]="['/joueurs', j.id, 'edit']" class="btn btn--ghost btn--sm">Modifier</a>
                  <button class="btn btn--danger btn--sm" (click)="onDelete(j.id)">Supprimer</button>
                </td>
              }
            </tr>
          } @empty {
            <tr><td colspan="5" class="empty-state">Aucun joueur dans cette équipe.</td></tr>
          }
        </tbody>
      </table>

      @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
        <div style="margin-top: 1.5rem;">
          <a [routerLink]="['/equipes', equipe.id, 'edit']" class="btn btn--primary">Modifier l'équipe</a>
          <button class="btn btn--danger" (click)="onDeleteEquipe(equipe.id)" style="margin-left: 0.5rem;">Supprimer l'équipe</button>
        </div>
      }
    }
  `,
  styles: [`
    .empty-state {
      text-align: center;
      padding: 2rem;
      color: var(--muted);
    }
  `]
})
export class EquipeDetailComponent implements OnInit {
  private readonly equipeService = inject(EquipeService);
  private readonly joueurService = inject(JoueurService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  equipe: Equipe | null = null;
  joueurs: Joueur[] = [];

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.equipeService.getById(id).subscribe({
      next: (e: Equipe) => { this.equipe = e; },
      error: () => { this.router.navigateByUrl('/equipes'); }
    });
    this.equipeService.getJoueurs(id, 1, 100).subscribe({
      next: (result: PagedJoueurResponse) => { this.joueurs = result.items; },
      error: (err: any) => console.error(err)
    });
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString('fr-FR');
  }

  async onDelete(id: number) {
    if (!confirm('Supprimer ce joueur ?')) return;
    try {
      await firstValueFrom(this.joueurService.delete(id));
      const equipeId = this.equipe!.id;
      this.equipeService.getJoueurs(equipeId, 1, 100).subscribe({
        next: (result: PagedJoueurResponse) => { this.joueurs = result.items; },
        error: (err: any) => console.error(err)
      });
    } catch (err: any) {
      alert(err?.error?.message || 'Erreur lors de la suppression.');
    }
  }

  async onDeleteEquipe(id: number) {
    if (!confirm('Supprimer cette équipe ?')) return;
    try {
      await firstValueFrom(this.equipeService.delete(id));
      this.router.navigateByUrl('/equipes');
    } catch (err: any) {
      alert(err?.error?.message || 'Erreur lors de la suppression.');
    }
  }
}