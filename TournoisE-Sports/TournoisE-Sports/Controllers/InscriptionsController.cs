using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Esport.Application.Commands.Tournoi;
using MediatR;

namespace TournoisE_Sports.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public InscriptionsController(IMediator mediator) => _mediator = mediator;

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Organisateur")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _mediator.Send(new DeleteInscriptionCommand(id), ct);
        return NoContent();
    }
}
