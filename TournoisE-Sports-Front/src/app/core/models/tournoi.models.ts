export enum TournoiStatut {
  AVenir = 0,
  EnCours = 1,
  Termine = 2
}

export interface Tournoi {
  id: number;
  nom: string;
  dateDebut: string;
  dateFin: string;
  dotation: number;
  statut: TournoiStatut;
  jeuId: number;
  nomJeu?: string | null;
  nombreEquipesInscrites: number;
  nombreMatchs: number;
}

export interface CreateTournoiDto {
  nom: string;
  dateDebut: string;
  dateFin: string;
  dotation: number;
  statut: TournoiStatut;
  jeuId: number;
}

export interface UpdateTournoiDto extends CreateTournoiDto {}

export interface ClassementDto {
  rang: number;
  equipeId: number;
  nomEquipe: string;
  tagEquipe?: string | null;
  victoires: number;
  defaites: number;
  matchsJoues: number;
  pointsMarques: number;
  pointsEncaisses: number;
  differencePoints: number;
  points: number;
}