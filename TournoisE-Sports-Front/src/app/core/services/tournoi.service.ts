import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  Tournoi,
  CreateTournoiDto,
  UpdateTournoiDto,
  ClassementDto,
  TournoiStatut
} from '../models/tournoi.models';
import { PagedResult } from '../models/paged-result.model';
import { Inscription, CreateInscriptionDto } from '../models/inscription.models';
import { Match } from '../models/match.models';

interface PagedTournoiResponse {
  items: Tournoi[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

interface PagedInscriptionResponse {
  items: Inscription[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

interface PagedMatchResponse {
  items: Match[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class TournoiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/tournois`;

  getAll(page = 1, pageSize = 10, jeuId?: number) {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (jeuId) {
      params = params.set('jeuId', jeuId.toString());
    }
    return this.http.get<PagedTournoiResponse>(this.url, { params });
  }

  getById(id: number) {
    return this.http.get<Tournoi>(`${this.url}/${id}`);
  }

  create(dto: CreateTournoiDto) {
    const body: CreateTournoiDto = {
      nom: dto.nom,
      dateDebut: dto.dateDebut,
      dateFin: dto.dateFin,
      dotation: dto.dotation,
      statut: dto.statut,
      jeuId: dto.jeuId
    };
    return this.http.post<Tournoi>(this.url, body);
  }

  update(id: number, dto: UpdateTournoiDto) {
    const body: UpdateTournoiDto = {
      nom: dto.nom,
      dateDebut: dto.dateDebut,
      dateFin: dto.dateFin,
      dotation: dto.dotation,
      statut: dto.statut,
      jeuId: dto.jeuId
    };
    return this.http.put<Tournoi>(`${this.url}/${id}`, body);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  getInscriptions(tournoiId: number, page = 1, pageSize = 10) {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedInscriptionResponse>(
      `${this.url}/${tournoiId}/inscriptions`,
      { params }
    );
  }

  createInscription(tournoiId: number, dto: CreateInscriptionDto) {
    return this.http.post<Inscription>(
      `${this.url}/${tournoiId}/inscriptions`,
      dto
    );
  }

  deleteInscription(inscriptionId: number) {
    return this.http.delete<void>(
      `${environment.apiUrl}/inscriptions/${inscriptionId}`
    );
  }

  getMatchs(tournoiId: number, page = 1, pageSize = 10) {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedMatchResponse>(
      `${this.url}/${tournoiId}/matchs`,
      { params }
    );
  }

  getClassement(tournoiId: number) {
    return this.http.get<ClassementDto[]>(
      `${this.url}/${tournoiId}/classement`
    );
  }
}