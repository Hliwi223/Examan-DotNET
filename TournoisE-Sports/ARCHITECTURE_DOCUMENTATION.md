# 📖 Documentation — Unit of Work, CQRS + MediatR, Clean Architecture

Ce document explique tout ce qui a été ajouté au projet **TournoisE-Sports** pour répondre aux 3 critères d'architecture :

1. **Unit of Work**
2. **CQRS + MediatR**
3. **Clean Architecture** (séparation en projets : Domain, Application, Infrastructure, Api)

---

## 🗂 Nouvelle structure du projet

```
TournoisE-Sports/                    # racine du dépôt
├── TournoisE-Sports.sln
│
├── Esport.Domain/                   # Couche Domain (métier pur, zéro dépendance infra)
│   ├── BaseEntity.cs                # Classe de base avec Id
│   ├── Enums/
│   │   └── TournoiStatut.cs         # Énumération AVenir / EnCours / Terminé
│   ├── Entities/
│   │   ├── Jeu.cs
│   │   ├── Equipe.cs
│   │   ├── Joueur.cs
│   │   ├── Tournoi.cs
│   │   ├── Inscription.cs
│   │   └── Match.cs
│   └── Exceptions/
│       └── DomainExceptions.cs      # NotFound, Conflict, BadRequest (métier)
│
├── Esport.Application/              # Couche Application (cas d'usage, CQRS)
│   ├── Common/
│   │   └── Pagination.cs            # PaginationParams, PagedResult<T>
│   ├── DTOs/                        # Contrats d'entrée/sortie (DTOs)
│   │   ├── AuthDtos.cs
│   │   ├── JeuDtos.cs
│   │   ├── EquipeDtos.cs
│   │   ├── JoueurDtos.cs
│   │   ├── TournoiDtos.cs
│   │   ├── InscriptionDtos.cs
│   │   ├── MatchDtos.cs
│   │   └── RoleDtos.cs
│   ├── Interfaces/                  # Contrats (repositories, UnitOfWork)
│   │   ├── IRepository.cs           # Repository générique
│   │   ├── IRepositories.cs         # Repositories spécialisés (IJeuRepository, etc.)
│   │   └── IUnitOfWork.cs           # Interface Unit of Work
│   ├── Commands/                    # CQRS — Côté écriture (Command + Handler)
│   │   ├── Jeu/
│   │   ├── Equipe/
│   │   ├── Joueur/
│   │   ├── Tournoi/
│   │   └── Match/
│   └── Queries/                     # CQRS — Côté lecture (Query + Handler)
│       ├── Jeu/
│       ├── Equipe/
│       ├── Joueur/
│       ├── Tournoi/
│       └── Match/
│
├── Esport.Infrastructure/           # Couche Infrastructure (EF Core, DB, Seed)
│   ├── Data/
│   │   ├── AppDbContext.cs          # DbContext (IdentityDbContext + DbSets)
│   │   └── Seed/
│   │       └── DataSeeder.cs        # Seed : rôles, admin, données d'exemple
│   ├── Data/Migrations/             # Migrations EF Core (InitialCreate)
│   └── Repositories/
│       └── Implementations/
│           ├── JeuRepository.cs
│           ├── EquipeRepository.cs
│           ├── JoueurRepository.cs
│           ├── TournoiRepository.cs
│           ├── InscriptionRepository.cs
│           ├── MatchRepository.cs
│           └── UnitOfWork.cs        # Implémentation Unit of Work
│
└── TournoisE-Sports/                # Projet API (Point d'entrée, Présentation)
    ├── Program.cs                   # DI, Identity, JWT, MediatR, Swagger, Middleware
    ├── Controllers/                 # Contrôleurs fins → appellent IMediator
    │   ├── AuthController.cs
    │   ├── RolesController.cs
    │   ├── JeuxController.cs
    │   ├── EquipesController.cs
    │   ├── JoueursController.cs
    │   ├── TournoisController.cs
    │   ├── InscriptionsController.cs
    │   └── MatchsController.cs
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs  # Mappe exceptions domaine → codes HTTP
    ├── Services/
    │   └── JwtService.cs            # Génération/validation tokens JWT
    ├── Properties/launchSettings.json
    └── appsettings.json
```

**Namespaces** : `Esport.Domain.*`, `Esport.Application.*`, `Esport.Infrastructure.*`, `TournoisE_Sports.*`

---

## 1️⃣ Unit of Work

### Qu'est-ce que c'est ?

L'**Unit of Work (UoW)** est un pattern qui maintient une liste d'objets affectés par une transaction métier et coordonne l'écriture de toutes les modifications en **une seule fois** vers la base de données.

### Pourquoi l'ajouter ?

Avant, chaque repository appelait `_db.SaveChangesAsync()` à la fin de chaque méthode (`AddAsync`, `UpdateAsync`, `DeleteAsync`). Cela posait deux problèmes :

- **Plusieurs transactions** pour une opération métier qui devrait être atomique.
- **Duplication** de l'appel à `SaveChangesAsync` dans chaque repository.

### Implémentation

**Interface** (`Esport.Application/Interfaces/IUnitOfWork.cs`) :

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    IJeuRepository Jeux { get; }
    IEquipeRepository Equipes { get; }
    IJoueurRepository Joueurs { get; }
    ITournoiRepository Tournois { get; }
    IInscriptionRepository Inscriptions { get; }
    IMatchRepository Matchs { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

**Implémentation** (`Esport.Infrastructure/Repositories/Implementations/UnitOfWork.cs`) :

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public IJeuRepository Jeux { get; }
    public IEquipeRepository Equipes { get; }
    // ... autres repositories

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
        Jeux = new JeuRepository(db);
        Equipes = new EquipeRepository(db);
        // ...
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _db.SaveChangesAsync(ct);
    }
}
```

**Modification des repositories** : ils ne sauvegardent plus eux-mêmes. Ils marquent l'entité (`.Add`, `.Remove`, `Entry.State = Modified`) et c'est l'UoW qui appelle `SaveChangesAsync()` une seule fois à la fin de la commande.

**Enregistrement DI** (`Program.cs`) :

```csharp
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### Avantage

Si une commande modifie plusieurs entités (ex: créer un tournoi + inscrire une équipe), tous les changements sont validés dans une seule transaction via un seul `SaveChangesAsync()`. Si une étape échoue, rien n'est écrit.

---

## 2️⃣ CQRS + MediatR

### Qu'est-ce que c'est ?

**CQRS** (Command Query Responsibility Segregation) sépare strictement les opérations d'**écriture** (Commands) et de **lecture** (Queries).

**MediatR** est la bibliothèque qui implémente le pattern *mediator* : le contrôleur envoie un message (Command ou Query) et un handler associé le traite, sans que le contrôleur connaisse l'implémentation.

### Avant (sans CQRS)

Le contrôleur injectait directement 5 repositories, construisait les entités EF, appelait les repositories, gérait les règles métier, puis retournait un DTO. Le contrôleur était "gras".

### Après (avec CQRS + MediatR)

Le contrôleur est **mince** : il reçoit le DTO d'entrée, crée un `Command` ou `Query`, l'envoie à `IMediator.Send(...)`, et retourne le résultat.

### Structure CQRS

Pour chaque entité, on a dans `Esport.Application/Commands/` et `Esport.Application/Queries/` :

| Fichier | Rôle |
|---------|------|
| `CreateJeuCommand.cs` | `record CreateJeuCommand(CreateJeuDto Dto) : IRequest<JeuDto>` + `CreateJeuCommandHandler : IRequestHandler<...>` |
| `GetJeusQuery.cs` | `record GetJeusQuery(PaginationParams Pagination) : IRequest<PagedResult<JeuDto>>` + handler |

### Exemple concret : création d'un tournoi

**Command** (écriture) :

```csharp
public record CreateTournoiCommand(CreateTournoiDto Dto) : IRequest<TournoiDto>;

public class CreateTournoiCommandHandler : IRequestHandler<CreateTournoiCommand, TournoiDto>
{
    private readonly IUnitOfWork _uow;
    public CreateTournoiCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TournoiDto> Handle(CreateTournoiCommand request, CancellationToken ct)
    {
        // 1. Validation métier
        if (request.Dto.DateFin < request.Dto.DateDebut)
            throw new BadRequestException("DateFin doit être postérieure à DateDebut.");

        // 2. Vérifier dépendances
        var jeu = await _uow.Jeux.GetByIdAsync(request.Dto.JeuId, ct)
            ?? throw new BadRequestException($"Jeu {request.Dto.JeuId} introuvable.");

        // 3. Créer l'entité
        var tournoi = new Esport.Domain.Entities.Tournoi { ... };

        // 4. Sauvegarder (une seule transaction via UoW)
        await _uow.Tournois.AddAsync(tournoi, ct);
        await _uow.SaveChangesAsync(ct);

        // 5. Retourner le DTO
        return new TournoiDto { ... };
    }
}
```

**Query** (lecture) :

```csharp
public record GetTournoiByIdQuery(int Id) : IRequest<TournoiDto>;

public class GetTournoiByIdQueryHandler : IRequestHandler<GetTournoiByIdQuery, TournoiDto>
{
    private readonly IUnitOfWork _uow;
    public GetTournoiByIdQueryHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<TournoiDto> Handle(GetTournoiByIdQuery request, CancellationToken ct)
    {
        var t = await _uow.Tournois.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException($"Tournoi {request.Id} introuvable.");
        return new TournoiDto { ... };
    }
}
```

**Contrôleur** (mince) :

```csharp
[HttpPost]
[Authorize(Roles = "Admin,Organisateur")]
public async Task<IActionResult> Create([FromBody] CreateTournoiDto dto, CancellationToken ct)
{
    var tournoi = await _mediator.Send(new CreateTournoiCommand(dto), ct);
    return StatusCode(StatusCodes.Status201Created, tournoi);
}
```

### Enregistrement MediatR (`Program.cs`) :

```csharp
builder.Services.AddMediatR(typeof(Esport.Application.Interfaces.IUnitOfWork).Assembly);
```

MediatR scanne automatiquement l'assembly `Esport.Application` pour découvrir tous les `IRequestHandler<,>` et les associer à leurs `IRequest<>`.

### Gestion des erreurs

Les exceptions de domaine (`NotFoundException`, `ConflictException`, `BadRequestException`) sont levées dans les handlers et interceptées par le **middleware global** (`ExceptionHandlingMiddleware.cs`) qui les mappe vers les codes HTTP appropriés (404, 409, 400).

---

## 3️⃣ Clean Architecture

### Qu'est-ce que c'est ?

La **Clean Architecture** organise le projet en couches concentriques où les dépendances pointent **toujours vers l'intérieur** (vers le domaine métier). La couche la plus externe (l'API) dépend des couches internes, mais jamais l'inverse.

### Règle des dépendances

```
┌─────────────────────────────────────┐
│   TournoisE-Sports (API / UI)       │  ← Couche Présentation
│   (contrôleurs, Program.cs)         │     Dépend de : Application, Infrastructure
├─────────────────────────────────────┤
│   Esport.Application                │  ← Couche Application (CQRS)
│   (commands, queries, DTOs, UoW)    │     Dépend de : Domain
├─────────────────────────────────────┤
│   Esport.Domain                     │  ← Couche Domaine (cœur métier)
│   (entités, enums, exceptions)      │     Dépend de : RIEN (zéro dépendance externe)
└─────────────────────────────────────┘
│   Esport.Infrastructure             │  ← Couche Infrastructure
│   (EF Core, DbContext, migrations)  │     Dépend de : Domain, Application
└─────────────────────────────────────┘
```

### Fichiers .csproj (références de projet)

| Projet | Références |
|--------|-----------|
| `Esport.Domain` | aucune (projet autonome) |
| `Esport.Application` | `Esport.Domain`, `MediatR` |
| `Esport.Infrastructure` | `Esport.Domain`, `Esport.Application`, `EF Core SqlServer`, `Identity` |
| `TournoisE-Sports (API)` | `Esport.Application`, `Esport.Infrastructure`, `MediatR` |

**Le domaine ne connaît ni EF Core, ni MediatR, ni ASP.NET Core.** Il contient uniquement des classes C# pures (`BaseEntity`, `Jeu`, `TournoiStatut`, exceptions). Cela rend les règles métier testables et indépendantes de la technologie.

### Ce qui a été supprimé du projet API

Pour respecter la Clean Architecture, les dossiers suivants ont été **retirés** du projet API et déplacés dans les bonnes couches :

| Dossier supprimé de l'API | Nouvelle localisation |
|--------------------------|----------------------|
| `Models/` (entités EF) | `Esport.Domain/Entities/` |
| `Repositories/` | `Esport.Infrastructure/Repositories/` |
| `Data/AppDbContext.cs` | `Esport.Infrastructure/Data/AppDbContext.cs` |
| `Data/Seed/DataSeeder.cs` | `Esport.Infrastructure/Data/Seed/DataSeeder.cs` |
| `Services/JwtService.cs` | `TournoisE-Sports/Services/JwtService.cs` (reste dans l'API car dépend d'Identity/ASP.NET) |

### Ce qui est conservé dans l'API

- `Controllers/` : points d'entrée HTTP, appellent `IMediator`
- `Program.cs` : configuration DI, Identity, JWT, CORS, Swagger, middleware
- `Middleware/ExceptionHandlingMiddleware.cs` : mapping exceptions → HTTP
- `Services/JwtService.cs` : génération token JWT (dépend d'ASP.NET Core Identity)

---

## 🔧 Comment exécuter le projet

### 1. Restaurer les paquets

```bash
dotnet restore TournoisE-Sports.sln
```

### 2. Créer/mettre à jour la base de données

```bash
dotnet ef database update --project Esport.Infrastructure/Esport.Infrastructure.csproj --startup-project TournoisE-Sports/TournoisE-Sports.csproj
```

### 3. Lancer l'API

```bash
dotnet run --project TournoisE-Sports/TournoisE-Sports.csproj --launch-profile https
```

- API : `https://localhost:7000`
- Swagger : `https://localhost:7000/swagger`

---

## 📋 Résumé des 3 critères implémentés

| Critère | Comment il est implémenté | Fichiers clés |
|---------|-------------------------|---------------|
| **Unit of Work** | `IUnitOfWork` coordonne 6 repositories, un seul `SaveChangesAsync()` par opération | `IUnitOfWork.cs`, `UnitOfWork.cs`, repositories sans `SaveChanges` |
| **CQRS + MediatR** | Commands (écriture) et Queries (lecture) séparés, handlers `IRequestHandler`, contrôleurs via `IMediator` | `Commands/*`, `Queries/*`, `AddMediatR()` dans `Program.cs` |
| **Clean Architecture** | 4 projets : Domain (zéro dépendance), Application (CQRS), Infrastructure (EF), API (présentation), dépendances vers l'intérieur | `Esport.Domain.csproj`, `Esport.Application.csproj`, `Esport.Infrastructure.csproj`, `Program.cs` |

---

## ✅ Ce qui est conservé de l'existant

- Identity + JWT Bearer (authentification, rôles Admin/Organisateur/Joueur)
- CORS pour Angular (`http://localhost:4200`)
- Swagger avec bouton Authorize
- Pagination sur les listes
- Toutes les règles métier (match équipe contre elle-même interdit, équipes inscrites, scores, vainqueur)
- Seed des données d'exemple au démarrage
