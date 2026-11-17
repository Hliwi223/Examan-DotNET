namespace TournoisE_Sports.Models;

public class Equipe
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public ICollection<Joueur> Joueurs { get; set; } = new List<Joueur>();
    public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
}
