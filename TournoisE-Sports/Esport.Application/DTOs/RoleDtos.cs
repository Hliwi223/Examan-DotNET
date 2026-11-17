using System.ComponentModel.DataAnnotations;

namespace Esport.Application.DTOs;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
}

public class CreateRoleDto
{
    [Required, MinLength(2), MaxLength(50)]
    public string Nom { get; set; } = string.Empty;
}

public class AssignRoleDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(2)]
    public string Role { get; set; } = string.Empty;
}
