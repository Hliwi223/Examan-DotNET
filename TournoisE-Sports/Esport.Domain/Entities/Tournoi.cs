using Esport.Domain.Enums;

namespace Esport.Domain.Entities;

/// <summary>
/// Tournoi organis� pour un jeu donn�.
/// </summary>
public class Tournoi : BaseEntity
{
    public string Nom { get; set; } = string.Empty;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal Dotation { get; set; }
    public TournoiStatut Statut { get; set; } = TournoiStatut.AVenir;

    public int JeuId { get; set; }
    public Jeu? Jeu { get; set; }

    public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
    public ICollection<Match> Matchs { get; set; } = new List<Match>();
}
