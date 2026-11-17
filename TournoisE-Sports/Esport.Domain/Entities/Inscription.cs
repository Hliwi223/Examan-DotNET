namespace Esport.Domain.Entities;

/// <summary>
/// Inscription d'une �quipe � un tournoi.
/// </summary>
public class Inscription : BaseEntity
{
    public DateTime DateInscription { get; set; } = DateTime.UtcNow;

    public int TournoiId { get; set; }
    public Tournoi? Tournoi { get; set; }

    public int EquipeId { get; set; }
    public Equipe? Equipe { get; set; }
}
