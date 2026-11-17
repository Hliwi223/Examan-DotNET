using System.ComponentModel.DataAnnotations;

namespace Esport.Application.DTOs;

public class JeuDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Editeur { get; set; }
    public string? Genre { get; set; }
}

public class CreateJeuDto
{
    [Required, MinLength(2), MaxLength(150)]
    public string Nom { get; set; } = string.Empty;
    public string? Editeur { get; set; }
    public string? Genre { get; set; }
}

public class UpdateJeuDto : CreateJeuDto { }
