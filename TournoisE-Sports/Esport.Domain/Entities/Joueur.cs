namespace Esport.Domain.Entities;

/// <summary>
/// Joueur individuel appartenant � une �quipe.
/// </summary>
public class Joueur : BaseEntity
{
    public string Pseudo { get; set; } = string.Empty;
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Email { get; set; }

    public int EquipeId { get; set; }
    public Equipe? Equipe { get; set; }
}
