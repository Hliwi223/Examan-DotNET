using Esport.Application.DTOs;
using Esport.Application.Interfaces;
using Esport.Domain.Entities;
using Esport.Domain.Exceptions;
using MediatR;

namespace Esport.Application.Commands.Match;

public record CreateMatchCommand(CreateMatchDto Dto) : IRequest<MatchDto>;

public class CreateMatchCommandHandler : IRequestHandler<CreateMatchCommand, MatchDto>
{
    private readonly IUnitOfWork _uow;
    public CreateMatchCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<MatchDto> Handle(CreateMatchCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        if (dto.EquipeAId == dto.EquipeBId)
            throw new BadRequestException("Une équipe ne peut pas jouer contre elle-même.");

        var tournoi = await _uow.Tournois.GetByIdAsync(dto.TournoiId, ct)
            ?? throw new BadRequestException($"Tournoi {dto.TournoiId} introuvable.");

        var equipeA = await _uow.Equipes.GetByIdAsync(dto.EquipeAId, ct)
            ?? throw new BadRequestException($"Equipe {dto.EquipeAId} introuvable.");
        var equipeB = await _uow.Equipes.GetByIdAsync(dto.EquipeBId, ct)
            ?? throw new BadRequestException($"Equipe {dto.EquipeBId} introuvable.");

        var inscA = await _uow.Inscriptions.GetByTournoiEtEquipeAsync(dto.TournoiId, dto.EquipeAId, ct);
        var inscB = await _uow.Inscriptions.GetByTournoiEtEquipeAsync(dto.TournoiId, dto.EquipeBId, ct);
        if (inscA is null || inscB is null)
            throw new BadRequestException("Les deux équipes doivent être inscrites au tournoi.");

        var match = new Esport.Domain.Entities.Match
        {
            DateMatch = dto.DateMatch,
            TournoiId = dto.TournoiId,
            EquipeAId = dto.EquipeAId,
            EquipeBId = dto.EquipeBId,
            Termine = false
        };

        await _uow.Matchs.AddAsync(match, ct);
        await _uow.SaveChangesAsync(ct);

        return ToDto(match, tournoi.Nom, equipeA.Nom, equipeB.Nom);
    }

    private static MatchDto ToDto(Esport.Domain.Entities.Match m, string nomTournoi, string nomA, string nomB) => new()
    {
        Id = m.Id, DateMatch = m.DateMatch, ScoreEquipeA = m.ScoreEquipeA, ScoreEquipeB = m.ScoreEquipeB,
        Termine = m.Termine, TournoiId = m.TournoiId, NomTournoi = nomTournoi,
        EquipeAId = m.EquipeAId, NomEquipeA = nomA, EquipeBId = m.EquipeBId, NomEquipeB = nomB,
        VainqueurId = m.VainqueurId, NomVainqueur = m.Vainqueur?.Nom
    };
}

public record UpdateScoreCommand(int Id, UpdateScoreDto Dto) : IRequest<MatchDto>;

public class UpdateScoreCommandHandler : IRequestHandler<UpdateScoreCommand, MatchDto>
{
    private readonly IUnitOfWork _uow;
    public UpdateScoreCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<MatchDto> Handle(UpdateScoreCommand request, CancellationToken ct)
    {
        var match = await _uow.Matchs.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Match {request.Id} introuvable.");

        if (request.Dto.ScoreEquipeA == request.Dto.ScoreEquipeB)
            throw new BadRequestException("Le score ne peut pas être nul : il doit y avoir un vainqueur (égalité refusée).");

        match.ScoreEquipeA = request.Dto.ScoreEquipeA;
        match.ScoreEquipeB = request.Dto.ScoreEquipeB;
        match.Termine = true;
        match.VainqueurId = request.Dto.ScoreEquipeA > request.Dto.ScoreEquipeB
            ? match.EquipeAId
            : match.EquipeBId;

        await _uow.Matchs.UpdateAsync(match, ct);
        await _uow.SaveChangesAsync(ct);

        return ToDto(match);
    }

    private static MatchDto ToDto(Esport.Domain.Entities.Match m) => new()
    {
        Id = m.Id, DateMatch = m.DateMatch, ScoreEquipeA = m.ScoreEquipeA, ScoreEquipeB = m.ScoreEquipeB,
        Termine = m.Termine, TournoiId = m.TournoiId, NomTournoi = m.Tournoi?.Nom,
        EquipeAId = m.EquipeAId, NomEquipeA = m.EquipeA?.Nom,
        EquipeBId = m.EquipeBId, NomEquipeB = m.EquipeB?.Nom,
        VainqueurId = m.VainqueurId, NomVainqueur = m.Vainqueur?.Nom
    };
}

public record DeleteMatchCommand(int Id) : IRequest;

public class DeleteMatchCommandHandler : IRequestHandler<DeleteMatchCommand>
{
    private readonly IUnitOfWork _uow;
    public DeleteMatchCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Unit> Handle(DeleteMatchCommand request, CancellationToken ct)
    {
        var match = await _uow.Matchs.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Match {request.Id} introuvable.");

        await _uow.Matchs.DeleteAsync(match, ct);
        await _uow.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
