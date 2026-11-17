using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TournoisE_Sports.DTOs;
using TournoisE_Sports.Services;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IJwtService _jwtService;

    public AuthController(UserManager<IdentityUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing is not null)
            return Conflict(new { message = $"L'utilisateur {dto.Email} existe déjà." });

        var user = new IdentityUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        // Rôle par défaut "Joueur"
        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
        {
            var roleCheck = await _userManager.IsInRoleAsync(user, "Joueur");
            if (!roleCheck && await _userManager.FindByEmailAsync(dto.Email) is not null)
            {
                // Le rôle peut ne pas exister encore si Seed n'a pas tourné — on ignore l'erreur
                await _userManager.AddToRoleAsync(user, "Joueur");
            }
        }

        return StatusCode(StatusCodes.Status201Created, new { message = "Compte créé. Connectez-vous pour obtenir votre token." });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return Unauthorized(new { message = "Email ou mot de passe incorrect." });

        var ok = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!ok)
            return Unauthorized(new { message = "Email ou mot de passe incorrect." });

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var (token, expires) = _jwtService.CreateToken(user, roles);
        return Ok(new AuthResponseDto
        {
            Token = token,
            Email = user.Email ?? string.Empty,
            Roles = roles,
            Expires = expires
        });
    }
}
