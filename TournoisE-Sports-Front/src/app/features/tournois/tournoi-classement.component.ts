import { Component, inject, Input, OnInit } from '@angular/core';
import { TournoiService } from '../../core/services/tournoi.service';

@Component({
  selector: 'app-tournoi-classement',
  standalone: true,
  imports: [],
  template: `
    <h3>Classement</h3>

    @if (loading) {
      <div class="loading">Chargement du classement...</div>
    } @else if (error) {
      <div class="alert alert--error">{{ error }}</div>
    } @else if (classementData.length === 0) {
      <div class="empty-state">
        <p>Aucun classement disponible.</p>
        <p class="hint">Le classement apparaît après avoir saisi au moins un score de match terminé.</p>
      </div>
    } @else {
      <table class="table">
        <thead>
          <tr>
            <th>Rang</th>
            <th>Équipe</th>
            <th>Tag</th>
            <th>Victoires</th>
            <th>Défaites</th>
            <th>Matchs joués</th>
            <th>Points marqués</th>
            <th>Points encaissés</th>
            <th>Différence</th>
            <th>Points</th>
          </tr>
        </thead>
        <tbody>
          @for (c of classementData; track c.equipeId) {
            <tr [class]="c.rang <= 3 ? 'podium' : ''">
              <td>
                @if (c.rang === 1) { 🥇 } @else if (c.rang === 2) { 🥈 } @else if (c.rang === 3) { 🥉 } @else { {{ c.rang }} }
              </td>
              <td><strong>{{ c.nomEquipe }}</strong></td>
              <td>{{ c.tagEquipe || '-' }}</td>
              <td>{{ c.victoires }}</td>
              <td>{{ c.defaites }}</td>
              <td>{{ c.matchsJoues }}</td>
              <td>{{ c.pointsMarques }}</td>
              <td>{{ c.pointsEncaisses }}</td>
              <td>{{ c.differencePoints }}</td>
              <td>{{ c.points }}</td>
            </tr>
          }
        </tbody>
      </table>
    }
  `,
  styles: [`
    .loading {
      text-align: center;
      padding: 2rem;
      color: var(--muted);
    }
    .podium td {
      font-weight: 700;
      background: rgba(139, 92, 246, 0.15);
    }
    .empty-state {
      text-align: center;
      padding: 2rem;
      color: var(--muted);
    }
    .hint {
      margin-top: 0.5rem;
      font-size: 0.85rem;
      font-style: italic;
    }
  `]
})
export class TournoiClassementComponent implements OnInit {
  private readonly tournoiService = inject(TournoiService);

  @Input() tournoiId = 0;
  @Input() classementData: any[] = [];

  loading = false;
  error = '';

  ngOnInit() {
    // Données fournies par le parent via classementData
  }
}