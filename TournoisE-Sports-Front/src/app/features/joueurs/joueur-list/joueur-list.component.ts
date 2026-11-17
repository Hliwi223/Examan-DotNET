import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { EquipeService } from '../../../core/services/equipe.service';
import { JoueurService } from '../../../core/services/joueur.service';
import { AuthService } from '../../../core/services/auth.service';
import { Joueur, CreateJoueurDto } from '../../../core/models/joueur.models';
import { firstValueFrom } from 'rxjs';

interface PagedJoueurResponse {
  items: Joueur[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Component({
  selector: 'app-joueur-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <div class="page-header">
      <h1>Joueurs de l'équipe</h1>
      <a [routerLink]="['/equipes', equipeId]" class="btn btn--ghost">Retour à l'équipe</a>
    </div>

    @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
      <form [formGroup]="joueurForm" (ngSubmit)="onAddJoueur()" class="joueur-form card" style="margin-bottom: 2rem; max-width: 600px;">
        <div class="form-row">
          <div class="form-group">
            <label for="pseudo">Pseudo</label>
            <input type="text" id="pseudo" formControlName="pseudo" class="form-control" [class.error]="pseudo?.invalid && pseudo?.touched" placeholder="Ex: Faker" />
            @if (pseudo?.invalid && pseudo?.touched) { <span class="error-text">Le pseudo est obligatoire.</span> }
          </div>
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
          <button type="submit" class="btn btn--primary" [disabled]="joueurForm.invalid || loading">
            {{ loading ? 'Ajout...' : 'Ajouter le joueur' }}
          </button>
        </div>
      </form>
    }

    <table class="table">
      <thead>
        <tr>
          <th>Pseudo</th>
          <th>Nom</th>
          <th>Prénom</th>
          <th>Email</th>
          @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
            <th>Actions</th>
          }
        </tr>
      </thead>
      <tbody>
        @for (j of joueurs; track j.id) {
          <tr>
            <td><strong>{{ j.pseudo }}</strong></td>
            <td>{{ j.nom || '-' }}</td>
            <td>{{ j.prenom || '-' }}</td>
            <td>{{ j.email || '-' }}</td>
            @if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
              <td>
                <a [routerLink]="['/joueurs', j.id, 'edit']" class="btn btn--ghost btn--sm">Modifier</a>
                <button class="btn btn--danger btn--sm" (click)="onDelete(j.id)">Supprimer</button>
              </td>
            }
          </tr>
        } @empty {
          <tr><td colspan="5" class="empty-state">Aucun joueur dans cette équipe.</td></tr>
        }
      </tbody>
    </table>
  `,
  styles: [`
    .joueur-form {
      display: flex;
      gap: 0.75rem;
      align-items: flex-end;
      flex-wrap: wrap;
    }
    .joueur-form .form-group {
      margin-bottom: 0;
      flex: 1;
      min-width: 150px;
    }
    .empty-state {
      text-align: center;
      padding: 2rem;
      color: var(--muted);
    }
  `]
})
export class JoueurListComponent implements OnInit {
  private readonly equipeService = inject(EquipeService);
  private readonly joueurService = inject(JoueurService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly auth = inject(AuthService);

  equipeId = 0;
  joueurs: Joueur[] = [];
  joueurForm: FormGroup;
  loading = false;

  constructor() {
    this.joueurForm = this.fb.group({
      pseudo: ['', Validators.required],
      nom: [''],
      prenom: [''],
      email: ['']
    });
  }

  ngOnInit() {
    this.equipeId = Number(this.route.snapshot.paramMap.get('equipeId')) || 0;
    this.loadJoueurs();
  }

  loadJoueurs() {
    this.equipeService.getJoueurs(this.equipeId, 1, 100).subscribe({
      next: (result: PagedJoueurResponse) => { this.joueurs = result.items; },
      error: (err: any) => console.error(err)
    });
  }

  get pseudo() { return this.joueurForm.get('pseudo'); }

  private extractErrorMessage(err: any): string {
    const body = err?.error;
    if (body?.message) return body.message;
    if (body?.errors) {
      const msgs = Object.values(body.errors as Record<string, string[]>).flat();
      return msgs.join(' ');
    }
    if (err?.message) return err.message;
    return 'Erreur lors de l\'ajout du joueur.';
  }

  async onAddJoueur() {
    if (this.joueurForm.invalid) return;
    this.loading = true;
    const dto: CreateJoueurDto = {
      pseudo: this.pseudo!.value,
      nom: this.joueurForm.value.nom || undefined,
      prenom: this.joueurForm.value.prenom || undefined,
      email: this.joueurForm.value.email || undefined,
      equipeId: this.equipeId
    };
    try {
      await firstValueFrom(this.equipeService.addJoueur(this.equipeId, dto));
      this.joueurForm.reset();
      this.loadJoueurs();
    } catch (err: any) {
      alert(this.extractErrorMessage(err));
    } finally {
      this.loading = false;
    }
  }

  async onDelete(id: number) {
    if (!confirm('Supprimer ce joueur ?')) return;
    try {
      await firstValueFrom(this.joueurService.delete(id));
      this.loadJoueurs();
    } catch (err: any) {
      alert(this.extractErrorMessage(err));
    }
  }
}