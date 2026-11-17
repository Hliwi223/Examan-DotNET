import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  Joueur,
  UpdateJoueurDto
} from '../models/joueur.models';

interface PagedJoueurResponse {
  items: Joueur[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class JoueurService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/joueurs`;

  getAll(page = 1, pageSize = 10, equipeId?: number) {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (equipeId) {
      params = params.set('equipeId', equipeId.toString());
    }
    return this.http.get<PagedJoueurResponse>(this.url, { params });
  }

  getById(id: number) {
    return this.http.get<Joueur>(`${this.url}/${id}`);
  }

  update(id: number, dto: UpdateJoueurDto) {
    return this.http.put<Joueur>(`${this.url}/${id}`, dto);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}