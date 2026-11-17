using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Commands.Jeu;

public record DeleteJeuCommand(int Id) : IRequest;

public class DeleteJeuCommandHandler : IRequestHandler<DeleteJeuCommand>
{
    private readonly IUnitOfWork _uow;
    public DeleteJeuCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(DeleteJeuCommand request, CancellationToken ct)
    {
        var jeu = await _uow.Jeux.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Jeu {request.Id} introuvable.");

        await _uow.Jeux.DeleteAsync(jeu, ct);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
