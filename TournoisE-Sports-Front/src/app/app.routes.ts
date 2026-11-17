import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { TournoiListComponent } from './features/tournois/tournoi-list/tournoi-list.component';
import { TournoiFormComponent } from './features/tournois/tournoi-form/tournoi-form.component';
import { TournoiDetailComponent } from './features/tournois/tournoi-detail/tournoi-detail.component';
import { EquipeListComponent } from './features/equipes/equipe-list/equipe-list.component';
import { EquipeFormComponent } from './features/equipes/equipe-form/equipe-form.component';
import { EquipeDetailComponent } from './features/equipes/equipe-detail/equipe-detail.component';
import { JeuListComponent } from './features/jeux/jeu-list/jeu-list.component';
import { JeuFormComponent } from './features/jeux/jeu-form/jeu-form.component';
import { JeuDetailComponent } from './features/jeux/jeu-detail/jeu-detail.component';
import { JoueurListComponent } from './features/joueurs/joueur-list/joueur-list.component';
import { JoueurFormComponent } from './features/joueurs/joueur-form/joueur-form.component';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

const organisateurs = { roles: ['Admin', 'Organisateur'] };

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'tournois',
    canActivate: [authGuard],
    children: [
      { path: '', component: TournoiListComponent },
      { path: 'new', component: TournoiFormComponent, canActivate: [roleGuard], data: organisateurs },
      { path: ':id/edit', component: TournoiFormComponent, canActivate: [roleGuard], data: organisateurs },
      { path: ':id', component: TournoiDetailComponent }
    ]
  },
  {
    path: 'equipes',
    canActivate: [authGuard],
    children: [
      { path: '', component: EquipeListComponent },
      { path: ':id', component: EquipeDetailComponent },
      { path: 'new', component: EquipeFormComponent, canActivate: [roleGuard], data: organisateurs },
      { path: ':id/edit', component: EquipeFormComponent, canActivate: [roleGuard], data: organisateurs }
    ]
  },
  {
    path: 'jeux',
    canActivate: [authGuard],
    children: [
      { path: '', component: JeuListComponent },
      { path: ':id', component: JeuDetailComponent },
      { path: 'new', component: JeuFormComponent, canActivate: [roleGuard], data: organisateurs },
      { path: ':id/edit', component: JeuFormComponent, canActivate: [roleGuard], data: organisateurs }
    ]
  },
  {
    path: 'joueurs',
    canActivate: [authGuard],
    children: [
      { path: ':equipeId', component: JoueurListComponent },
      { path: ':id/edit', component: JoueurFormComponent, canActivate: [roleGuard], data: organisateurs }
    ]
  },
  { path: '', pathMatch: 'full', redirectTo: 'tournois' },
  { path: '**', redirectTo: 'tournois' }
];