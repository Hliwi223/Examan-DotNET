# ⚙️ Backend — API REST Tournois d'e-sport

> ### ⚠️ Note de vérification — 5 novembre 2026
>
> Vérifié ligne par ligne contre le code réel (`TournoisE-Sports/TournoisE-Sports`, build `dotnet build` : **0 erreur, 0 avertissement**).
> **Verdict : backend conforme au cahier des charges (T1–T8 + backlog 1–8).** Les corrections ci-dessous ont été appliquées au document :
>
> 1. **Stack .NET 9** (et non .NET 8) — `net9.0`, paquets Microsoft `9.0.0`, Swashbuckle `7.3.0`.
> 2. **Chemins réels** : `TournoisE-Sports/TournoisE-Sports` (et non `backend/Esport.Api`) ; migrations dans `Migrations/`, namespaces `TournoisE_Sports`.
> 3. **Pas d'`ApplicationUser`** : le contexte est `IdentityDbContext` (non générique), l'énumération s'appelle `TournoiStatut`.
> 4. **Unicités** : seul l'index unique `(TournoiId, EquipeId)` sur `Inscription` existe en base. `Nom` (Jeu/Equipe), `Tag`, `Pseudo` ne sont **pas** uniques en base (`Tag` : `MaxLength = 10`, pas 2–5).
> 5. **`Statut`** : stocké en `int` en base et sérialisé en **nombre** en JSON (aucun `JsonStringEnumConverter` dans `Program.cs`).
> 6. **Contrainte check `CK_Match_EquipesDifferentes` : absente** — la règle `EquipeAId != EquipeBId` est appliquée uniquement dans `MatchsController`.
> 7. **Suppressions** : `Match → Tournoi` est `Restrict` (le contrôleur renvoie `409` si le tournoi contient des matchs) ; seul `Inscription → Tournoi` est `Cascade`.
> 8. **Pagination : `/api/roles` et `/api/tournois/{id}/classement` NE sont PAS paginés** (listes simples). Le filtre `statut` sur `GET /api/tournois` n'existe pas (seul `jeuId`).
> 9. **Contrats JSON** : connexion renvoie `expires` (pas `expiration`) ; matchs exposent `nomEquipeA`/`nomEquipeB`/`nomVainqueur` ; classement renvoie un **tableau** `[{rang, equipeId, nomEquipe, pointsMarques, …}]` sans enveloppe `items` (3 points par victoire).
> 10. **Codes HTTP** : `201` **sans** en-tête `Location` ; `PUT` renvoie `200` (seul `DELETE` renvoie `204`) ; les erreurs métier sont `{ message: "…" }`, pas `ProblemDetails`.
> 11. **Sécurité / outillage** : `Jwt:Key` et `SeedAdmin:Password` sont encore en clair (placeholders) dans `appsettings.json` ; la collection Postman `docs/postman_collection.json` est **absente** ; le front Angular est absent du dépôt.
>
> Voir aussi les sections §8 (modèle), §12 (pagination) et la checklist §17 mises à jour en conséquence.

---

API REST **ASP.NET Core** du projet *Tournois d'e-sport* (Examen .NET · Sujet 2). Elle gère les jeux, les équipes, les joueurs, les tournois, les inscriptions et les matchs, avec une authentification **Identity + JWT** et des autorisations par **rôles**.

🏠 [README principal](./README.md) · 🖥 Front-end Angular : **absent du dépôt** (le CORS autorise `http://localhost:4200` en prévision)

---

## 📑 Sommaire

1. [Stack technique](#-stack-technique)
2. [Prérequis](#-prérequis)
3. [Structure du dossier](#-structure-du-dossier)
4. [Configuration](#-configuration)
5. [Base de données, migrations et seed](#-base-de-données-migrations-et-seed)
6. [Lancement](#-lancement)
7. [Swagger et authentification](#-swagger-et-authentification)
8. [Modèle de données](#-modèle-de-données)
9. [Règles métier](#-règles-métier)
10. [Rôles et autorisations](#-rôles-et-autorisations)
11. [Endpoints](#-endpoints)
12. [Pagination](#-pagination)
13. [Codes HTTP et erreurs](#-codes-http-et-erreurs)
14. [Choix d'architecture](#-choix-darchitecture)
15. [Bonus](#-bonus)
16. [Dépannage](#-dépannage)
17. [Checklist backend](#-checklist-backend)

---

## 🧰 Stack technique

| Domaine | Technologie |
|---|---|
| Framework | ASP.NET Core Web API (.NET 9, `net9.0`) |
| ORM | Entity Framework Core — **Code First** et migrations |
| Base de données | SQL Server (LocalDB, Express ou Docker) |
| Accès aux données | **Repository Pattern** : une interface et une implémentation par entité, injectées par DI |
| Sécurité | ASP.NET Core **Identity** + **JWT Bearer** + rôles |
| Asynchronisme | **async / await** de bout en bout |
| Documentation | Swagger / OpenAPI avec bouton *Authorize* |
| Validation | DataAnnotations |

**Packages NuGet** (versions réelles du `.csproj`)

| Package | Version | Rôle |
|---|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | `9.0.0` | Fournisseur SQL Server |
| `Microsoft.EntityFrameworkCore.Design` | `9.0.0` | Outils de migrations |
| `Microsoft.EntityFrameworkCore.Tools` | `9.0.0` | Outils EF |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | `9.0.0` | Identity avec EF Core |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | `9.0.0` | Validation des tokens JWT |
| `System.IdentityModel.Tokens.Jwt` | `8.3.0` | Génération des tokens JWT |
| `Swashbuckle.AspNetCore` | `7.3.0` | Swagger |

---

## ✅ Prérequis

- [.NET SDK 9.0](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express ou Docker)
- Outil EF Core : `dotnet tool install --global dotnet-ef`
- Certificat HTTPS de développement : `dotnet dev-certs https --trust`
- Git

---

## 🗂 Structure du dossier

```
TournoisE-Sports/                # racine du dépôt
├── TournoisE-Sports.sln
└── TournoisE-Sports/            # projet API (.NET 9)
    ├── Controllers/             # Auth, Roles, Jeux, Equipes, Joueurs, Tournois, Inscriptions, Matchs
    ├── DTOs/                    # Objets de transfert (requêtes et réponses)
    ├── Models/                  # Entités EF Core + enum TournoiStatut
    ├── Data/
    │   ├── AppDbContext.cs      # IdentityDbContext (non générique)
    │   └── Seed/                # Rôles, administrateur, données d'exemple
    ├── Migrations/              # InitialCreate, AddPrecisionDotation
    ├── Repositories/
    │   ├── Interfaces/          # IJeuRepository, IEquipeRepository, …
    │   └── Implementations/     # JeuRepository, EquipeRepository, …
    ├── Services/                # JwtService, RankingService
    ├── Common/                  # PagedResult<T>, PaginationParams
    ├── Properties/launchSettings.json
    ├── appsettings.json
    └── Program.cs               # DI, Identity, JWT, CORS, Swagger
```

Namespaces : `TournoisE_Sports.*` (tiret du dossier remplacé par `_`).

---

## 🔧 Configuration

> ⚠️ **Placeholders à remplacer avant le rendu.** `appsettings.json` contient encore des valeurs en clair (`Jwt:Key`, `SeedAdmin:Password`) ; pour le rendu, les définir via les *User Secrets* ou des variables d'environnement et ne committer que des valeurs factices.

| Clé | Rôle |
|---|---|
| `ConnectionStrings:DefaultConnection` | Chaîne de connexion SQL Server |
| `Jwt:Issuer` · `Jwt:Audience` | Émetteur et audience du token |
| `Jwt:Key` | Clé de signature HMAC (32 caractères minimum) — **secret** |
| `Jwt:ExpireMinutes` | Durée de vie du token |
| `SeedAdmin:Email` · `SeedAdmin:Password` | Compte administrateur créé par le seed — **secret** |

`appsettings.json` (valeurs d'exemple, celles effectivement livrées) :

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EsportTournamentsDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "EsportApi",
    "Audience": "EsportClient",
    "Key": "CHANGEZ-MOI-clé-d'au-moins-32-caractères",
    "ExpireMinutes": 60
  },
  "SeedAdmin": {
    "Email": "admin@esport.local",
    "Password": "ChangeMe!123"
  }
}
```

Définir les vraies valeurs en local :

```bash
cd TournoisE-Sports/TournoisE-Sports
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "votre-clé-secrète-de-32-caractères-minimum"
dotnet user-secrets set "SeedAdmin:Password" "VotreMotDePasseAdmin!1"
```

`JwtService` lève une `InvalidOperationException` au démarrage si `Jwt:Key` fait moins de 32 caractères.

Ou par variables d'environnement (le séparateur `:` devient `__`) : `Jwt__Key`, `SeedAdmin__Password`, `ConnectionStrings__DefaultConnection`.

### CORS

Le front-end Angular tourne sur `http://localhost:4200` : l'API l'autorise, et `UseCors` est appelé **avant** `UseAuthentication` dans `Program.cs`.

```csharp
builder.Services.AddCors(options =>
    options.AddPolicy("AngularClient", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

app.UseCors("AngularClient");
app.UseAuthentication();
app.UseAuthorization();
```

---

## 🗄 Base de données, migrations et seed

Depuis le dossier du projet :

```bash
cd TournoisE-Sports/TournoisE-Sports

# Créer une migration après une modification du modèle ( dossier Migrations/ par défaut )
dotnet ef migrations add NomMigration

# Créer ou mettre à jour la base
dotnet ef database update

# Repartir de zéro (supprime la base)
dotnet ef database drop
```

Migrations livrées : `20261105210042_InitialCreate`, `20261105210910_AddPrecisionDotation` (`Dotation` → `decimal(18,2)`).

### Seed

`dotnet ef database update` crée le schéma. **La base doit exister avant le premier lancement** (le seeder n'appelle ni `EnsureCreated` ni `Migrate`). Le seed s'exécute ensuite **au démarrage de l'API** (`Program.cs` → `DataSeeder.SeedAsync`) et ne duplique jamais les données : les rôles et l'administrateur sont vérifiés à chaque démarrage ; les données d'exemple ne sont insérées que si `db.Jeux` est vide.

| Donnée | Contenu |
|---|---|
| Rôles | `Admin`, `Organisateur`, `Joueur` |
| Administrateur | Compte défini par `SeedAdmin` (rôle `Admin`) |
| Jeux | 4 : *League of Legends*, *Valorant*, *EA FC 26*, *Rocket League* |
| Équipes et joueurs | 6 équipes, 6 joueurs (Faker, Shroud, s1mple, ZywOo, Caps, Rekkles) |
| Tournoi | 1 tournoi (*Spring Cup 2026*, jeu *League of Legends*), 4 inscriptions, 2 matchs **terminés** (aucun à venir) |

**Tester les autres rôles** : créer un compte avec `POST /api/auth/register`, puis lui attribuer un rôle avec `POST /api/roles/assign` en étant connecté en `Admin`.

---

## 🚀 Lancement

```bash
cd TournoisE-Sports/TournoisE-Sports
dotnet restore
dotnet ef database update
dotnet run --launch-profile https
```

- API : `https://localhost:7000` (port défini dans `Properties/launchSettings.json`)
- Swagger : `https://localhost:7000/swagger` (uniquement en environnement `Development`, voir `Program.cs`)

---

## 📘 Swagger et authentification

1. Ouvrir `https://localhost:7000/swagger`.
2. Appeler `POST /api/auth/login` avec le compte administrateur (`SeedAdmin`) et copier la valeur `token` de la réponse.
3. Cliquer sur **Authorize** et coller le token (le préfixe `Bearer` est ajouté automatiquement).
4. Les endpoints protégés par `[Authorize]` sont alors utilisables.

Configuration du bouton *Authorize* (Swashbuckle.AspNetCore 7.3.0, telle qu'implémentée) :

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Collez votre token : Bearer <token>"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[0]
        }
    });
});
```

Une collection Postman est annoncée dans le README principal (`docs/postman_collection.json`) mais **n'est pas présente** dans le dépôt : Swagger doit suffire pour le rendu, ou la collection doit être ajoutée.

---

## 🗃 Modèle de données

```
Jeu 1 ───< Tournoi 1 ───< Inscription >─── 1 Equipe 1 ───< Joueur
                │                              │
                └────────< Match >─────────────┘
                     (EquipeAId, EquipeBId)
```

| Entité | Propriétés | Clés étrangères |
|---|---|---|
| **Jeu** | `Id`, `Nom` (requis), `Editeur`, `Genre` | — |
| **Equipe** | `Id`, `Nom` (requis), `Tag` (`MaxLength = 10`), `DateCreation` | — |
| **Joueur** | `Id`, `Pseudo` (requis), `Nom`, `Prenom`, `Email` | `EquipeId` → Equipe |
| **Tournoi** | `Id`, `Nom`, `DateDebut`, `DateFin`, `Dotation` (`decimal(18,2)`), `Statut` | `JeuId` → Jeu |
| **Inscription** | `Id`, `DateInscription` | `TournoiId` → Tournoi, `EquipeId` → Equipe |
| **Match** | `Id`, `DateMatch`, `ScoreEquipeA`, `ScoreEquipeB` (nullable), `Termine` | `TournoiId` → Tournoi, `EquipeAId` → Equipe, `EquipeBId` → Equipe, `VainqueurId` → Equipe (nullable) |

`Statut` est l'énumération `TournoiStatut` : `AVenir` (0), `EnCours` (1), `Termine` (2) — **stockée en `int`** en base et **sérialisée en nombre** en JSON (aucun convertisseur texte n'est configuré).

### Configuration Fluent API (`AppDbContext.OnModelCreating`)

```csharp
// Seule contrainte d'unicité en base : une équipe ne s'inscrit qu'une fois par tournoi
modelBuilder.Entity<Inscription>()
    .HasIndex(i => new { i.TournoiId, i.EquipeId })
    .IsUnique();

// Plusieurs FK vers Equipe : Restrict pour éviter les chemins de cascade multiples
modelBuilder.Entity<Match>()
    .HasOne(m => m.EquipeA).WithMany().HasForeignKey(m => m.EquipeAId).OnDelete(DeleteBehavior.Restrict);
modelBuilder.Entity<Match>()
    .HasOne(m => m.EquipeB).WithMany().HasForeignKey(m => m.EquipeBId).OnDelete(DeleteBehavior.Restrict);
modelBuilder.Entity<Match>()
    .HasOne(m => m.Vainqueur).WithMany().HasForeignKey(m => m.VainqueurId).OnDelete(DeleteBehavior.Restrict);

// Précision décimale explicite pour Dotation
modelBuilder.Entity<Tournoi>().Property(t => t.Dotation).HasPrecision(18, 2);
```

> ℹ️ Il n'y a **aucune** contrainte check `EquipeAId <> EquipeBId` en base : la règle est appliquée dans `MatchsController.Create`. De même, l'unicité de `Jeu.Nom` n'est contrôlée qu'en code (`JeuRepository.ExistsAsync` → `409`).

### Comportement à la suppression

| Relation | Comportement réel |
|---|---|
| Jeu → Tournoi | `Restrict` : le contrôleur renvoie `409` si le jeu est référencé |
| Equipe → Joueur / Inscription | `Restrict` : `409` tant que des liens existent |
| Equipe → Match (A / B / Vainqueur) | `Restrict` : `409` |
| Tournoi → Match | `Restrict` : `TournoisController.Delete` renvoie `409` si le tournoi contient des matchs |
| Tournoi → Inscription | `Cascade` : supprimer un tournoi sans matchs supprime ses inscriptions |

---

## 📏 Règles métier

| # | Règle | Si elle est violée |
|---|---|---|
| 1 | Un tournoi est créé pour un jeu existant | `400 Bad Request` |
| 2 | `DateFin >= DateDebut` et `Dotation >= 0` (contrôlées dans `TournoisController`) | `400 Bad Request` |
| 3 | Une équipe ne s'inscrit qu'une seule fois à un même tournoi | `409 Conflict` |
| 4 | Un match oppose deux équipes inscrites au tournoi | `400 Bad Request` |
| 5 | Un match d'une équipe contre elle-même est interdit (`EquipeAId != EquipeBId`) | `400 Bad Request` |
| 6 | Les scores sont entre 0 et 10000 et l'égalité est refusée : un match terminé a un vainqueur | `400 Bad Request` |

À la saisie du score (`PUT /api/matchs/{id}/score`), le match est marqué `Termine = true` et `VainqueurId` est l'équipe au score le plus élevé.

**Classement** (`RankingService`) : ne tient compte que des matchs terminés des équipes inscrites. Tri : **points** (3 par victoire) décroissants, puis **différence de points** (`PointsMarques − PointsEncaisses`), puis `PointsMarques`, puis nom. Les champs exposés sont `rang`, `equipeId`, `nomEquipe`, `tagEquipe`, `victoires`, `defaites`, `matchsJoues`, `pointsMarques`, `pointsEncaisses`, `differencePoints`, `points`.

---

## 🔐 Rôles et autorisations

À l'inscription, un utilisateur reçoit **uniquement** le rôle `Joueur`. Seul un `Admin` peut attribuer `Organisateur` ou `Admin` : le rôle n'est jamais accepté dans le corps de `register`.

| Action | Anonyme | Joueur | Organisateur | Admin |
|---|:---:|:---:|:---:|:---:|
| S'inscrire, se connecter | ✅ | ✅ | ✅ | ✅ |
| Consulter (GET) | ❌ | ✅ | ✅ | ✅ |
| Créer et modifier jeux, équipes, joueurs, tournois | ❌ | ❌ | ✅ | ✅ |
| Inscrire une équipe, planifier un match, saisir un score | ❌ | ❌ | ✅ | ✅ |
| Supprimer | ❌ | ❌ | ❌ | ✅ |
| Gérer les rôles | ❌ | ❌ | ❌ | ✅ |

Les contrôleurs portent `[Authorize]`, les écritures `[Authorize(Roles = "Admin,Organisateur")]`, et les suppressions ainsi que `RolesController` `[Authorize(Roles = "Admin")]`. L'attribution du rôle `Joueur` à l'inscription est silencieuse : si le rôle n'existe pas encore (seed non exécuté), `AddToRoleAsync` échoue sans erreur retournée.

---

## 📡 Endpoints

> Les routes marquées 📄 renvoient une **liste paginée** (voir [Pagination](#-pagination)). `/api/roles` et `/api/tournois/{id}/classement` renvoient des listes simples, **non paginées**.

### 🔑 Authentification — `/api/auth`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Créer un compte (rôle `Joueur`) |
| POST | `/api/auth/login` | Public | Se connecter et obtenir le token JWT |

### 👥 Rôles — `/api/roles`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/roles` | Admin | Lister les rôles |
| POST | `/api/roles` | Admin | Créer un rôle |
| POST | `/api/roles/assign` | Admin | Attribuer un rôle à un utilisateur |
| POST | `/api/roles/remove` | Admin | Retirer un rôle à un utilisateur |

### 🎲 Jeux — `/api/jeux`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/jeux` | Authentifié | Lister les jeux 📄 |
| GET | `/api/jeux/{id}` | Authentifié | Détail d'un jeu |
| POST | `/api/jeux` | Admin, Organisateur | Créer un jeu |
| PUT | `/api/jeux/{id}` | Admin, Organisateur | Modifier un jeu |
| DELETE | `/api/jeux/{id}` | Admin | Supprimer un jeu |

### 🛡 Équipes — `/api/equipes`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/equipes` | Authentifié | Lister les équipes 📄 |
| GET | `/api/equipes/{id}` | Authentifié | Détail d'une équipe |
| POST | `/api/equipes` | Admin, Organisateur | Créer une équipe |
| PUT | `/api/equipes/{id}` | Admin, Organisateur | Modifier une équipe |
| DELETE | `/api/equipes/{id}` | Admin | Supprimer une équipe |
| GET | `/api/equipes/{id}/joueurs` | Authentifié | Joueurs d'une équipe 📄 |
| POST | `/api/equipes/{id}/joueurs` | Admin, Organisateur | Ajouter un joueur à l'équipe |

### 🧑‍🎮 Joueurs — `/api/joueurs`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/joueurs` | Authentifié | Lister les joueurs (filtre `equipeId`) 📄 |
| GET | `/api/joueurs/{id}` | Authentifié | Détail d'un joueur |
| PUT | `/api/joueurs/{id}` | Admin, Organisateur | Modifier un joueur |
| DELETE | `/api/joueurs/{id}` | Admin | Supprimer un joueur |

### 🏆 Tournois — `/api/tournois`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/tournois` | Authentifié | Lister les tournois (filtre `jeuId`) 📄 |
| GET | `/api/tournois/{id}` | Authentifié | Détail d'un tournoi |
| POST | `/api/tournois` | Admin, Organisateur | Créer un tournoi pour un jeu |
| PUT | `/api/tournois/{id}` | Admin, Organisateur | Modifier un tournoi |
| DELETE | `/api/tournois/{id}` | Admin | Supprimer un tournoi |
| GET | `/api/tournois/{id}/classement` | Authentifié | Classement des équipes (liste complète, non paginée) |

### 📝 Inscriptions

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/tournois/{id}/inscriptions` | Authentifié | Équipes inscrites à un tournoi 📄 |
| POST | `/api/tournois/{id}/inscriptions` | Admin, Organisateur | Inscrire une équipe (`409` si déjà inscrite) |
| DELETE | `/api/inscriptions/{id}` | Admin, Organisateur | Désinscrire une équipe |

### ⚔️ Matchs

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/tournois/{id}/matchs` | Authentifié | Matchs d'un tournoi 📄 |
| GET | `/api/matchs/{id}` | Authentifié | Détail d'un match |
| POST | `/api/matchs` | Admin, Organisateur | Planifier un match (`400` si A = B) |
| PUT | `/api/matchs/{id}/score` | Admin, Organisateur | Saisir le score et désigner le vainqueur |
| DELETE | `/api/matchs/{id}` | Admin | Supprimer un match |

### Exemples de requêtes

**Inscription** — `POST /api/auth/register` → `201 Created`

```json
{ "email": "joueur@esport.local", "password": "Str0ng!Pass" }
```

La politique de mot de passe est celle d'Identity par défaut : 6 caractères minimum, avec majuscule, minuscule, chiffre et caractère spécial.

**Connexion** — `POST /api/auth/login` → `200 OK`

```json
{ "email": "admin@esport.local", "password": "VotreMotDePasseAdmin!1" }
```

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@esport.local",
  "roles": ["Admin"],
  "expires": "2026-11-05T15:30:00Z"
}
```

`expires` est une date UTC au format ISO 8601. Le token contient aussi les rôles, mais la réponse les renvoie pour que le front-end n'ait pas à le décoder.

**Création d'un tournoi** — `POST /api/tournois` → `201 Created`

```json
{
  "nom": "Winter Cup 2026",
  "dateDebut": "2026-12-10T10:00:00Z",
  "dateFin": "2026-12-12T20:00:00Z",
  "dotation": 5000,
  "statut": 0,
  "jeuId": 1
}
```

`statut` est l'entier `TournoiStatut` (`0 = AVenir`, `1 = EnCours`, `2 = Termine`) ; il vaut `0` par défaut.

**Inscription d'une équipe** — `POST /api/tournois/1/inscriptions` → `201 Created`

```json
{ "equipeId": 3 }
```

**Planification d'un match** — `POST /api/matchs` → `201 Created`

```json
{
  "tournoiId": 1,
  "equipeAId": 3,
  "equipeBId": 4,
  "dateMatch": "2026-12-10T14:00:00Z"
}
```

**Saisie du score** — `PUT /api/matchs/7/score` → `200 OK`

```json
{ "scoreEquipeA": 2, "scoreEquipeB": 1 }
```

```json
{
  "id": 7,
  "dateMatch": "2026-12-10T14:00:00Z",
  "scoreEquipeA": 2,
  "scoreEquipeB": 1,
  "termine": true,
  "tournoiId": 1,
  "nomTournoi": "Winter Cup 2026",
  "equipeAId": 3,
  "nomEquipeA": "Team Alpha",
  "equipeBId": 4,
  "nomEquipeB": "Team Beta",
  "vainqueurId": 3,
  "nomVainqueur": "Team Alpha"
}
```

**Classement** — `GET /api/tournois/1/classement` → `200 OK` (tableau, sans pagination)

```json
[
  {
    "rang": 1,
    "equipeId": 3,
    "nomEquipe": "Team Alpha",
    "tagEquipe": "ALP",
    "victoires": 3,
    "defaites": 1,
    "matchsJoues": 4,
    "pointsMarques": 9,
    "pointsEncaisses": 5,
    "differencePoints": 4,
    "points": 9
  }
]
```

Le `rang` est calculé sur le classement complet.

---

## 📄 Pagination

Les listes paginées renvoient `items` + métadonnées. **Exceptions** : `/api/roles` et `/api/tournois/{id}/classement` renvoient des listes simples sans pagination.

| Paramètre | Défaut | Règle |
|---|---|---|
| `page` | `1` | Minimum 1 |
| `pageSize` | `10` | Borné entre 1 et 50 (via `Skip` / `Take`) |

Format de réponse commun :

```json
{
  "items": [],
  "page": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0
}
```

```csharp
public class PaginationParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int Skip => (Math.Max(Page, 1) - 1) * Math.Min(PageSize, 50);
    public int Take => Math.Min(Math.Max(PageSize, 1), 50);
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

Dans les repositories, la requête est toujours **triée** avant `Skip` / `Take` pour que les pages soient stables (ex. `OrderBy(j => j.Nom)`, `OrderByDescending(t => t.DateDebut)`).

---

## 🚦 Codes HTTP et erreurs

| Code | Utilisation réelle |
|---|---|
| `200 OK` | Lecture, modification (`PUT`) réussie avec contenu |
| `201 Created` | Création réussie (corps JSON, **sans** en-tête `Location`) |
| `204 No Content` | Suppression réussie (`DELETE`) |
| `400 Bad Request` | Validation DataAnnotations ou règle métier violée |
| `401 Unauthorized` | Token absent, invalide ou expiré ; identifiants incorrects à la connexion |
| `403 Forbidden` | Utilisateur connecté mais rôle insuffisant |
| `404 Not Found` | Ressource introuvable |
| `409 Conflict` | Doublon (inscription déjà existante, jeu déjà existant) ou suppression bloquée par des dépendances |

Erreur de validation (générée par `[ApiController]` pour les DataAnnotations) :

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Nom": ["Le nom est obligatoire."]
  }
}
```

Erreur métier (objet anonyme `{ message }`, **pas** `ProblemDetails`) :

```json
{ "message": "Une équipe ne peut pas jouer contre elle-même." }
```

---

## 🏛 Choix d'architecture

- **Contrôleurs fins** : ils exposent uniquement des **DTO** en entrée et en sortie, appellent les repositories ou services injectés et renvoient les bons codes HTTP via `IActionResult` (pas `ActionResult<T>`). En interne, ils construisent les entités EF avant les appels aux repositories.
- **Repository Pattern** : une interface et une implémentation par entité (`Jeu`, `Equipe`, `Joueur`, `Tournoi`, `Inscription`, `Match`), enregistrées en `Scoped` dans `Program.cs`.
- **async / await de bout en bout** : contrôleurs, repositories et accès EF Core retournent des `Task`. Aucun appel bloquant (`.Result`, `.Wait()`, `.GetAwaiter().GetResult()`).
- **Validation** : DataAnnotations sur les DTO (`[Required]`, `[MinLength]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`) ; aucune `IValidatableObject` — la cohérence des dates du tournoi est vérifiée à la main dans `TournoisController` (`DateFin >= DateDebut`).

Exemple d'interface de repository (signature réelle) :

```csharp
public interface ITournoiRepository
{
    Task<PagedResult<Tournoi>> GetPagedAsync(PaginationParams pagination, int? jeuId = null, CancellationToken ct = default);
    Task<Tournoi?> GetByIdAsync(int id, CancellationToken ct = default);
    Task AddAsync(Tournoi tournoi, CancellationToken ct = default);
    Task UpdateAsync(Tournoi tournoi, CancellationToken ct = default);
    Task DeleteAsync(Tournoi tournoi, CancellationToken ct = default);
}
```

Enregistrement dans `Program.cs` :

```csharp
builder.Services.AddScoped<IJeuRepository, JeuRepository>();
builder.Services.AddScoped<IEquipeRepository, EquipeRepository>();
builder.Services.AddScoped<IJoueurRepository, JoueurRepository>();
builder.Services.AddScoped<ITournoiRepository, TournoiRepository>();
builder.Services.AddScoped<IInscriptionRepository, InscriptionRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
```

**JWT** : le token est signé en HMAC-SHA256 et contient `sub`, `email`, `jti`, `NameIdentifier` et un claim `ClaimTypes.Role` par rôle. Le type `ClaimTypes.Role` est indispensable pour que `[Authorize(Roles = "...")]` fonctionne.

```csharp
var claims = new List<Claim>
{
    new Claim("sub", user.Id),
    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    new Claim(ClaimTypes.NameIdentifier, user.Id),
};
foreach (var role in roles)
    claims.Add(new Claim(ClaimTypes.Role, role));
```

Avec `AddIdentity`, le schéma par défaut est celui des cookies : `DefaultAuthenticateScheme` et `DefaultChallengeScheme` sont définis sur `JwtBearerDefaults.AuthenticationScheme` pour que l'API réponde `401` (et non une redirection) aux requêtes non authentifiées.

---

## ⭐ Bonus

| Bonus | Points | Mise en œuvre | Statut |
|---|---|---|---|
| Unit of Work | +1 | `IUnitOfWork` expose les repositories et `SaveChangesAsync()` : une seule transaction | ☐ |
| CQRS + MediatR | +1,5 | Dossiers `Commands/` et `Queries/` avec handlers `IRequestHandler`, contrôleurs → `IMediator` | ☐ |
| Clean Architecture | +1,5 | Projets `Esport.Domain`, `Esport.Application`, `Esport.Infrastructure`, `Esport.Api`, dépendances tournées vers le domaine | ☐ |

Les bonus ne comptent que si le socle obligatoire est atteint.

---

## 🛠 Dépannage

| Problème | Solution |
|---|---|
| `Cannot open database` ou connexion refusée | Vérifier que SQL Server est démarré et la chaîne `DefaultConnection`, puis relancer `dotnet ef database update` |
| Base introuvable au premier démarrage | Le seed exige une base existante : lancer d'abord `dotnet ef database update` |
| Version des outils EF trop ancienne | `dotnet tool update --global dotnet-ef` |
| Certificat HTTPS non approuvé | `dotnet dev-certs https --trust` |
| `Jwt:Key` manquante ou trop courte | Définir la clé avec `dotnet user-secrets set "Jwt:Key" "..."` (32 caractères minimum) |
| `401` sur Swagger | Token absent ou expiré : se reconnecter avec `/api/auth/login`, puis *Authorize* |
| `403` | Le rôle de l'utilisateur ne permet pas l'action : vérifier ses rôles ou se connecter avec un compte `Admin` |
| Swagger introuvable | Swagger n'est activé qu'en `Development` : lancer avec `--launch-profile https` |
| Erreur CORS depuis Angular | Vérifier `WithOrigins("http://localhost:4200")` et l'ordre `UseCors` avant `UseAuthentication` |
| `may cause cycles or multiple cascade paths` à la migration | Mettre `DeleteBehavior.Restrict` sur les FK de `Match` vers `Equipe` |

---

## 📋 Checklist backend

- [x] Entités, clés étrangères et migrations (Code First)
- [x] Seed : rôles, administrateur, données d'exemple
- [x] 6 repositories (interface + implémentation) enregistrés par DI
- [x] DTO pour toutes les entrées et sorties
- [x] Pagination sur les listes principales (`/roles` et `/classement` non paginés)
- [x] Identity : inscription et connexion avec token JWT contenant les rôles
- [x] Gestion des rôles : créer, lister, attribuer, retirer
- [x] `[Authorize(Roles = "...")]` sur les endpoints sensibles
- [x] Règles métier : match équipe contre elle-même interdit, équipes inscrites, score et vainqueur
- [x] async / await partout, sans appel bloquant
- [x] Validation DataAnnotations avec erreurs claires
- [x] Swagger avec bouton *Authorize*
- [ ] Aucun secret réel dans le dépôt — placeholders encore en clair dans `appsettings.json` (à basculer en user-secrets avant rendu)
- [ ] Collection Postman ou Swagger fournie — Swagger présent, collection Postman absente
