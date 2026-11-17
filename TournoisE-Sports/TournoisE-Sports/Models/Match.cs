namespace TournoisE_Sports.Models;

public class Match
{
    public int Id { get; set; }
    public DateTime DateMatch { get; set; }
    public int? ScoreEquipeA { get; set; }
    public int? ScoreEquipeB { get; set; }
    public bool Termine { get; set; } = false;

    public int TournoiId { get; set; }
    public Tournoi? Tournoi { get; set; }

    public int EquipeAId { get; set; }
    public Equipe? EquipeA { get; set; }

    public int EquipeBId { get; set; }
    public Equipe? EquipeB { get; set; }

    public int? VainqueurId { get; set; }
    public Equipe? Vainqueur { get; set; }
}
