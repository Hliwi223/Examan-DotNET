using System.ComponentModel.DataAnnotations;

namespace Esport.Application.DTOs;

public class MatchDto
{
    public int Id { get; set; }
    public DateTime DateMatch { get; set; }
    public int? ScoreEquipeA { get; set; }
    public int? ScoreEquipeB { get; set; }
    public bool Termine { get; set; }
    public int TournoiId { get; set; }
    public string? NomTournoi { get; set; }
    public int EquipeAId { get; set; }
    public string? NomEquipeA { get; set; }
    public int EquipeBId { get; set; }
    public string? NomEquipeB { get; set; }
    public int? VainqueurId { get; set; }
    public string? NomVainqueur { get; set; }
}

public class CreateMatchDto
{
    [Required]
    public DateTime DateMatch { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "TournoiId invalide")]
    public int TournoiId { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "EquipeAId invalide")]
    public int EquipeAId { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "EquipeBId invalide")]
    public int EquipeBId { get; set; }
}

public class UpdateScoreDto
{
    [Required, Range(0, 10000, ErrorMessage = "Score invalide")]
    public int ScoreEquipeA { get; set; }

    [Required, Range(0, 10000, ErrorMessage = "Score invalide")]
    public int ScoreEquipeB { get; set; }
}
