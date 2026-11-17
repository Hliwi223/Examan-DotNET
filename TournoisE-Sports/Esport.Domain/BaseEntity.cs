namespace Esport.Domain;

/// <summary>
/// Entit� de base avec un identifiant num�rique.
/// Toutes les entit�s du domaine h�ritent de cette classe.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; protected set; }
}
