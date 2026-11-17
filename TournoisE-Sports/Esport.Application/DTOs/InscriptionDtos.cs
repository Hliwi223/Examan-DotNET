using System.ComponentModel.DataAnnotations;

namespace Esport.Application.DTOs;

public class InscriptionDto
{
    public int Id { get; set; }
    public DateTime DateInscription { get; set; }
    public int TournoiId { get; set; }
    public string? NomTournoi { get; set; }
    public int EquipeId { get; set; }
    public string? NomEquipe { get; set; }
    public string? TagEquipe { get; set; }
}

public class CreateInscriptionDto
{
    [Required, Range(1, int.MaxValue, ErrorMessage = "EquipeId invalide")]
    public int EquipeId { get; set; }
}
