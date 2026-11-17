import { Component, inject, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { TournoiService } from '../../core/services/tournoi.service';
import { EquipeService } from '../../core/services/equipe.service';
import { MatchService } from '../../core/services/match.service';
import { AuthService } from '../../core/services/auth.service';
import { CreateMatchDto, UpdateScoreDto } from '../../core/models/match.models';
import { Equipe } from '../../core/models/equipe.models';
import { Tournoi } from '../../core/models/tournoi.models';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-tournoi-matchs',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, RouterLink],
  template: `
    <h3>Matchs du tournoi</h3>

    @if (tournoi) {
      <div class="tournoi-info" style="margin-bottom: 1rem; padding: 0.75rem; background: var(--surface); border-radius: 6px; border: 1px solid var(--border);">
        <p><strong>Tournoi :</strong> {{ tournoi.nom }}</p>
        <p><strong>Du {{ formatDate(tournoi.dateDebut) }} au {{ formatDate(tournoi.dateFin) }}</strong></p>
      </div>
    }

    @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
      <form [formGroup]="matchForm" (ngSubmit)="onAddMatch()" class="match-form card" style="margin-bottom: 2rem;">
        <div class="form-row">
          <div class="form-group">
            <label for="dateMatch">Date du match</label>
            <input type="datetime-local" id="dateMatch" formControlName="dateMatch" class="form-control" [class.error]="dateMatch?.invalid && dateMatch?.touched" />
            @if (dateMatch?.invalid && dateMatch?.touched) {
              <span class="error-text">{{ dateMatchError }}</span>
            }
          </div>
          <div class="form-group">
            <label for="equipeAId">Équipe A</label>
            <select id="equipeAId" formControlName="equipeAId" class="form-control">
              @for (e of equipes; track e.id) {
                <option [value]="e.id">{{ e.nom }}</option>
              }
            </select>
          </div>
          <div class="form-group">
            <label for="equipeBId">Équipe B</label>
            <select id="equipeBId" formControlName="equipeBId" class="form-control">
              @for (e of equipes; track e.id) {
                <option [value]="e.id">{{ e.nom }}</option>
              }
            </select>
          </div>
          <button type="submit" class="btn btn--primary" [disabled]="matchForm.invalid || loading">
            {{ loading ? 'Création...' : 'Planifier le match' }}
          </button>
        </div>
      </form>

      @if (matchError) {
        <div class="alert alert--error">{{ matchError }}</div>
      }
    }

    <table class="table">
      <thead>
        <tr>
          <th>Équipe A</th>
          <th>Score A</th>
          <th>Score B</th>
          <th>Équipe B</th>
          <th>État</th>
          <th>Vainqueur</th>
          @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
            <th>Actions</th>
          }
        </tr>
      </thead>
      <tbody>
        @for (m of matchs; track m.id) {
          <tr>
            <td>{{ m.nomEquipeA }}</td>
            <td>
              @if (auth.hasAnyRole(['Admin', 'Organisateur']) && !m.termine) {
                <input type="number" class="form-control" style="width: 60px;" [value]="editScores[m.id]?.scoreA ?? 0" (input)="onScoreInput(m.id, 'scoreA', $event)" />
              } @else {
                {{ m.scoreEquipeA ?? '-' }}
              }
            </td>
            <td>
              @if (auth.hasAnyRole(['Admin', 'Organisateur']) && !m.termine) {
                <input type="number" class="form-control" style="width: 60px;" [value]="editScores[m.id]?.scoreB ?? 0" (input)="onScoreInput(m.id, 'scoreB', $event)" />
              } @else {
                {{ m.scoreEquipeB ?? '-' }}
              }
            </td>
            <td>{{ m.nomEquipeB }}</td>
            <td><span class="badge" [class]="m.termine ? 'badge--termine' : 'badge--avenir'">{{ m.termine ? 'Terminé' : 'À venir' }}</span></td>
            <td>{{ m.nomVainqueur || (m.termine ? '—' : '-') }}</td>
            @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
              <td class="actions">
                @if (!m.termine) {
                  <button class="btn btn--primary btn--sm" (click)="onSaveScore(m.id)">Valider</button>
                  <button class="btn btn--danger btn--sm" (click)="onDelete(m.id)">Supprimer</button>
                } @else {
                  <span class="badge badge--termine" style="font-size: 0.7rem;">Terminé</span>
                }
              </td>
            }
          </tr>
        } @empty {
          <tr><td colspan="7" class="empty-state">Aucun match planifié.</td></tr>
        }
      </tbody>
    </table>
  `,
  styles: [`
    .match-form {
      display: flex;
      gap: 0.75rem;
      align-items: flex-end;
      flex-wrap: wrap;
    }
    .match-form .form-group {
      margin-bottom: 0;
      flex: 1;
      min-width: 150px;
    }
    .tournoi-info p {
      margin: 0.25rem 0;
      color: var(--muted);
    }
    .empty-state {
      text-align: center;
      padding: 1rem;
      color: var(--muted);
    }
    .actions {
      display: flex;
      gap: 0.4rem;
    }
  `]
})
export class TournoiMatchsComponent implements OnInit {
  private readonly tournoiService = inject(TournoiService);
  private readonly equipeService = inject(EquipeService);
  private readonly matchService = inject(MatchService);
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);

  @Input() tournoiId = 0;
  @Output() matchsChanged = new EventEmitter<void>();

  matchs: any[] = [];
  equipes: Equipe[] = [];
  tournoi: Tournoi | null = null;
  matchForm: FormGroup;
  loading = false;
  editScores: { [key: number]: { scoreA: number; scoreB: number } } = {};
  matchError = '';

  constructor() {
    this.matchForm = this.fb.group({
      dateMatch: ['', Validators.required],
      equipeAId: [null, Validators.required],
      equipeBId: [null, Validators.required]
    });
  }

  ngOnInit() {
    this.loadMatchs();
    this.loadTournoi();
    this.equipeService.getAll(1, 100).subscribe({
      next: (result: { items: Equipe[] }) => { this.equipes = result.items; },
      error: (err: any) => console.error(err)
    });
  }

  loadMatchs() {
    this.tournoiService.getMatchs(this.tournoiId, 1, 10).subscribe({
      next: (result: { items: any[] }) => { this.matchs = result.items; },
      error: (err: any) => console.error(err)
    });
  }

  loadTournoi() {
    this.tournoiService.getById(this.tournoiId).subscribe({
      next: (t: Tournoi) => { this.tournoi = t; },
      error: (err: any) => console.error('Erreur chargement tournoi', err)
    });
  }

  get dateMatch() { return this.matchForm.get('dateMatch'); }

  get dateMatchError(): string {
    if (!this.dateMatch?.touched) return '';
    if (!this.tournoi) return '';

    const matchDate = new Date(this.dateMatch!.value);
    const debut = new Date(this.tournoi.dateDebut);
    const fin = new Date(this.tournoi.dateFin);

    if (matchDate < debut) {
      return `La date du match doit être après le début du tournoi (${this.formatDate(this.tournoi.dateDebut)}).`;
    }
    if (matchDate > fin) {
      return `La date du match doit être avant la fin du tournoi (${this.formatDate(this.tournoi.dateFin)}).`;
    }
    return '';
  }

  onScoreInput(matchId: number, field: 'scoreA' | 'scoreB', event: Event) {
    const input = event.target as HTMLInputElement;
    if (!this.editScores[matchId]) {
      this.editScores[matchId] = { scoreA: 0, scoreB: 0 };
    }
    this.editScores[matchId][field] = parseInt(input.value, 10) || 0;
  }

  async onAddMatch() {
    this.matchError = '';
    if (this.matchForm.invalid) {
      this.matchForm.markAllAsTouched();
      return;
    }

    // Validation de la date du match par rapport aux dates du tournoi
    if (this.tournoi) {
      const matchDate = new Date(this.dateMatch!.value);
      const debut = new Date(this.tournoi.dateDebut);
      const fin = new Date(this.tournoi.dateFin);

      if (matchDate < debut) {
        this.matchError = `La date du match doit être après le début du tournoi (${this.formatDate(this.tournoi.dateDebut)}).`;
        return;
      }
      if (matchDate > fin) {
        this.matchError = `La date du match doit être avant la fin du tournoi (${this.formatDate(this.tournoi.dateFin)}).`;
        return;
      }
    }

    this.loading = true;
    const dto: CreateMatchDto = {
      dateMatch: this.matchForm.value.dateMatch + 'Z',
      tournoiId: this.tournoiId,
      equipeAId: this.matchForm.value.equipeAId,
      equipeBId: this.matchForm.value.equipeBId
    };
    try {
      await firstValueFrom(this.matchService.create(dto));
      this.matchForm.reset();
      this.loadMatchs();
      this.matchsChanged.emit();
    } catch (err: any) {
      this.matchError = this.extractErrorMessage(err);
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
    return 'Erreur lors de la création du match.';
  }

  async onSaveScore(matchId: number) {
    const s = this.editScores[matchId];
    if (s === undefined) return;
    const dto: UpdateScoreDto = { scoreEquipeA: s.scoreA, scoreEquipeB: s.scoreB };
    try {
      await firstValueFrom(this.matchService.updateScore(matchId, dto));
      delete this.editScores[matchId];
      this.loadMatchs();
      this.matchsChanged.emit();
    } catch (err: any) {
      alert(this.extractErrorMessage(err));
    }
  }

  async onDelete(id: number) {
    if (!confirm('Supprimer ce match ?')) return;
    try {
      await firstValueFrom(this.matchService.delete(id));
      this.loadMatchs();
      this.matchsChanged.emit();
    } catch (err: any) {
      alert(this.extractErrorMessage(err));
    }
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString('fr-FR');
  }
}