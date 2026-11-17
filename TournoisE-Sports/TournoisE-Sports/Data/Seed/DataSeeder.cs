using Microsoft.AspNetCore.Identity;
using TournoisE_Sports.Data;
using TournoisE_Sports.Models;

namespace TournoisE_Sports.Data.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration config,
        ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        // --- Rôles ---
        string[] roles = { "Admin", "Organisateur", "Joueur" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var res = await roleManager.CreateAsync(new IdentityRole(role));
                if (!res.Succeeded)
                    logger.LogWarning("Échec création rôle {Role}: {Errors}", role, string.Join(", ", res.Errors.Select(e => e.Description)));
            }
        }

        // --- Admin ---
        var adminEmail = config["SeedAdmin:Email"] ?? config["AdminUser:Email"] ?? "admin@esport.local";
        var adminPassword = config["SeedAdmin:Password"] ?? config["AdminUser:Password"] ?? "ChangeMe!123";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var res = await userManager.CreateAsync(admin, adminPassword);
            if (res.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Admin de seed créé: {Email}", adminEmail);
            }
            else
            {
                logger.LogWarning("Échec création admin: {Errors}", string.Join(", ", res.Errors.Select(e => e.Description)));
            }
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // --- Données d'exemple ---
        if (db.Jeux.Any())
            return;

        logger.LogInformation("Seed des données d'exemple...");

        var jeux = new List<Jeu>
        {
            new() { Nom = "League of Legends", Editeur = "Riot Games", Genre = "MOBA" },
            new() { Nom = "Valorant", Editeur = "Riot Games", Genre = "FPS" },
            new() { Nom = "EA FC 26", Editeur = "EA Sports", Genre = "Sport" },
            new() { Nom = "Rocket League", Editeur = "Psyonix", Genre = "Sport" },
        };
        db.Jeux.AddRange(jeux);
        await db.SaveChangesAsync();

        var equipes = new List<Equipe>
        {
            new() { Nom = "Team Vitality", Tag = "VIT" },
            new() { Nom = "Karmine Corp", Tag = "KC" },
            new() { Nom = "Team Liquid", Tag = "TL" },
            new() { Nom = "G2 Esports", Tag = "G2" },
            new() { Nom = "Fnatic", Tag = "FNC" },
            new() { Nom = "Team Falcons", Tag = "FAL" },
        };
        db.Equipes.AddRange(equipes);
        await db.SaveChangesAsync();

        var joueurs = new List<Joueur>
        {
            new() { Pseudo = "Faker", Nom = "Lee", Prenom = "Sang-hyeok", Email = "faker@team.local", EquipeId = equipes[0].Id },
            new() { Pseudo = "Shroud", Nom = "Grzesiek", Prenom = "Michael", Email = "shroud@team.local", EquipeId = equipes[1].Id },
            new() { Pseudo = "s1mple", Nom = "Kostyliev", Prenom = "Oleksandr", Email = "s1mple@team.local", EquipeId = equipes[2].Id },
            new() { Pseudo = "ZywOo", Nom = "Heraut", Prenom = "Mathieu", Email = "zywoo@team.local", EquipeId = equipes[2].Id },
            new() { Pseudo = "Caps", Nom = "Søkvist", Prenom = "Rasmus", Email = "caps@team.local", EquipeId = equipes[3].Id },
            new() { Pseudo = "Rekkles", Nom = "Lärsson", Prenom = "Martin", Email = "rekles@team.local", EquipeId = equipes[4].Id },
        };
        db.Joueurs.AddRange(joueurs);

        var tournoi = new Tournoi
        {
            Nom = "Spring Cup 2026",
            DateDebut = DateTime.UtcNow.AddDays(7),
            DateFin = DateTime.UtcNow.AddDays(14),
            Dotation = 50000,
            Statut = TournoiStatut.AVenir,
            JeuId = jeux[0].Id
        };
        db.Tournois.Add(tournoi);
        await db.SaveChangesAsync();

        var inscriptions = new List<Inscription>
        {
            new() { TournoiId = tournoi.Id, EquipeId = equipes[0].Id },
            new() { TournoiId = tournoi.Id, EquipeId = equipes[1].Id },
            new() { TournoiId = tournoi.Id, EquipeId = equipes[2].Id },
            new() { TournoiId = tournoi.Id, EquipeId = equipes[3].Id },
        };
        db.Inscriptions.AddRange(inscriptions);

        var match1 = new Match
        {
            DateMatch = DateTime.UtcNow.AddDays(8),
            TournoiId = tournoi.Id,
            EquipeAId = equipes[0].Id,
            EquipeBId = equipes[1].Id,
            ScoreEquipeA = 2,
            ScoreEquipeB = 1,
            Termine = true,
            VainqueurId = equipes[0].Id
        };
        var match2 = new Match
        {
            DateMatch = DateTime.UtcNow.AddDays(9),
            TournoiId = tournoi.Id,
            EquipeAId = equipes[2].Id,
            EquipeBId = equipes[3].Id,
            ScoreEquipeA = 0,
            ScoreEquipeB = 2,
            Termine = true,
            VainqueurId = equipes[3].Id
        };
        db.Matchs.AddRange(new[] { match1, match2 });
        await db.SaveChangesAsync();

        logger.LogInformation("Seed terminé.");
    }
}
