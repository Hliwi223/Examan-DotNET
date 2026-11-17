export interface Jeu {
  id: number;
  nom: string;
  editeur?: string | null;
  genre?: string | null;
}

export interface CreateJeuDto {
  nom: string;
  editeur?: string | null;
  genre?: string | null;
}

export interface UpdateJeuDto extends CreateJeuDto {}