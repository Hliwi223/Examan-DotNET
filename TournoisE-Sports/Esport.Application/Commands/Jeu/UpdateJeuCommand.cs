using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Commands.Jeu;

/// <summary>Modification d'un jeu.</summary>
public record UpdateJeuCommand(int Id, UpdateJeuDto Dto) : IRequest<JeuDto>;

public class UpdateJeuCommandHandler : IRequestHandler<UpdateJeuCommand, JeuDto>
{
    private readonly IUnitOfWork _uow;
    public UpdateJeuCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<JeuDto> Handle(UpdateJeuCommand request, CancellationToken ct)
    {
        var jeu = await _uow.Jeux.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Jeu {request.Id} introuvable.");

        if (await _uow.Jeux.ExistsAsync(request.Dto.Nom, request.Id, ct))
            throw new ConflictException($"Un jeu nomm� '{request.Dto.Nom}' existe d�j�.");

        jeu.Nom = request.Dto.Nom;
        jeu.Editeur = request.Dto.Editeur;
        jeu.Genre = request.Dto.Genre;

        await _uow.Jeux.UpdateAsync(jeu, ct);
        await _uow.SaveChangesAsync(ct);

        return new JeuDto { Id = jeu.Id, Nom = jeu.Nom, Editeur = jeu.Editeur, Genre = jeu.Genre };
    }
}
