using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Enums;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Commands.Tournoi;

public record CreateTournoiCommand(CreateTournoiDto Dto) : IRequest<TournoiDto>;

public class CreateTournoiCommandHandler : IRequestHandler<CreateTournoiCommand, TournoiDto>
{
    private readonly IUnitOfWork _uow;
    public CreateTournoiCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TournoiDto> Handle(CreateTournoiCommand request, CancellationToken ct)
    {
        if (request.Dto.DateFin < request.Dto.DateDebut)
            throw new BadRequestException("DateFin doit être postérieure à DateDebut.");

        var jeu = await _uow.Jeux.GetByIdAsync(request.Dto.JeuId, ct)
            ?? throw new BadRequestException($"Jeu {request.Dto.JeuId} introuvable.");

        var tournoi = new Esport.Domain.Entities.Tournoi
        {
            Nom = request.Dto.Nom,
            DateDebut = request.Dto.DateDebut,
            DateFin = request.Dto.DateFin,
            Dotation = request.Dto.Dotation,
            Statut = (TournoiStatut)request.Dto.Statut,
            JeuId = request.Dto.JeuId
        };

        await _uow.Tournois.AddAsync(tournoi, ct);
        await _uow.SaveChangesAsync(ct);

        return new TournoiDto
        {
            Id = tournoi.Id, Nom = tournoi.Nom, DateDebut = tournoi.DateDebut,
            DateFin = tournoi.DateFin, Dotation = tournoi.Dotation, Statut = (int)tournoi.Statut,
            JeuId = tournoi.JeuId, NomJeu = jeu.Nom
        };
    }
}

public record UpdateTournoiCommand(int Id, UpdateTournoiDto Dto) : IRequest<TournoiDto>;

public class UpdateTournoiCommandHandler : IRequestHandler<UpdateTournoiCommand, TournoiDto>
{
    private readonly IUnitOfWork _uow;
    public UpdateTournoiCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TournoiDto> Handle(UpdateTournoiCommand request, CancellationToken ct)
    {
        if (request.Dto.DateFin < request.Dto.DateDebut)
            throw new BadRequestException("DateFin doit être postérieure à DateDebut.");

        var tournoi = await _uow.Tournois.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Tournoi {request.Id} introuvable.");

        var jeu = await _uow.Jeux.GetByIdAsync(request.Dto.JeuId, ct)
            ?? throw new BadRequestException($"Jeu {request.Dto.JeuId} introuvable.");

        tournoi.Nom = request.Dto.Nom;
        tournoi.DateDebut = request.Dto.DateDebut;
        tournoi.DateFin = request.Dto.DateFin;
        tournoi.Dotation = request.Dto.Dotation;
        tournoi.Statut = (TournoiStatut)request.Dto.Statut;
        tournoi.JeuId = request.Dto.JeuId;

        await _uow.Tournois.UpdateAsync(tournoi, ct);
        await _uow.SaveChangesAsync(ct);

        return new TournoiDto
        {
            Id = tournoi.Id, Nom = tournoi.Nom, DateDebut = tournoi.DateDebut,
            DateFin = tournoi.DateFin, Dotation = tournoi.Dotation, Statut = (int)tournoi.Statut,
            JeuId = tournoi.JeuId, NomJeu = jeu.Nom
        };
    }
}

public record DeleteTournoiCommand(int Id) : IRequest;

public class DeleteTournoiCommandHandler : IRequestHandler<DeleteTournoiCommand>
{
    private readonly IUnitOfWork _uow;
    public DeleteTournoiCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(DeleteTournoiCommand request, CancellationToken ct)
    {
        var tournoi = await _uow.Tournois.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Tournoi {request.Id} introuvable.");

        var matchs = await _uow.Matchs.GetByTournoiAsync(request.Id, ct);
        if (matchs.Any())
            throw new ConflictException("Impossible de supprimer : ce tournoi contient des matchs. Supprimez d'abord les matchs.");

        await _uow.Tournois.DeleteAsync(tournoi, ct);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public record CreateInscriptionCommand(int TournoiId, CreateInscriptionDto Dto) : IRequest<InscriptionDto>;

public class CreateInscriptionCommandHandler : IRequestHandler<CreateInscriptionCommand, InscriptionDto>
{
    private readonly IUnitOfWork _uow;
    public CreateInscriptionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<InscriptionDto> Handle(CreateInscriptionCommand request, CancellationToken ct)
    {
        var tournoi = await _uow.Tournois.GetByIdAsync(request.TournoiId, ct)
            ?? throw new NotFoundException($"Tournoi {request.TournoiId} introuvable.");

        var exist = await _uow.Inscriptions.GetByTournoiEtEquipeAsync(request.TournoiId, request.Dto.EquipeId, ct);
        if (exist is not null)
            throw new ConflictException("Cette équipe est déjà inscrite à ce tournoi.");

        var inscription = new Esport.Domain.Entities.Inscription
        {
            TournoiId = request.TournoiId,
            EquipeId = request.Dto.EquipeId,
            DateInscription = DateTime.UtcNow
        };

        await _uow.Inscriptions.AddAsync(inscription, ct);
        await _uow.SaveChangesAsync(ct);

        var saved = await _uow.Inscriptions.GetByTournoiEtEquipeAsync(request.TournoiId, request.Dto.EquipeId, ct);
        return new InscriptionDto
        {
            Id = inscription.Id, DateInscription = inscription.DateInscription,
            TournoiId = request.TournoiId, EquipeId = request.Dto.EquipeId,
            NomTournoi = tournoi.Nom, NomEquipe = saved?.Equipe?.Nom, TagEquipe = saved?.Equipe?.Tag
        };
    }
}

public record DeleteInscriptionCommand(int Id) : IRequest;

public class DeleteInscriptionCommandHandler : IRequestHandler<DeleteInscriptionCommand>
{
    private readonly IUnitOfWork _uow;
    public DeleteInscriptionCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(DeleteInscriptionCommand request, CancellationToken ct)
    {
        var inscription = await _uow.Inscriptions.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Inscription {request.Id} introuvable.");

        await _uow.Inscriptions.DeleteAsync(inscription, ct);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
