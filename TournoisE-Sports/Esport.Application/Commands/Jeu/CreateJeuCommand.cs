using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Commands.Jeu;

/// <summary>Cr�ation d'un jeu (Command c�t� criture).</summary>
public record CreateJeuCommand(CreateJeuDto Dto) : IRequest<JeuDto>;

public class CreateJeuCommandHandler : IRequestHandler<CreateJeuCommand, JeuDto>
{
    private readonly IUnitOfWork _uow;
    public CreateJeuCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<JeuDto> Handle(CreateJeuCommand request, CancellationToken ct)
    {
        if (await _uow.Jeux.ExistsAsync(request.Dto.Nom, null, ct))
            throw new ConflictException($"Un jeu nomm� '{request.Dto.Nom}' existe d�j�.");

        var jeu = new Esport.Domain.Entities.Jeu
        {
            Nom = request.Dto.Nom,
            Editeur = request.Dto.Editeur,
            Genre = request.Dto.Genre
        };

        await _uow.Jeux.AddAsync(jeu, ct);
        await _uow.SaveChangesAsync(ct);

        return new JeuDto { Id = jeu.Id, Nom = jeu.Nom, Editeur = jeu.Editeur, Genre = jeu.Genre };
    }
}
