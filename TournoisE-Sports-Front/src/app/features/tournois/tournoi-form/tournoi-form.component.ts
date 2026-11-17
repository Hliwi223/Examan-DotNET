import { Component, inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TournoiService } from '../../../core/services/tournoi.service';
import { JeuService } from '../../../core/services/jeu.service';
import { AuthService } from '../../../core/services/auth.service';
import { CreateTournoiDto, TournoiStatut } from '../../../core/models/tournoi.models';
import { Jeu } from '../../../core/models/jeu.models';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-tournoi-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="page-header">
      <h1>{{ isEdit ? 'Modifier le tournoi' : 'Créer un tournoi' }}</h1>
      <a routerLink="/tournois" class="btn btn--ghost">Retour à la liste</a>
    </div>

    @if (errorMessage) {
      <div class="alert alert--error">{{ errorMessage }}</div>
    }

    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="tournoi-form card">
      <div class="form-group">
        <label for="nom">Nom du tournoi</label>
        <input
          type="text"
          id="nom"
          formControlName="nom"
          class="form-control"
          [class.error]="nom?.invalid && nom?.touched"
          placeholder="Ex: Spring Cup 2026"
        />
        @if (nom?.invalid && nom?.touched) {
          <span class="error-text">Le nom est obligatoire.</span>
        }
      </div>

      <div class="form-row">
        <div class="form-group">
          <label for="dateDebut">Date de début</label>
          <input
            type="date"
            id="dateDebut"
            formControlName="dateDebut"
            class="form-control"
            [class.error]="dateDebut?.invalid && dateDebut?.touched"
          />
          @if (dateDebut?.invalid && dateDebut?.touched) {
            <span class="error-text">Date de début requise.</span>
          }
        </div>
        <div class="form-group">
          <label for="dateFin">Date de fin</label>
          <input
            type="date"
            id="dateFin"
            formControlName="dateFin"
            class="form-control"
            [class.error]="dateFin?.invalid && dateFin?.touched"
          />
          @if (dateFin?.invalid && dateFin?.touched) {
            <span class="error-text">Date de fin requise.</span>
          }
        </div>
      </div>

      <div class="form-row">
        <div class="form-group">
          <label for="dotation">Dotation (€)</label>
          <input
            type="number"
            id="dotation"
            formControlName="dotation"
            class="form-control"
            [class.error]="dotation?.invalid && dotation?.touched"
            min="0"
          />
          @if (dotation?.invalid && dotation?.touched) {
            <span class="error-text">Dotation invalide.</span>
          }
        </div>
        <div class="form-group">
          <label for="statut">Statut</label>
          <select id="statut" formControlName="statut" class="form-control">
            <option [value]="0">À venir</option>
            <option [value]="1">En cours</option>
            <option [value]="2">Terminé</option>
          </select>
        </div>
        <div class="form-group">
          <label for="jeuId">Jeu</label>
          <select id="jeuId" formControlName="jeuId" class="form-control">
            @for (jeu of jeux; track jeu.id) {
  <option [value]="jeu.id">{{ jeu.nom }}</option>
}
          </select>
        </div>
      </div>

      <button type="submit" class="btn btn--primary" [disabled]="form.invalid || loading">
        {{ loading ? 'Enregistrement...' : (isEdit ? 'Modifier' : 'Créer') }}
      </button>
    </form>
  `,
  styles: [`
    .tournoi-form {
      max-width: 600px;
      margin: 0 auto;
    }
    .form-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
    }
  `]
})
export class TournoiFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly tournoiService = inject(TournoiService);
  private readonly jeuService = inject(JeuService);
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  form: FormGroup;
  jeux: Jeu[] = [];
  isEdit = false;
  tournoiId?: number;
  loading = false;
  errorMessage = '';

  constructor() {
    this.form = this.fb.group({
      nom: ['', Validators.required],
      dateDebut: ['', Validators.required],
      dateFin: ['', Validators.required],
      dotation: [0, [Validators.required, Validators.min(0)]],
      statut: [0, Validators.required],
      jeuId: [null, Validators.required]
    });
  }

  ngOnInit() {
    this.loadJeux();
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.tournoiId = +id;
      this.loadTournoi(+id);
    }
  }

  loadJeux() {
    this.jeuService.getAll(1, 100).subscribe({
      next: (result: { items: Jeu[] }) => {
        this.jeux = result.items;
      },
      error: (err: any) => {
        console.error('Erreur chargement jeux', err);
      }
    });
  }

  loadTournoi(id: number) {
    this.tournoiService.getById(id).subscribe({
      next: (tournoi: any) => {
        this.form.patchValue({
          nom: tournoi.nom,
          dateDebut: tournoi.dateDebut.substring(0, 10),
          dateFin: tournoi.dateFin.substring(0, 10),
          dotation: tournoi.dotation,
          statut: tournoi.statut,
          jeuId: tournoi.jeuId
        });
      },
      error: (err: any) => {
        this.errorMessage = 'Tournoi introuvable.';
      }
    });
  }

  get nom() { return this.form.get('nom'); }
  get dateDebut() { return this.form.get('dateDebut'); }
  get dateFin() { return this.form.get('dateFin'); }
  get dotation() { return this.form.get('dotation'); }
  get statut() { return this.form.get('statut'); }
  get jeuId() { return this.form.get('jeuId'); }

  async onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';

    const dto: CreateTournoiDto = {
      nom: this.nom!.value,
      dateDebut: this.dateDebut!.value + 'T00:00:00Z',
      dateFin: this.dateFin!.value + 'T23:59:59Z',
      dotation: Number(this.dotation!.value),
      statut: Number(this.statut!.value) as TournoiStatut,
      jeuId: Number(this.jeuId!.value)
    };

    try {
      if (this.isEdit && this.tournoiId) {
        await firstValueFrom(this.tournoiService.update(this.tournoiId, dto));
      } else {
        await firstValueFrom(this.tournoiService.create(dto));
      }
      this.router.navigateByUrl('/tournois');
    } catch (err: any) {
      this.errorMessage = this.extractErrorMessage(err);
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
    return 'Une erreur est survenue.';
  }
}