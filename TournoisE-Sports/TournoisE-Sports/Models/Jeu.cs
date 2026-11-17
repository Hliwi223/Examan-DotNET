namespace TournoisE_Sports.Models;

public class Jeu
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Editeur { get; set; }
    public string? Genre { get; set; }

    public ICollection<Tournoi> Tournois { get; set; } = new List<Tournoi>();
}
