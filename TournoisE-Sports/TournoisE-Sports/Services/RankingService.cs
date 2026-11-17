using TournoisE_Sports.DTOs;
using TournoisE_Sports.Models;
using TournoisE_Sports.Repositories.Interfaces;

namespace TournoisE_Sports.Services;

public interface IRankingService
{
    Task<List<ClassementDto>> GetClassementAsync(int tournoiId, CancellationToken ct = default);
}

public class RankingService : IRankingService
{
    private readonly IInscriptionRepository _inscriptions;
    private readonly IMatchRepository _matchs;

    public RankingService(IInscriptionRepository inscriptions, IMatchRepository matchs)
    {
        _inscriptions = inscriptions;
        _matchs = matchs;
    }

    public async Task<List<ClassementDto>> GetClassementAsync(int tournoiId, CancellationToken ct = default)
    {
        var inscriptions = await _inscriptions.GetByTournoiAsync(tournoiId, ct);
        var matchs = await _matchs.GetByTournoiTermineAsync(tournoiId, ct);

        var classements = inscriptions.Select(i => new ClassementDto
        {
            EquipeId = i.EquipeId,
            NomEquipe = i.Equipe?.Nom ?? "?",
            TagEquipe = i.Equipe?.Tag,
        }).ToDictionary(c => c.EquipeId);

        foreach (var match in matchs)
        {
            if (match.ScoreEquipeA is null || match.ScoreEquipeB is null)
                continue;

            if (!classements.TryGetValue(match.EquipeAId, out var a))
            {
                a = new ClassementDto { EquipeId = match.EquipeAId, NomEquipe = match.EquipeA?.Nom ?? "?" };
                classements[match.EquipeAId] = a;
            }
            if (!classements.TryGetValue(match.EquipeBId, out var b))
            {
                b = new ClassementDto { EquipeId = match.EquipeBId, NomEquipe = match.EquipeB?.Nom ?? "?" };
                classements[match.EquipeBId] = b;
            }

            a.MatchsJoues++;
            b.MatchsJoues++;
            a.PointsMarques += match.ScoreEquipeA.Value;
            a.PointsEncaisses += match.ScoreEquipeB.Value;
            b.PointsMarques += match.ScoreEquipeB.Value;
            b.PointsEncaisses += match.ScoreEquipeA.Value;

            if (match.VainqueurId == match.EquipeAId)
            {
                a.Victoires++;
                a.Points += 3;
                b.Defaites++;
            }
            else if (match.VainqueurId == match.EquipeBId)
            {
                b.Victoires++;
                b.Points += 3;
                a.Defaites++;
            }
        }

        foreach (var c in classements.Values)
            c.DifferencePoints = c.PointsMarques - c.PointsEncaisses;

        var ordered = classements.Values
            .OrderByDescending(c => c.Points)
            .ThenByDescending(c => c.DifferencePoints)
            .ThenByDescending(c => c.PointsMarques)
            .ThenBy(c => c.NomEquipe)
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
            ordered[i].Rang = i + 1;

        return ordered;
    }
}
