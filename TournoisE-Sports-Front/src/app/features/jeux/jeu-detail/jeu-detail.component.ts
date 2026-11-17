import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { JeuService } from '../../../core/services/jeu.service';
import { AuthService } from '../../../core/services/auth.service';
import { Jeu } from '../../../core/models/jeu.models';
import { TournoiService } from '../../../core/services/tournoi.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-jeu-detail',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="page-header">
      <h1>{{ jeu?.nom }}</h1>
      <a routerLink="/jeux" class="btn btn--ghost">Retour à la liste</a>
    </div>

    @if (jeu) {
      <div class="card" style="max-width: 500px; margin: 0 auto 2rem auto;">
        <h2>{{ jeu.nom }}</h2>
        @if (jeu.editeur) {
          <p><strong>Éditeur :</strong> {{ jeu.editeur }}</p>
        }
        @if (jeu.genre) {
          <p><strong>Genre :</strong> {{ jeu.genre }}</p>
        }
      </div>

      <h3>Tournois sur ce jeu</h3>
      <table class="table">
        <thead>
          <tr>
            <th>Nom du tournoi</th>
            <th>Début</th>
            <th>Fin</th>
            <th>Dotation</th>
            <th>Statut</th>
            <th>Équipes inscrites</th>
            <th>Matchs</th>
          </tr>
        </thead>
        <tbody>
          @for (t of tournois; track t.id) {
            <tr>
              <td><a [routerLink]="['/tournois', t.id]">{{ t.nom }}</a></td>
              <td>{{ formatDate(t.dateDebut) }}</td>
              <td>{{ formatDate(t.dateFin) }}</td>
              <td>{{ t.dotation }} €</td>
              <td>
                @if (t.statut === 0) { À venir } @else if (t.statut === 1) { En cours } @else { Terminé }
              </td>
              <td>{{ t.nombreEquipesInscrites }}</td>
              <td>{{ t.nombreMatchs }}</td>
            </tr>
          } @empty {
            <tr><td colspan="7" class="empty-state">Aucun tournoi sur ce jeu.</td></tr>
          }
        </tbody>
      </table>

      @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
        <div style="margin-top: 1.5rem;">
          <a [routerLink]="['/jeux', jeu.id, 'edit']" class="btn btn--primary">Modifier le jeu</a>
          <button class="btn btn--danger" (click)="onDeleteJeu(jeu.id)" style="margin-left: 0.5rem;">Supprimer le jeu</button>
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
export class JeuDetailComponent implements OnInit {
  private readonly jeuService = inject(JeuService);
  private readonly tournoiService = inject(TournoiService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  jeu: Jeu | null = null;
  tournois: any[] = [];

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.jeuService.getById(id).subscribe({
      next: (j: Jeu) => { this.jeu = j; },
      error: () => { this.router.navigateByUrl('/jeux'); }
    });
    this.tournoiService.getAll(1, 100, id).subscribe({
      next: (result: { items: any[] }) => { this.tournois = result.items; },
      error: (err: any) => console.error(err)
    });
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString('fr-FR');
  }

  async onDeleteJeu(id: number) {
    if (!confirm('Supprimer ce jeu ?')) return;
    try {
      await firstValueFrom(this.jeuService.delete(id));
      this.router.navigateByUrl('/jeux');
    } catch (err: any) {
      alert(err?.error?.message || 'Erreur lors de la suppression.');
    }
  }
}