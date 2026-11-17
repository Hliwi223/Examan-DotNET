using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Esport.Application.DTOs;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;

    public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult GetRoles()
    {
        var roles = _roleManager.Roles.Select(r => new RoleDto { Id = r.Id, Nom = r.Name ?? string.Empty }).ToList();
        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        if (await _roleManager.RoleExistsAsync(dto.Nom))
            return Conflict(new { message = $"Le r\u00f4le '{dto.Nom}' existe d\u00e9j\u00e0." });

        var role = new IdentityRole(dto.Nom);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return StatusCode(StatusCodes.Status201Created, new RoleDto { Id = role.Id, Nom = role.Name! });
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return NotFound(new { message = $"Utilisateur '{dto.Email}' introuvable." });

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return BadRequest(new { message = $"Le r\u00f4le '{dto.Role}' n'existe pas." });

        if (await _userManager.IsInRoleAsync(user, dto.Role))
            return Conflict(new { message = $"L'utilisateur a d\u00e9j\u00e0 le r\u00f4le '{dto.Role}'." });

        var result = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return NotFound(new { message = $"Utilisateur '{dto.Email}' introuvable." });

        if (!await _userManager.IsInRoleAsync(user, dto.Role))
            return BadRequest(new { message = $"L'utilisateur ne poss\u00e8de pas le r\u00f4le '{dto.Role}'." });

        var result = await _userManager.RemoveFromRoleAsync(user, dto.Role);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }
}
