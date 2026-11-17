import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TournoiService } from '../../../core/services/tournoi.service';
import { AuthService } from '../../../core/services/auth.service';
import { Tournoi } from '../../../core/models/tournoi.models';
import { PaginatorComponent } from '../../../shared/paginator.component';
import { StatutBadgeComponent } from '../../../shared/statut-badge.component';
import { TournoiInscriptionsComponent } from '../tournoi-inscriptions.component';
import { TournoiMatchsComponent } from '../tournoi-matchs.component';
import { TournoiClassementComponent } from '../tournoi-classement.component';

@Component({
  selector: 'app-tournoi-detail',
  standalone: true,
  imports: [PaginatorComponent, StatutBadgeComponent, RouterLink, TournoiInscriptionsComponent, TournoiMatchsComponent, TournoiClassementComponent],
  template: `
    <div class="page-header">
      <h1>{{ tournoi?.nom }}</h1>
      <a routerLink="/tournois" class="btn btn--ghost">Retour à la liste</a>
    </div>

    @if (tournoi) {
      <div class="tournoi-hero card">
        <div class="hero-info">
          <h2>{{ tournoi.nom }}</h2>
          <p><strong>Jeu :</strong> {{ tournoi.nomJeu }}</p>
          <p><strong>Du {{ formatDate(tournoi.dateDebut) }} au {{ formatDate(tournoi.dateFin) }}</strong></p>
          <p><strong>Dotation :</strong> {{ tournoi.dotation }} €</p>
          <app-statut-badge [statut]="tournoi.statut"></app-statut-badge>
        </div>
      </div>

      <div class="tabs">
        <button class="tab" [class.active]="activeTab === 'inscriptions'" (click)="activeTab = 'inscriptions'">
          Inscriptions ({{ inscriptionsCount }})
        </button>
        <button class="tab" [class.active]="activeTab === 'matchs'" (click)="activeTab = 'matchs'">
          Matchs ({{ matchsCount }})
        </button>
        <button class="tab" [class.active]="activeTab === 'classement'" (click)="activeTab = 'classement'">
          Classement
        </button>
      </div>

      @if (activeTab === 'inscriptions') {
        <app-tournoi-inscriptions
          [tournoiId]="tournoi.id"
          (inscriptionsChanged)="loadData()"
        ></app-tournoi-inscriptions>
      }
      @if (activeTab === 'matchs') {
        <app-tournoi-matchs
          [tournoiId]="tournoi.id"
          (matchsChanged)="loadData()"
        ></app-tournoi-matchs>
      }
      @if (activeTab === 'classement') {
        <app-tournoi-classement
          [tournoiId]="tournoi.id"
          [classementData]="classement"
        ></app-tournoi-classement>
      }
    }
  `,
  styles: [`
    .tournoi-hero {
      margin-bottom: 2rem;
    }
    .hero-info h2 {
      margin: 0 0 1rem 0;
      color: var(--accent-2);
    }
    .hero-info p {
      margin: 0.25rem 0;
      color: var(--text);
    }
  `]
})
export class TournoiDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly tournoiService = inject(TournoiService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  tournoi: Tournoi | null = null;
  activeTab = 'inscriptions';
  inscriptionsCount = 0;
  matchsCount = 0;
  classement: any[] = [];

  ngOnInit() {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.tournoiService.getById(id).subscribe({
      next: (t: Tournoi) => {
        this.tournoi = t;
        this.loadData();
      },
      error: () => {
        this.router.navigateByUrl('/tournois');
      }
    });
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString('fr-FR');
  }

  loadData() {
    if (!this.tournoi) return;
    this.tournoiService.getInscriptions(this.tournoi.id, 1, 1).subscribe({
      next: (r: { totalCount: number }) => { this.inscriptionsCount = r.totalCount; },
      error: (err: any) => console.error(err)
    });
    this.tournoiService.getMatchs(this.tournoi.id, 1, 1).subscribe({
      next: (r: { totalCount: number }) => { this.matchsCount = r.totalCount; },
      error: (err: any) => console.error(err)
    });
    this.loadClassement();
  }

  loadClassement() {
    if (!this.tournoi) return;
    console.log('Fetching classement for tournoi', this.tournoi.id);
    this.tournoiService.getClassement(this.tournoi.id).subscribe({
      next: (c: any[]) => {
        console.log('Classement response:', c);
        this.classement = c;
      },
      error: (err: any) => {
        console.error('Classement error:', err);
        console.error('Error status:', err.status);
        console.error('Error message:', err.message);
        console.error('Error body:', err.error);
      }
    });
  }
}