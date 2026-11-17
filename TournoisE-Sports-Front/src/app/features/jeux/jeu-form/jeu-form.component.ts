import { Component, inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { JeuService } from '../../../core/services/jeu.service';
import { CreateJeuDto } from '../../../core/models/jeu.models';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-jeu-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="page-header">
      <h1>{{ isEdit ? 'Modifier le jeu' : 'Créer un jeu' }}</h1>
      <a routerLink="/jeux" class="btn btn--ghost">Retour à la liste</a>
    </div>

    @if (errorMessage) {
      <div class="alert alert--error">{{ errorMessage }}</div>
    }

    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="jeu-form card">
      <div class="form-group">
        <label for="nom">Nom du jeu</label>
        <input
          type="text"
          id="nom"
          formControlName="nom"
          class="form-control"
          [class.error]="nom?.invalid && nom?.touched"
          placeholder="Ex: League of Legends"
        />
        @if (nom?.invalid && nom?.touched) {
          <span class="error-text">Le nom est obligatoire.</span>
        }
      </div>
      <div class="form-row">
        <div class="form-group">
          <label for="editeur">Éditeur</label>
          <input type="text" id="editeur" formControlName="editeur" class="form-control" />
        </div>
        <div class="form-group">
          <label for="genre">Genre</label>
          <input type="text" id="genre" formControlName="genre" class="form-control" />
        </div>
      </div>
      <button type="submit" class="btn btn--primary" [disabled]="form.invalid || loading">
        {{ loading ? 'Enregistrement...' : (isEdit ? 'Modifier' : 'Créer') }}
      </button>
    </form>
  `,
  styles: [`
    .jeu-form {
      max-width: 500px;
      margin: 0 auto;
    }
    .form-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
    }
  `]
})
export class JeuFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly jeuService = inject(JeuService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  form: FormGroup;
  isEdit = false;
  jeuId?: number;
  loading = false;
  errorMessage = '';

  constructor() {
    this.form = this.fb.group({
      nom: ['', [Validators.required, Validators.minLength(2)]],
      editeur: [''],
      genre: ['']
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.jeuId = +id;
      this.jeuService.getById(+id).subscribe({
        next: (j: any) => this.form.patchValue({ nom: j.nom, editeur: j.editeur || '', genre: j.genre || '' }),
        error: () => { this.errorMessage = 'Jeu introuvable.'; }
      });
    }
  }

  get nom() { return this.form.get('nom'); }

  async onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';

    const dto: CreateJeuDto = {
      nom: this.nom!.value,
      editeur: this.form.value.editeur || undefined,
      genre: this.form.value.genre || undefined
    };

    try {
      if (this.isEdit && this.jeuId) {
        await firstValueFrom(this.jeuService.update(this.jeuId, dto));
      } else {
        await firstValueFrom(this.jeuService.create(dto));
      }
      this.router.navigateByUrl('/jeux');
    } catch (err: any) {
      this.errorMessage = err?.error?.message || 'Une erreur est survenue.';
    } finally {
      this.loading = false;
    }
  }
}