export interface Inscription {
  id: number;
  dateInscription: string;
  tournoiId: number;
  nomTournoi?: string | null;
  equipeId: number;
  nomEquipe?: string | null;
  tagEquipe?: string | null;
}

export interface CreateInscriptionDto {
  equipeId: number;
}