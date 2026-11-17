import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import {
  Match,
  CreateMatchDto,
  UpdateScoreDto
} from '../models/match.models';

@Injectable({ providedIn: 'root' })
export class MatchService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/matchs`;

  getById(id: number) {
    return this.http.get<Match>(`${this.url}/${id}`);
  }

  create(dto: CreateMatchDto) {
    const body: CreateMatchDto = {
      dateMatch: dto.dateMatch,
      tournoiId: dto.tournoiId,
      equipeAId: dto.equipeAId,
      equipeBId: dto.equipeBId
    };
    return this.http.post<Match>(this.url, body);
  }

  updateScore(id: number, dto: UpdateScoreDto) {
    return this.http.put<Match>(`${this.url}/${id}/score`, dto);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}