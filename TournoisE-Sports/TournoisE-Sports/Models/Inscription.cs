namespace TournoisE_Sports.Models;

public class Inscription
{
    public int Id { get; set; }
    public DateTime DateInscription { get; set; } = DateTime.UtcNow;

    public int TournoiId { get; set; }
    public Tournoi? Tournoi { get; set; }

    public int EquipeId { get; set; }
    public Equipe? Equipe { get; set; }
}
