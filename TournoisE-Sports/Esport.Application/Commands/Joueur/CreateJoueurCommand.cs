using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Commands.Joueur;

public record CreateJoueurCommand(CreateJoueurDto Dto) : IRequest<JoueurDto>;

public class CreateJoueurCommandHandler : IRequestHandler<CreateJoueurCommand, JoueurDto>
{
    private readonly IUnitOfWork _uow;
    public CreateJoueurCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<JoueurDto> Handle(CreateJoueurCommand request, CancellationToken ct)
    {
        var equipe = await _uow.Equipes.GetByIdAsync(request.Dto.EquipeId, ct)
            ?? throw new NotFoundException($"Equipe {request.Dto.EquipeId} introuvable.");

        var joueur = new Esport.Domain.Entities.Joueur
        {
            Pseudo = request.Dto.Pseudo,
            Nom = request.Dto.Nom,
            Prenom = request.Dto.Prenom,
            Email = request.Dto.Email,
            EquipeId = request.Dto.EquipeId
        };

        await _uow.Joueurs.AddAsync(joueur, ct);
        await _uow.SaveChangesAsync(ct);

        return new JoueurDto
        {
            Id = joueur.Id, Pseudo = joueur.Pseudo, Nom = joueur.Nom,
            Prenom = joueur.Prenom, Email = joueur.Email,
            EquipeId = joueur.EquipeId, NomEquipe = equipe.Nom
        };
    }
}

public record UpdateJoueurCommand(int Id, UpdateJoueurDto Dto) : IRequest<JoueurDto>;

public class UpdateJoueurCommandHandler : IRequestHandler<UpdateJoueurCommand, JoueurDto>
{
    private readonly IUnitOfWork _uow;
    public UpdateJoueurCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<JoueurDto> Handle(UpdateJoueurCommand request, CancellationToken ct)
    {
        var joueur = await _uow.Joueurs.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Joueur {request.Id} introuvable.");

        var equipe = await _uow.Equipes.GetByIdAsync(request.Dto.EquipeId, ct)
            ?? throw new NotFoundException($"Equipe {request.Dto.EquipeId} introuvable.");

        joueur.Pseudo = request.Dto.Pseudo;
        joueur.Nom = request.Dto.Nom;
        joueur.Prenom = request.Dto.Prenom;
        joueur.Email = request.Dto.Email;
        joueur.EquipeId = request.Dto.EquipeId;

        await _uow.Joueurs.UpdateAsync(joueur, ct);
        await _uow.SaveChangesAsync(ct);

        return new JoueurDto
        {
            Id = joueur.Id, Pseudo = joueur.Pseudo, Nom = joueur.Nom,
            Prenom = joueur.Prenom, Email = joueur.Email,
            EquipeId = joueur.EquipeId, NomEquipe = equipe.Nom
        };
    }
}

public record DeleteJoueurCommand(int Id) : IRequest;

public class DeleteJoueurCommandHandler : IRequestHandler<DeleteJoueurCommand>
{
    private readonly IUnitOfWork _uow;
    public DeleteJoueurCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(DeleteJoueurCommand request, CancellationToken ct)
    {
        var joueur = await _uow.Joueurs.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Joueur {request.Id} introuvable.");

        await _uow.Joueurs.DeleteAsync(joueur, ct);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
