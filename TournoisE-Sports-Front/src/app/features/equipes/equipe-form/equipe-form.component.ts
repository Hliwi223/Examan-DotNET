import { Component, inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { EquipeService } from '../../../core/services/equipe.service';
import { CreateEquipeDto } from '../../../core/models/equipe.models';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-equipe-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="page-header">
      <h1>{{ isEdit ? 'Modifier l\'équipe' : 'Créer une équipe' }}</h1>
      <a routerLink="/equipes" class="btn btn--ghost">Retour à la liste</a>
    </div>

    @if (errorMessage) {
      <div class="alert alert--error">{{ errorMessage }}</div>
    }

    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="equipe-form card">
      <div class="form-group">
        <label for="nom">Nom de l'équipe</label>
        <input
          type="text"
          id="nom"
          formControlName="nom"
          class="form-control"
          [class.error]="nom?.invalid && nom?.touched"
          placeholder="Ex: Team Alpha"
        />
        @if (nom?.invalid && nom?.touched) {
          <span class="error-text">Le nom est obligatoire.</span>
        }
      </div>
      <div class="form-group">
        <label for="tag">Tag (optionnel, max 10 car.)</label>
        <input
          type="text"
          id="tag"
          formControlName="tag"
          class="form-control"
          placeholder="ALP"
        />
      </div>
      <button type="submit" class="btn btn--primary" [disabled]="form.invalid || loading">
        {{ loading ? 'Enregistrement...' : (isEdit ? 'Modifier' : 'Créer') }}
      </button>
    </form>
  `,
  styles: [`
    .equipe-form {
      max-width: 500px;
      margin: 0 auto;
    }
  `]
})
export class EquipeFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly equipeService = inject(EquipeService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  form: FormGroup;
  isEdit = false;
  equipeId?: number;
  loading = false;
  errorMessage = '';

  constructor() {
    this.form = this.fb.group({
      nom: ['', [Validators.required, Validators.minLength(2)]],
      tag: ['']
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.equipeId = +id;
      this.equipeService.getById(+id).subscribe({
        next: (e: any) => this.form.patchValue({ nom: e.nom, tag: e.tag || '' }),
        error: () => { this.errorMessage = 'Équipe introuvable.'; }
      });
    }
  }

  get nom() { return this.form.get('nom'); }
  get tag() { return this.form.get('tag'); }

  async onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';

    const dto: CreateEquipeDto = {
      nom: this.nom!.value,
      tag: this.tag!.value || undefined
    };

    try {
      if (this.isEdit && this.equipeId) {
        await firstValueFrom(this.equipeService.update(this.equipeId, dto));
      } else {
        await firstValueFrom(this.equipeService.create(dto));
      }
      this.router.navigateByUrl('/equipes');
    } catch (err: any) {
      this.errorMessage = err?.error?.message || 'Une erreur est survenue.';
    } finally {
      this.loading = false;
    }
  }
}