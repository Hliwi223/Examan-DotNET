import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  Equipe,
  CreateEquipeDto,
  UpdateEquipeDto
} from '../models/equipe.models';
import { PagedResult } from '../models/paged-result.model';
import { Joueur, CreateJoueurDto } from '../models/joueur.models';

interface PagedEquipeResponse {
  items: Equipe[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

interface PagedJoueurResponse {
  items: Joueur[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class EquipeService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/equipes`;

  getAll(page = 1, pageSize = 10) {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedEquipeResponse>(this.url, { params });
  }

  getById(id: number) {
    return this.http.get<Equipe>(`${this.url}/${id}`);
  }

  create(dto: CreateEquipeDto) {
    return this.http.post<Equipe>(this.url, dto);
  }

  update(id: number, dto: UpdateEquipeDto) {
    return this.http.put<Equipe>(`${this.url}/${id}`, dto);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  getJoueurs(equipeId: number, page = 1, pageSize = 10) {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedJoueurResponse>(
      `${this.url}/${equipeId}/joueurs`,
      { params }
    );
  }

  addJoueur(equipeId: number, dto: CreateJoueurDto) {
    return this.http.post<Joueur>(
      `${this.url}/${equipeId}/joueurs`,
      dto
    );
  }
}