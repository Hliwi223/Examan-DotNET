using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TournoisE_Sports.DTOs;

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
        var roles = _roleManager.Roles.Select(r => new RoleDto { Id = r.Id, Name = r.Name ?? string.Empty }).ToList();
        return Ok(roles);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        if (await _roleManager.RoleExistsAsync(dto.Name))
            return Conflict(new { message = $"Le rôle '{dto.Name}' existe déjà." });

        var role = new IdentityRole(dto.Name);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return StatusCode(StatusCodes.Status201Created, new RoleDto { Id = role.Id, Name = role.Name! });
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
            return NotFound(new { message = $"Utilisateur '{dto.Email}' introuvable." });

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return BadRequest(new { message = $"Le rôle '{dto.Role}' n'existe pas." });

        if (await _userManager.IsInRoleAsync(user, dto.Role))
            return Conflict(new { message = $"L'utilisateur a déjà le rôle '{dto.Role}'." });

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
            return BadRequest(new { message = $"L'utilisateur ne possède pas le rôle '{dto.Role}'." });

        var result = await _userManager.RemoveFromRoleAsync(user, dto.Role);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        return NoContent();
    }
}
