export interface Match {
  id: number;
  dateMatch: string;
  scoreEquipeA?: number | null;
  scoreEquipeB?: number | null;
  termine: boolean;
  tournoiId: number;
  nomTournoi?: string | null;
  equipeAId: number;
  nomEquipeA?: string | null;
  equipeBId: number;
  nomEquipeB?: string | null;
  vainqueurId?: number | null;
  nomVainqueur?: string | null;
}

export interface CreateMatchDto {
  dateMatch: string;
  tournoiId: number;
  equipeAId: number;
  equipeBId: number;
}

export interface UpdateScoreDto {
  scoreEquipeA: number;
  scoreEquipeB: number;
}