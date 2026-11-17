import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  Jeu,
  CreateJeuDto,
  UpdateJeuDto
} from '../models/jeu.models';

interface PagedJeuResponse {
  items: Jeu[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class JeuService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/jeux`;

  getAll(page = 1, pageSize = 10) {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedJeuResponse>(this.url, { params });
  }

  getById(id: number) {
    return this.http.get<Jeu>(`${this.url}/${id}`);
  }

  create(dto: CreateJeuDto) {
    return this.http.post<Jeu>(this.url, dto);
  }

  update(id: number, dto: UpdateJeuDto) {
    return this.http.put<Jeu>(`${this.url}/${id}`, dto);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}