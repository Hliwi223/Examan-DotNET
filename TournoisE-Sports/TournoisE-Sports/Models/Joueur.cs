namespace TournoisE_Sports.Models;

public class Joueur
{
    public int Id { get; set; }
    public string Pseudo { get; set; } = string.Empty;
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Email { get; set; }

    public int EquipeId { get; set; }
    public Equipe? Equipe { get; set; }
}
