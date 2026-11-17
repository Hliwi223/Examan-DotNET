using System.ComponentModel.DataAnnotations;

namespace Esport.Application.DTOs;

public class JoueurDto
{
    public int Id { get; set; }
    public string Pseudo { get; set; } = string.Empty;
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Email { get; set; }
    public int EquipeId { get; set; }
    public string? NomEquipe { get; set; }
}

public class CreateJoueurDto
{
    [Required, MinLength(2), MaxLength(100)]
    public string Pseudo { get; set; } = string.Empty;
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public string? Email { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int EquipeId { get; set; }
}

public class UpdateJoueurDto : CreateJoueurDto { }
