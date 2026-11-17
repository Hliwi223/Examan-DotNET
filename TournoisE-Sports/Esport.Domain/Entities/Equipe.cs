namespace Esport.Domain.Entities;

/// <summary>
/// �quipe de joueurs participant aux tournois.
/// </summary>
public class Equipe : BaseEntity
{
    public string Nom { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    public ICollection<Joueur> Joueurs { get; set; } = new List<Joueur>();
    public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
}
