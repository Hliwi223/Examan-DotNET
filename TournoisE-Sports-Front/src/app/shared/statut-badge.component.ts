import { Component, Input } from '@angular/core';
import { TournoiStatut } from '../core/models/tournoi.models';

@Component({
  selector: 'app-statut-badge',
  standalone: true,
  imports: [],
  template: `
    <span class="badge" [class]="badgeClass">{{ label }}</span>
  `,
  styles: [`
    .badge {
      display: inline-block;
      padding: 0.2rem 0.6rem;
      border-radius: 999px;
      font-size: 0.8rem;
      font-weight: 600;
      border: 1px solid;
    }
    .badge--avenir { color: var(--accent-2); border-color: var(--accent-2); }
    .badge--encours { color: var(--accent); border-color: var(--accent); }
    .badge--termine { color: var(--muted); border-color: var(--muted); }
  `]
})
export class StatutBadgeComponent {
  @Input() statut!: TournoiStatut;

  get label(): string {
    switch (this.statut) {
      case TournoiStatut.AVenir: return 'À venir';
      case TournoiStatut.EnCours: return 'En cours';
      case TournoiStatut.Termine: return 'Terminé';
      default: return 'Inconnu';
    }
  }

  get badgeClass(): string {
    switch (this.statut) {
      case TournoiStatut.AVenir: return 'badge badge--avenir';
      case TournoiStatut.EnCours: return 'badge badge--encours';
      case TournoiStatut.Termine: return 'badge badge--termine';
      default: return 'badge';
    }
  }
}