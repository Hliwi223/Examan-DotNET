import { Component, inject, OnInit } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { JoueurService } from '../../../core/services/joueur.service';
import { UpdateJoueurDto } from '../../../core/models/joueur.models';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-joueur-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="page-header">
      <h1>Modifier le joueur</h1>
      <a [routerLink]="['/joueurs', joueurId]" class="btn btn--ghost">Retour au joueur</a>
    </div>

    @if (errorMessage) {
      <div class="alert alert--error">{{ errorMessage }}</div>
    }

    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="joueur-form card" style="max-width: 600px;">
      <div class="form-group">
        <label for="pseudo">Pseudo</label>
        <input type="text" id="pseudo" formControlName="pseudo" class="form-control" [class.error]="pseudo?.invalid && pseudo?.touched" />
        @if (pseudo?.invalid && pseudo?.touched) { <span class="error-text">Le pseudo est obligatoire.</span> }
      </div>
      <div class="form-row">
        <div class="form-group">
          <label for="nom">Nom</label>
          <input type="text" id="nom" formControlName="nom" class="form-control" />
        </div>
        <div class="form-group">
          <label for="prenom">Prénom</label>
          <input type="text" id="prenom" formControlName="prenom" class="form-control" />
        </div>
        <div class="form-group">
          <label for="email">Email</label>
          <input type="email" id="email" formControlName="email" class="form-control" />
        </div>
      </div>
      <div class="form-group">
        <label for="equipeId">Équipe</label>
        <input type="number" id="equipeId" formControlName="equipeId" class="form-control" [class.error]="equipeId?.invalid && equipeId?.touched" />
        @if (equipeId?.invalid && equipeId?.touched) { <span class="error-text">Équipe requise.</span> }
      </div>
      <button type="submit" class="btn btn--primary" [disabled]="form.invalid || loading">
        {{ loading ? 'Enregistrement...' : 'Modifier' }}
      </button>
    </form>
  `,
  styles: [`
    .joueur-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .form-row {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1rem;
    }
  `]
})
export class JoueurFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly joueurService = inject(JoueurService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  form: FormGroup;
  joueurId?: number;
  loading = false;
  errorMessage = '';

  constructor() {
    this.form = this.fb.group({
      pseudo: ['', Validators.required],
      nom: [''],
      prenom: [''],
      email: [''],
      equipeId: [null, Validators.required]
    });
  }

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.joueurId = +id;
      this.joueurService.getById(+id).subscribe({
        next: (j: any) => this.form.patchValue({ pseudo: j.pseudo, nom: j.nom, prenom: j.prenom, email: j.email, equipeId: j.equipeId }),
        error: () => { this.errorMessage = 'Joueur introuvable.'; }
      });
    }
  }

  get pseudo() { return this.form.get('pseudo'); }
  get equipeId() { return this.form.get('equipeId'); }

  async onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    this.errorMessage = '';

    const dto: UpdateJoueurDto = {
      pseudo: this.pseudo!.value,
      nom: this.form.value.nom || undefined,
      prenom: this.form.value.prenom || undefined,
      email: this.form.value.email || undefined,
      equipeId: this.equipeId!.value
    };

    try {
      if (this.joueurId) {
        await firstValueFrom(this.joueurService.update(this.joueurId, dto));
      }
      this.router.navigateByUrl(`/joueurs/${this.joueurId}`);
    } catch (err: any) {
      this.errorMessage = err?.error?.message || 'Une erreur est survenue.';
    } finally {
      this.loading = false;
    }
  }
}