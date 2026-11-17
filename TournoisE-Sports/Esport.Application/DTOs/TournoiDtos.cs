using System.ComponentModel.DataAnnotations;

namespace Esport.Application.DTOs;

public class TournoiDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal Dotation { get; set; }
    public int Statut { get; set; }
    public int JeuId { get; set; }
    public string? NomJeu { get; set; }
    public int NombreEquipesInscrites { get; set; }
    public int NombreMatchs { get; set; }
}

public class CreateTournoiDto
{
    [Required, MinLength(2), MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    [Required]
    public DateTime DateDebut { get; set; }

    [Required]
    public DateTime DateFin { get; set; }

    [Range(0, 10000000)]
    public decimal Dotation { get; set; }

    public int Statut { get; set; } = 0;

    [Required, Range(1, int.MaxValue, ErrorMessage = "JeuId invalide")]
    public int JeuId { get; set; }
}

public class UpdateTournoiDto : CreateTournoiDto { }

public class ClassementDto
{
    public int Rang { get; set; }
    public int EquipeId { get; set; }
    public string NomEquipe { get; set; } = string.Empty;
    public string? TagEquipe { get; set; }
    public int Victoires { get; set; }
    public int Defaites { get; set; }
    public int MatchsJoues { get; set; }
    public int PointsMarques { get; set; }
    public int PointsEncaisses { get; set; }
    public int DifferencePoints { get; set; }
    public int Points { get; set; }
}
