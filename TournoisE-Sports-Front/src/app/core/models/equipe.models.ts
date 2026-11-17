export interface Equipe {
  id: number;
  nom: string;
  tag?: string | null;
  dateCreation: string;
  nombreJoueurs: number;
}

export interface CreateEquipeDto {
  nom: string;
  tag?: string | null;
}

export interface UpdateEquipeDto extends CreateEquipeDto {}