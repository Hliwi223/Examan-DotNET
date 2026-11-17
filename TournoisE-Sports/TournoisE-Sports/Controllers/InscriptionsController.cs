using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TournoisE_Sports.DTOs;
using TournoisE_Sports.Repositories.Interfaces;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InscriptionsController : ControllerBase
{
    private readonly IInscriptionRepository _inscriptions;

    public InscriptionsController(IInscriptionRepository inscriptions) => _inscriptions = inscriptions;

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var inscription = await _inscriptions.GetByIdAsync(id, ct);
        if (inscription is null) return NotFound(new { message = $"Inscription {id} introuvable." });
        await _inscriptions.DeleteAsync(inscription, ct);
        return NoContent();
    }
}
