export interface Joueur {
  id: number;
  pseudo: string;
  nom?: string | null;
  prenom?: string | null;
  email?: string | null;
  equipeId: number;
  nomEquipe?: string | null;
}

export interface CreateJoueurDto {
  pseudo: string;
  nom?: string | null;
  prenom?: string | null;
  email?: string | null;
  equipeId: number;
}

export interface UpdateJoueurDto extends CreateJoueurDto {}