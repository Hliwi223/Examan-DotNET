import { Component, inject, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TournoiService } from '../../core/services/tournoi.service';
import { EquipeService } from '../../core/services/equipe.service';
import { AuthService } from '../../core/services/auth.service';
import { CreateInscriptionDto } from '../../core/models/inscription.models';
import { Equipe } from '../../core/models/equipe.models';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-tournoi-inscriptions',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <h3>Inscriptions des équipes</h3>

    @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
      <form [formGroup]="inscriptionForm" (ngSubmit)="onAddInscription()" class="inscription-form card" style="margin-bottom: 2rem;">
        <div class="form-row">
          <div class="form-group">
            <label for="equipeId">Équipe</label>
            <select id="equipeId" formControlName="equipeId" class="form-control" [class.error]="equipeId?.invalid && equipeId?.touched">
              <option value="">-- Sélectionner une équipe --</option>
              @for (equipe of equipes; track equipe.id) {
                <option [value]="equipe.id">{{ equipe.nom }} ({{ equipe.tag || '—' }})</option>
              }
            </select>
            @if (equipeId?.invalid && equipeId?.touched) {
              <span class="error-text">Veuillez sélectionner une équipe.</span>
            }
          </div>
          <button type="submit" class="btn btn--primary" [disabled]="inscriptionForm.invalid || loading">
            {{ submitButtonText }}
          </button>
        </div>
      </form>

      @if (errorMessage) {
        <div class="alert alert--error">{{ errorMessage }}</div>
      }
    }

    <table class="table">
      <thead>
        <tr>
          <th>Équipe</th>
          <th>Tag</th>
          <th>Date d'inscription</th>
          @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
            <th>Actions</th>
          }
        </tr>
      </thead>
      <tbody>
        @for (insc of inscriptions; track insc.id) {
          <tr>
            <td>{{ insc.nomEquipe }}</td>
            <td>{{ insc.tagEquipe || '-' }}</td>
            <td>{{ formatDate(insc.dateInscription) }}</td>
            @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
              <td>
                <button class="btn btn--danger btn--sm" (click)="onDelete(insc.id)">Désinscrire</button>
              </td>
            }
          </tr>
        } @empty {
          <tr><td colspan="4" class="empty-state">Aucune équipe inscrite.</td></tr>
        }
      </tbody>
    </table>
  `,
  styles: [`
    .inscription-form {
      display: flex;
      gap: 0.75rem;
      align-items: flex-end;
      flex-wrap: wrap;
    }
    .inscription-form .form-group {
      margin-bottom: 0;
      flex: 1;
      min-width: 200px;
    }
    .inscription-form select {
      min-width: 150px;
    }
    .empty-state {
      text-align: center;
      padding: 1rem;
      color: var(--muted);
    }
  `]
})
export class TournoiInscriptionsComponent implements OnInit {
  private readonly tournoiService = inject(TournoiService);
  private readonly equipeService = inject(EquipeService);
  private readonly fb = inject(FormBuilder);
  readonly auth = inject(AuthService);

  @Input() tournoiId = 0;
  @Output() inscriptionsChanged = new EventEmitter<void>();

  inscriptions: any[] = [];
  equipes: Equipe[] = [];
  inscriptionForm: FormGroup;
  loading = false;
  errorMessage = '';

  constructor() {
    this.inscriptionForm = this.fb.group({
      equipeId: [null, Validators.required]
    });
  }

  get equipeId() { return this.inscriptionForm.get('equipeId'); }

  get submitButtonText(): string {
    return this.loading ? 'Ajout...' : 'Inscrire une équipe';
  }

  ngOnInit() {
    this.loadInscriptions();
    this.equipeService.getAll(1, 100).subscribe({
      next: (result: { items: Equipe[] }) => { this.equipes = result.items; },
      error: (err: any) => console.error('Erreur chargement équipes', err)
    });
  }

  loadInscriptions() {
    this.tournoiService.getInscriptions(this.tournoiId, 1, 10).subscribe({
      next: (result: { items: any[] }) => { this.inscriptions = result.items; },
      error: (err: any) => console.error('Erreur chargement inscriptions', err)
    });
  }

  formatDate(isoString: string): string {
    return new Date(isoString).toLocaleDateString('fr-FR');
  }

  private extractErrorMessage(err: any): string {
    const body = err?.error;
    if (body?.message) return body.message;
    if (body?.errors) {
      const msgs = Object.values(body.errors as Record<string, string[]>).flat();
      return msgs.join(' ');
    }
    if (err?.message) return err.message;
    return 'Erreur lors de l\'inscription.';
  }

  async onAddInscription() {
    if (this.inscriptionForm.invalid) {
      this.inscriptionForm.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.errorMessage = '';
    const dto: CreateInscriptionDto = { equipeId: this.equipeId!.value };
    try {
      await firstValueFrom(this.tournoiService.createInscription(this.tournoiId, dto));
      this.inscriptionForm.reset({ equipeId: null });
      this.loadInscriptions();
      this.inscriptionsChanged.emit();
    } catch (err: any) {
      this.errorMessage = this.extractErrorMessage(err);
    } finally {
      this.loading = false;
    }
  }

  async onDelete(id: number) {
    if (!confirm('Désinscrire cette équipe ?')) return;
    try {
      await firstValueFrom(this.tournoiService.deleteInscription(id));
      this.loadInscriptions();
      this.inscriptionsChanged.emit();
    } catch (err: any) {
      this.errorMessage = this.extractErrorMessage(err);
    }
  }
}