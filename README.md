# 🎮 Tournois d'e-sport — API REST ASP.NET Core

Examen .NET · **Sujet 2 : Tournois d'e-sport**

API REST de gestion de tournois d'e-sport : jeux, équipes, joueurs, tournois, inscriptions des équipes, planification des matchs, saisie des scores et classement. L'API est sécurisée par **ASP.NET Core Identity + JWT** avec gestion des rôles, et accompagnée d'un front-end **Angular** qui consomme l'API réelle.

> **Auteurs** : `[Nom Prénom]` · `[Nom Prénom]`
> **Date de rendu** : `[à préciser]`

---

## 📑 Sommaire

1. [Stack technique](#-stack-technique)
2. [Modèle de données](#-modèle-de-données)
3. [Règles métier](#-règles-métier)
4. [Prérequis](#-prérequis)
5. [Configuration](#-configuration)
6. [Lancement](#-lancement)
7. [Authentification et rôles](#-authentification-et-rôles)
8. [Endpoints](#-endpoints)
9. [Pagination](#-pagination)
10. [Architecture du projet](#-architecture-du-projet)
11. [Front-end](#-front-end)
12. [Bonus réalisés](#-bonus-réalisés)
13. [Couverture du cahier des charges](#-couverture-du-cahier-des-charges)

---

## 🧰 Stack technique

| Domaine | Technologie |
|---|---|
| Framework | ASP.NET Core Web API (`.NET 8`) |
| ORM | Entity Framework Core — **Code First** |
| Base de données | SQL Server (LocalDB / Docker) |
| Accès aux données | **Repository Pattern** (interface + implémentation par entité, injection de dépendances) |
| Sécurité | ASP.NET Core Identity + **JWT Bearer** + rôles |
| Programmation | **async / await** de bout en bout |
| Documentation | Swagger / OpenAPI (bouton *Authorize*) |
| Validation | DataAnnotations |
| Front-end | **Angular** (TypeScript, composants standalone, Angular Router, `HttpClient` + intercepteur JWT) |

---

## 🗂 Modèle de données

```
Jeu 1 ───< Tournoi 1 ───< Inscription >─── 1 Equipe 1 ───< Joueur
                │                              │
                └────────< Match >─────────────┘
                     (EquipeAId, EquipeBId)
```

| Entité | Champs principaux | Clés étrangères |
|---|---|---|
| **Jeu** | `Id`, `Nom`, `Editeur`, `Genre` | — |
| **Equipe** | `Id`, `Nom`, `Tag`, `DateCreation` | — |
| **Joueur** | `Id`, `Pseudo`, `Nom`, `Prenom`, `Email` | `EquipeId` → Equipe |
| **Tournoi** | `Id`, `Nom`, `DateDebut`, `DateFin`, `Dotation`, `Statut` | `JeuId` → Jeu |
| **Inscription** | `Id`, `DateInscription` | `TournoiId` → Tournoi, `EquipeId` → Equipe |
| **Match** | `Id`, `DateMatch`, `ScoreEquipeA`, `ScoreEquipeB`, `Termine` | `TournoiId` → Tournoi, `EquipeAId` → Equipe, `EquipeBId` → Equipe, `VainqueurId` → Equipe (nullable) |

**Contraintes** :
- Index unique `(TournoiId, EquipeId)` sur `Inscription` (une équipe ne s'inscrit qu'une fois par tournoi).
- `Match` : deux relations vers `Equipe` (`EquipeA` / `EquipeB`) avec `DeleteBehavior.Restrict` pour éviter les cascades multiples.

---

## 📏 Règles métier

- Un joueur appartient à **une seule équipe**.
- Un tournoi est créé **pour un jeu donné**.
- Une équipe ne peut s'inscrire **qu'une seule fois** à un même tournoi.
- Un match n'est possible qu'entre deux équipes **inscrites au tournoi**.
- ❌ Un match **d'une équipe contre elle-même est interdit** (`EquipeAId != EquipeBId`) → `400 Bad Request`.
- Le **vainqueur** est désigné à la saisie du score (score le plus élevé ; égalité refusée ou gérée selon le choix du projet).
- Le **classement** d'un tournoi est calculé à partir des matchs terminés (victoires, points marqués/encaissés).

---

## ✅ Prérequis

- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express ou Docker)
- Outil EF Core :
  ```bash
  dotnet tool install --global dotnet-ef
  ```
- Git
- [Node.js](https://nodejs.org) LTS (version compatible avec votre Angular CLI) et npm
- Angular CLI :
  ```bash
  npm install -g @angular/cli
  ```

SQL Server via Docker (optionnel) :
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<VotreMotDePasse>" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

---

## ⚙️ Configuration

> ⚠️ **Aucun secret réel ne doit être versionné.** Utilisez les *User Secrets* ou des variables d'environnement.

Fichier `appsettings.json` (valeurs d'exemple) :

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

Configuration recommandée des secrets en local :

```bash
cd src/Esport.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "votre-clé-secrète-de-32-caractères-minimum"
dotnet user-secrets set "SeedAdmin:Password" "VotreMotDePasseAdmin!1"
```

---

## 🚀 Lancement

```bash
# 1. Cloner le dépôt
git clone <url-du-depot>
cd <dossier-du-projet>

# 2. Restaurer les dépendances
dotnet restore

# 3. Appliquer les migrations (création de la base + seed)
dotnet ef database update --project src/Esport.Api

# 4. Lancer l'API
dotnet run --project src/Esport.Api

# 5. Lancer le front-end Angular (dans un second terminal)
cd frontend
npm install
ng serve
```

- API : `https://localhost:7000` *(port à adapter)*
- Swagger : `https://localhost:7000/swagger`
- Front-end Angular : `http://localhost:4200`

### Migrations (rappel)

```bash
dotnet ef migrations add NomMigration --project src/Esport.Api
dotnet ef database update --project src/Esport.Api
```

### Données initiales (seed)

Au démarrage, le seed crée :
- **Rôles** : `Admin`, `Organisateur`, `Joueur`
- **Compte administrateur** : défini dans la configuration (`SeedAdmin`)
- **Données d'exemple** : jeux (ex. *League of Legends*, *Valorant*, *EA FC*), équipes, joueurs, un tournoi avec inscriptions et quelques matchs

---

## 🔐 Authentification et rôles

| Rôle | Droits |
|---|---|
| **Admin** | Tout : gestion des rôles, des utilisateurs, CRUD complet |
| **Organisateur** | CRUD jeux / tournois, inscriptions, planification des matchs, saisie des scores |
| **Joueur** | Consultation, gestion de son équipe / ses joueurs |
| *Anonyme* | Inscription et connexion uniquement |

**Utilisation du token dans Swagger** : `POST /api/auth/login` → copier le `token` → bouton **Authorize** → saisir `Bearer <token>`.

Le JWT contient les *claims* : `sub`, `email`, `jti` et les **rôles** de l'utilisateur.

---

## 📡 Endpoints

> Toutes les listes sont **paginées** (voir plus bas). Codes HTTP : `200`, `201`, `204`, `400`, `401`, `403`, `404`, `409`.

### 🔑 Authentification — `/api/auth`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Inscription (Identity) |
| POST | `/api/auth/login` | Public | Connexion → token JWT |

### 👥 Rôles — `/api/roles` *(Admin)*

| Méthode | Route | Description |
|---|---|---|
| POST | `/api/roles` | Créer un rôle |
| GET | `/api/roles` | Lister les rôles |
| POST | `/api/roles/assign` | Attribuer un rôle à un utilisateur |
| POST | `/api/roles/remove` | Retirer un rôle à un utilisateur |

### 🎲 Jeux — `/api/jeux`

| Méthode | Route | Accès |
|---|---|---|
| GET | `/api/jeux` | Authentifié |
| GET | `/api/jeux/{id}` | Authentifié |
| POST | `/api/jeux` | Admin, Organisateur |
| PUT | `/api/jeux/{id}` | Admin, Organisateur |
| DELETE | `/api/jeux/{id}` | Admin |

### 🛡 Équipes — `/api/equipes`

| Méthode | Route | Accès |
|---|---|---|
| GET | `/api/equipes` | Authentifié |
| GET | `/api/equipes/{id}` | Authentifié |
| POST | `/api/equipes` | Admin, Organisateur |
| PUT | `/api/equipes/{id}` | Admin, Organisateur |
| DELETE | `/api/equipes/{id}` | Admin |
| GET | `/api/equipes/{id}/joueurs` | Authentifié |
| POST | `/api/equipes/{id}/joueurs` | Admin, Organisateur | Ajouter un joueur à l'équipe |

### 🧑‍🎮 Joueurs — `/api/joueurs`

| Méthode | Route | Accès |
|---|---|---|
| GET | `/api/joueurs` | Authentifié |
| GET | `/api/joueurs/{id}` | Authentifié |
| PUT | `/api/joueurs/{id}` | Admin, Organisateur |
| DELETE | `/api/joueurs/{id}` | Admin |

### 🏆 Tournois — `/api/tournois`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/tournois` | Authentifié | Liste (filtre `jeuId` possible) |
| GET | `/api/tournois/{id}` | Authentifié | Détail |
| POST | `/api/tournois` | Admin, Organisateur | Créer un tournoi pour un jeu |
| PUT | `/api/tournois/{id}` | Admin, Organisateur | Modifier |
| DELETE | `/api/tournois/{id}` | Admin | Supprimer |
| GET | `/api/tournois/{id}/classement` | Authentifié | Classement des équipes |

### 📝 Inscriptions — `/api/inscriptions`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/tournois/{id}/inscriptions` | Authentifié | Équipes inscrites |
| POST | `/api/tournois/{id}/inscriptions` | Admin, Organisateur | Inscrire une équipe (`409` si déjà inscrite) |
| DELETE | `/api/inscriptions/{id}` | Admin, Organisateur | Désinscrire |

### ⚔️ Matchs — `/api/matchs`

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/tournois/{id}/matchs` | Authentifié | Matchs d'un tournoi |
| GET | `/api/matchs/{id}` | Authentifié | Détail |
| POST | `/api/matchs` | Admin, Organisateur | Planifier un match (`400` si A = B) |
| PUT | `/api/matchs/{id}/score` | Admin, Organisateur | Saisir le score, désigner le vainqueur |
| DELETE | `/api/matchs/{id}` | Admin | Supprimer |

> 📌 Adapter ce tableau aux routes réellement implémentées.

---

## 📄 Pagination

Toutes les listes acceptent les paramètres :

| Paramètre | Défaut | Description |
|---|---|---|
| `page` | `1` | Numéro de page |
| `pageSize` | `10` | Taille de page (max 50) |

Exemple : `GET /api/tournois?page=2&pageSize=5`

Réponse :

```json
{
  "items": [ { "id": 6, "nom": "Spring Cup" } ],
  "page": 2,
  "pageSize": 5,
  "totalCount": 23,
  "totalPages": 5
}
```

---

## 🏗 Architecture du projet

```
src/
└── Esport.Api/
    ├── Controllers/        # AuthController, RolesController, JeuxController, ...
    ├── DTOs/               # Objets de transfert (requêtes / réponses)
    ├── Models/             # Entités EF Core (Jeu, Equipe, Joueur, Tournoi, Inscription, Match)
    ├── Data/
    │   ├── AppDbContext.cs # IdentityDbContext
    │   ├── Migrations/
    │   └── Seed/           # Rôles, admin, données d'exemple
    ├── Repositories/
    │   ├── Interfaces/     # IJeuRepository, IEquipeRepository, ...
    │   └── Implementations/
    ├── Services/           # JwtService, logique métier (classement, score)
    ├── Common/             # PagedResult<T>, PaginationParams
    └── Program.cs          # DI, Identity, JWT, Swagger
frontend/                   # Application Angular (voir section Front-end)
```

**Principes** :
- Un contrôleur ne manipule que des **DTO** et appelle les **repositories** (ou services) injectés.
- Toutes les méthodes d'accès aux données sont `async Task<...>` avec `CancellationToken` quand c'est pertinent.
- Validation par **DataAnnotations** ; réponses d'erreur claires (`ProblemDetails` / `ValidationProblemDetails`).

---

## 🖥 Front-end

Application **Angular** (composants standalone) qui consomme l'API réelle avec l'authentification JWT. Elle est évaluée sur son fonctionnement, pas sur son design.

**Pages minimales** : connexion · liste · création · modification · suppression.

### Démarrage

```bash
cd frontend
npm install
ng serve        # http://localhost:4200
```

### URL de l'API

```bash
ng generate environments
```

`src/environments/environment.development.ts` :

```ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7000/api'   // à adapter au port de l'API
};
```

### CORS côté API

Dans `Program.cs`, l'origine du front doit être autorisée, et `UseCors` doit être appelé avant `UseAuthentication` :

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

### Pages et routes

| Route | Page | Accès |
|---|---|---|
| `/login` | Connexion | Public |
| `/register` | Inscription | Public |
| `/tournois` | Liste paginée des tournois (bouton de suppression avec confirmation) | Connecté |
| `/tournois/new` | Création d'un tournoi | Admin, Organisateur |
| `/tournois/:id/edit` | Modification d'un tournoi | Admin, Organisateur |
| `/tournois/:id` | Détail : inscriptions, matchs, classement | Connecté |
| `/equipes`, `/jeux` | Liste et CRUD des équipes et des jeux | Connecté (écriture : Admin, Organisateur) |

> 📌 Adapter ce tableau aux pages réellement implémentées.

### Design et thème e-sport

Le design du front doit rester **cohérent avec le sujet** : une plateforme de tournois d'e-sport. Le cahier des charges évalue le front sur son fonctionnement et non sur son design : le thème reste donc léger (quelques variables CSS et des composants simples), sans y consacrer plus de temps que nécessaire.

**Identité visuelle**

| Élément | Proposition |
|---|---|
| Mode | Thème sombre par défaut |
| Couleurs | Fond bleu nuit avec accents néon (violet, cyan) |
| Titres | Police « gaming » (ex. *Orbitron* ou *Rajdhani*) |
| Texte | Police lisible (ex. *Inter*) |
| Effets | Légère lueur (`box-shadow`) au survol des boutons et des cartes, sans animation lourde |

```scss
// src/styles.scss
:root {
  --bg: #0b0f1a;
  --surface: #151b2d;
  --text: #e8ecf5;
  --muted: #8b95b0;
  --accent: #8b5cf6;    // violet
  --accent-2: #22d3ee;  // cyan
  --danger: #f43f5e;
  --win: #22c55e;
}

body {
  background: var(--bg);
  color: var(--text);
}
```

**Rendu proposé par page**

| Page | Idée de rendu |
|---|---|
| Liste des tournois | Cartes avec nom du jeu, dates, dotation et badge de statut (*À venir* · *En cours* · *Terminé*) |
| Détail d'un tournoi | Onglets *Inscriptions* · *Matchs* · *Classement* |
| Matchs | Carte « Équipe A **VS** Équipe B » avec le score et le vainqueur mis en avant |
| Classement | Tableau (rang, équipe, victoires, points) avec podium 🥇🥈🥉 pour le top 3 |
| Équipes | Cartes avec le tag de l'équipe et la liste des pseudos des joueurs |
| Connexion | Formulaire centré sur fond sombre |

**Recommandations** : interface responsive, contraste suffisant du texte sur fond sombre, messages d'erreur de validation visibles et confirmation avant toute suppression. Bibliothèque UI au choix : Angular Material (thème sombre), Bootstrap ou Tailwind CSS.

### Authentification côté Angular

- **`AuthService`** : `login()`, `register()`, `logout()`, stockage du token (`localStorage`) et lecture des rôles dans le JWT.
- **Intercepteur HTTP** : ajoute `Authorization: Bearer <token>` à chaque requête et redirige vers `/login` en cas de `401`.
- **Guards** : `authGuard` (utilisateur connecté) et `roleGuard` (rôle requis) sur les routes.

```ts
// app.config.ts
provideHttpClient(withInterceptors([authInterceptor]))
```

### Structure

```
frontend/src/app/
├── core/
│   ├── services/       # auth.service.ts, tournoi.service.ts, equipe.service.ts, ...
│   ├── interceptors/   # auth.interceptor.ts
│   ├── guards/         # auth.guard.ts, role.guard.ts
│   └── models/         # interfaces TypeScript (Tournoi, Equipe, PagedResult<T>, ...)
├── features/
│   ├── auth/           # login, register
│   ├── tournois/       # liste, formulaire (création / modification), détail
│   ├── equipes/
│   ├── jeux/
│   └── matchs/
├── shared/             # composant de pagination, etc.
├── app.routes.ts
└── app.config.ts
```

---

## ⭐ Bonus réalisés

| Bonus | Points | Statut |
|---|---|---|
| Unit of Work | +1 | ☐ |
| CQRS + MediatR | +1,5 | ☐ |
| Clean Architecture | +1,5 | ☐ |

*(Cocher ce qui est réalisé ; les bonus ne comptent que si le socle obligatoire est atteint.)*

---

## 📋 Couverture du cahier des charges

### Fonctionnalités transversales

- [ ] **T1** Inscription (Identity)
- [ ] **T2** Connexion — JWT contenant les rôles
- [ ] **T3** Gestion des rôles — créer, lister, attribuer, retirer
- [ ] **T4** Autorisation par rôle — `[Authorize(Roles = "...")]`
- [ ] **T5** Pagination — `page`, `pageSize`, total sur toutes les listes
- [ ] **T6** Validation — DataAnnotations et erreurs claires
- [ ] **T7** Swagger — bouton *Authorize*
- [ ] **T8** Données initiales — migrations et seed

### Backlog produit — Sujet 2

- [ ] 1. CRUD des jeux et des équipes
- [ ] 2. Ajouter des joueurs à une équipe
- [ ] 3. Créer un tournoi pour un jeu donné
- [ ] 4. Inscrire une équipe à un tournoi
- [ ] 5. Planifier un match entre deux équipes
- [ ] 6. Interdire un match d'une équipe contre elle-même
- [ ] 7. Saisir le score et désigner le vainqueur
- [ ] 8. Classement des équipes d'un tournoi

---

## 📦 Livrables

- Solution .NET complète (API + front)
- Ce `README.md`
- Migrations EF Core et données de test
- Collection Swagger ou Postman : `docs/postman_collection.json`

---

## 📝 Licence

Projet réalisé dans un cadre pédagogique — Examen .NET.
