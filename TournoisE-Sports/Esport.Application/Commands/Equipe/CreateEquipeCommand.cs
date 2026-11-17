using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Commands.Equipe;

public record CreateEquipeCommand(CreateEquipeDto Dto) : IRequest<EquipeDto>;

public class CreateEquipeCommandHandler : IRequestHandler<CreateEquipeCommand, EquipeDto>
{
    private readonly IUnitOfWork _uow;
    public CreateEquipeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EquipeDto> Handle(CreateEquipeCommand request, CancellationToken ct)
    {
        var equipe = new Esport.Domain.Entities.Equipe
        {
            Nom = request.Dto.Nom,
            Tag = request.Dto.Tag,
            DateCreation = DateTime.UtcNow
        };

        await _uow.Equipes.AddAsync(equipe, ct);
        await _uow.SaveChangesAsync(ct);

        return new EquipeDto
        {
            Id = equipe.Id, Nom = equipe.Nom, Tag = equipe.Tag,
            DateCreation = equipe.DateCreation, NombreJoueurs = 0
        };
    }
}

public record UpdateEquipeCommand(int Id, UpdateEquipeDto Dto) : IRequest<EquipeDto>;

public class UpdateEquipeCommandHandler : IRequestHandler<UpdateEquipeCommand, EquipeDto>
{
    private readonly IUnitOfWork _uow;
    public UpdateEquipeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EquipeDto> Handle(UpdateEquipeCommand request, CancellationToken ct)
    {
        var equipe = await _uow.Equipes.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Equipe {request.Id} introuvable.");

        equipe.Nom = request.Dto.Nom;
        equipe.Tag = request.Dto.Tag;

        await _uow.Equipes.UpdateAsync(equipe, ct);
        await _uow.SaveChangesAsync(ct);

        return new EquipeDto
        {
            Id = equipe.Id, Nom = equipe.Nom, Tag = equipe.Tag,
            DateCreation = equipe.DateCreation, NombreJoueurs = equipe.Joueurs?.Count ?? 0
        };
    }
}

public record DeleteEquipeCommand(int Id) : IRequest;

public class DeleteEquipeCommandHandler : IRequestHandler<DeleteEquipeCommand>
{
    private readonly IUnitOfWork _uow;
    public DeleteEquipeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(DeleteEquipeCommand request, CancellationToken ct)
    {
        var equipe = await _uow.Equipes.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Equipe {request.Id} introuvable.");

        await _uow.Equipes.DeleteAsync(equipe, ct);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
