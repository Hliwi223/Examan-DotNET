using System.ComponentModel.DataAnnotations;

namespace TournoisE_Sports.DTOs;

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
    [Required, MinLength(2), MaxLength(50)]
    public string Pseudo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Nom { get; set; }

    [MaxLength(50)]
    public string? Prenom { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "EquipeId invalide")]
    public int EquipeId { get; set; }
}

public class UpdateJoueurDto
{
    [Required, MinLength(2), MaxLength(50)]
    public string Pseudo { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Nom { get; set; }

    [MaxLength(50)]
    public string? Prenom { get; set; }

    [EmailAddress]
    public string? Email { get; set; }

    [Required, Range(1, int.MaxValue, ErrorMessage = "EquipeId invalide")]
    public int EquipeId { get; set; }
}
