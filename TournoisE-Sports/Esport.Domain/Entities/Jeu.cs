using Esport.Domain.Enums;

namespace Esport.Domain.Entities;

/// <summary>
/// Jeu vid�o sur lequel reposent les tournois (ex: League of Legends, Valorant).
/// </summary>
public class Jeu : BaseEntity
{
    public string Nom { get; set; } = string.Empty;
    public string? Editeur { get; set; }
    public string? Genre { get; set; }

    public ICollection<Tournoi> Tournois { get; set; } = new List<Tournoi>();
}
