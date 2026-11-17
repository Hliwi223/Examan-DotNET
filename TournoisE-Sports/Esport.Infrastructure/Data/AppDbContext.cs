using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Esport.Domain.Entities;

namespace Esport.Infrastructure.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Jeu> Jeux => Set<Jeu>();
    public DbSet<Equipe> Equipes => Set<Equipe>();
    public DbSet<Joueur> Joueurs => Set<Joueur>();
    public DbSet<Tournoi> Tournois => Set<Tournoi>();
    public DbSet<Inscription> Inscriptions => Set<Inscription>();
    public DbSet<Domain.Entities.Match> Matchs => Set<Domain.Entities.Match>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Inscription : unique (TournoiId, EquipeId)
        modelBuilder.Entity<Inscription>()
            .HasIndex(i => new { i.TournoiId, i.EquipeId })
            .IsUnique();

        // Joueur -> Equipe
        modelBuilder.Entity<Joueur>()
            .HasOne(j => j.Equipe)
            .WithMany(e => e.Joueurs)
            .HasForeignKey(j => j.EquipeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tournoi -> Jeu
        modelBuilder.Entity<Tournoi>()
            .HasOne(t => t.Jeu)
            .WithMany(j => j.Tournois)
            .HasForeignKey(t => t.JeuId)
            .OnDelete(DeleteBehavior.Restrict);

        // Inscription -> Tournoi (Cascade) / Equipe (Restrict)
        modelBuilder.Entity<Inscription>()
            .HasOne(i => i.Tournoi)
            .WithMany(t => t.Inscriptions)
            .HasForeignKey(i => i.TournoiId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Inscription>()
            .HasOne(i => i.Equipe)
            .WithMany(e => e.Inscriptions)
            .HasForeignKey(i => i.EquipeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Match : plusieurs relations vers Equipe avec Restrict (évite les chemins de cascade multiples)
        modelBuilder.Entity<Domain.Entities.Match>()
            .HasOne(m => m.Tournoi)
            .WithMany(t => t.Matchs)
            .HasForeignKey(m => m.TournoiId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Domain.Entities.Match>()
            .HasOne(m => m.EquipeA)
            .WithMany()
            .HasForeignKey(m => m.EquipeAId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Domain.Entities.Match>()
            .HasOne(m => m.EquipeB)
            .WithMany()
            .HasForeignKey(m => m.EquipeBId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Domain.Entities.Match>()
            .HasOne(m => m.Vainqueur)
            .WithMany()
            .HasForeignKey(m => m.VainqueurId)
            .OnDelete(DeleteBehavior.Restrict);

        // Index pour les requêtes fréquentes
        modelBuilder.Entity<Domain.Entities.Match>()
            .HasIndex(m => m.TournoiId);

        modelBuilder.Entity<Joueur>()
            .HasIndex(j => j.EquipeId);

        modelBuilder.Entity<Tournoi>()
            .HasIndex(t => t.JeuId);

        // Précision décimale explicite pour Dotation
        modelBuilder.Entity<Tournoi>()
            .Property(t => t.Dotation)
            .HasPrecision(18, 2);
    }
}
