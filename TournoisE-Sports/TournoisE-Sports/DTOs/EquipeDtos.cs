using System.ComponentModel.DataAnnotations;

namespace TournoisE_Sports.DTOs;

public class EquipeDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public DateTime DateCreation { get; set; }
    public int NombreJoueurs { get; set; }
}

public class CreateEquipeDto
{
    [Required, MinLength(2), MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? Tag { get; set; }
}

public class UpdateEquipeDto : CreateEquipeDto
{
}
