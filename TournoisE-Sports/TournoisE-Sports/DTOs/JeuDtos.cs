using System.ComponentModel.DataAnnotations;

namespace TournoisE_Sports.DTOs;

public class JeuDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Editeur { get; set; }
    public string? Genre { get; set; }
}

public class CreateJeuDto
{
    [Required, MinLength(2), MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Editeur { get; set; }

    [MaxLength(50)]
    public string? Genre { get; set; }
}

public class UpdateJeuDto : CreateJeuDto
{
}
