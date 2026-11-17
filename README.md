# 🎮 Tournois d'e-sport

Application de gestion de tournois d'e-sport composée d'une API REST ASP.NET Core et d'une application web Angular.

La plateforme permet de gérer les jeux, les équipes, les joueurs, les tournois, les inscriptions, les matchs, les scores et les classements. L'accès à l'API est sécurisé avec ASP.NET Core Identity, JWT et des rôles.

## Sommaire

1. [Fonctionnalités](#fonctionnalités)
2. [Stack technique](#stack-technique)
3. [Modèle de données](#modèle-de-données)
4. [Règles métier](#règles-métier)
5. [Structure du projet](#structure-du-projet)
6. [Configuration et lancement](#configuration-et-lancement)
7. [Authentification et rôles](#authentification-et-rôles)
8. [API](#api)
9. [Application Angular](#application-angular)
10. [Données initiales](#données-initiales)
11. [État du projet](#état-du-projet)

## Fonctionnalités

- Inscription et connexion des utilisateurs.
- Authentification par token JWT.
- Gestion des rôles Admin, Organisateur et Joueur.
- CRUD des jeux et des équipes.
- Gestion des joueurs associés à une équipe.
- Création et modification des tournois.
- Inscription des équipes à un tournoi.
- Planification des matchs.
- Saisie des scores et désignation du vainqueur.
- Calcul du classement d'un tournoi.
- Pagination des listes.
- Documentation de l'API avec Swagger en environnement de développement.
- Interface Angular protégée par des guards d'authentification et de rôle.

## Stack technique

| Domaine | Technologie |
|---|---|
| API | ASP.NET Core Web API |
| Runtime | .NET 9 |
| Langage backend | C# |
| ORM | Entity Framework Core 9, approche Code First |
| Base de données | SQL Server |
| Sécurité | ASP.NET Core Identity, JWT et rôles |
| Accès aux données | Repository Pattern |
| Documentation API | Swagger / OpenAPI |
| Frontend | Angular 17 |
| Langage frontend | TypeScript |
| Styles | SCSS |

## Modèle de données

Les principales entités sont les suivantes :

| Entité | Description | Relations principales |
|---|---|---|
| Jeu | Jeu vidéo utilisé par un tournoi | Un jeu possède plusieurs tournois |
| Équipe | Équipe participant aux compétitions | Une équipe possède plusieurs joueurs et inscriptions |
| Joueur | Joueur rattaché à une équipe | Un joueur appartient à une seule équipe |
| Tournoi | Compétition organisée pour un jeu | Un tournoi possède des inscriptions et des matchs |
| Inscription | Participation d'une équipe à un tournoi | Relie une équipe et un tournoi |
| Match | Rencontre entre deux équipes | Relie un tournoi à deux équipes et éventuellement un vainqueur |

Les relations et contraintes sont définies dans `AppDbContext`. Une contrainte d'unicité empêche une équipe de s'inscrire plusieurs fois au même tournoi. Les relations entre les matchs et les équipes utilisent une suppression restrictive afin d'éviter les suppressions en cascade ambiguës.

## Règles métier

- Un joueur appartient à une seule équipe.
- Un tournoi est associé à un jeu existant.
- Une équipe ne peut être inscrite qu'une fois à un tournoi.
- Un match oppose deux équipes différentes.
- Les équipes d'un match doivent être inscrites au tournoi concerné.
- Le vainqueur est déterminé lors de la saisie d'un score valide.
- Le classement est calculé à partir des matchs terminés.
- Les listes sont paginées avec une page et une taille de page.

## Structure du projet

Le dépôt contient deux applications principales :

### Backend

Le backend se trouve dans `TournoisE-Sports/Tournois-E-Sports`.

| Dossier ou fichier | Responsabilité |
|---|---|
| `Controllers` | Endpoints HTTP de l'API |
| `Models` | Entités métier et relations |
| `DTOs` | Objets d'entrée et de sortie de l'API |
| `Data` | Contexte Entity Framework, migrations et seed |
| `Repositories` | Accès aux données par entité |
| `Services` | JWT et calcul du classement |
| `Common` | Types communs, notamment la pagination |
| `Program.cs` | Configuration de l'application et du pipeline HTTP |
| `Migrations` | Historique du schéma de base de données |

### Frontend

Le frontend se trouve dans `TournoisE-Sports-Front`.

| Dossier | Responsabilité |
|---|---|
| `src/app/core/models` | Interfaces TypeScript et modèles |
| `src/app/core/services` | Services HTTP pour les ressources de l'API |
| `src/app/core/guards` | Protection des routes selon la session et les rôles |
| `src/app/core/interceptors` | Ajout du token JWT aux appels API |
| `src/app/features/auth` | Connexion et inscription |
| `src/app/features/tournois` | Liste, formulaire, détail, inscriptions, matchs et classement |
| `src/app/features/equipes` | Liste, formulaire et détail des équipes |
| `src/app/features/jeux` | Liste, formulaire et détail des jeux |
| `src/app/features/joueurs` | Liste et modification des joueurs |
| `src/app/shared` | Composants partagés, notamment la pagination et les statuts |

## Configuration et lancement

### Prérequis

- .NET SDK 9.
- SQL Server, SQL Server LocalDB ou SQL Server dans Docker.
- Node.js LTS et npm.
- Angular CLI compatible avec Angular 17.

### Configuration backend

La chaîne de connexion SQL Server et la configuration JWT sont lues depuis la configuration ASP.NET Core. La clé JWT et le mot de passe du compte administrateur doivent être fournis avec les User Secrets ou des variables d'environnement en dehors des fichiers versionnés.

Le projet utilise l'identifiant de User Secrets `tournois-esports-secrets`.

### Configuration frontend

L'URL de l'API de développement est définie dans `TournoisE-Sports-Front/src/environments/environment.development.ts`. La valeur actuellement utilisée est `https://localhost:7001/api`.

L'API autorise l'origine Angular locale `http://localhost:4200` via CORS.

### Ordre de démarrage

1. Démarrer SQL Server.
2. Restaurer les dépendances du projet backend.
3. Appliquer les migrations Entity Framework à la base de données.
4. Démarrer l'API backend.
5. Installer les dépendances du frontend.
6. Démarrer l'application Angular.

L'API est disponible sur l'URL HTTPS configurée par les profils de lancement. Swagger est disponible sur le chemin `/swagger` lorsque l'environnement est Development. Le frontend Angular est normalement disponible sur `http://localhost:4200`.

## Authentification et rôles

| Rôle | Principaux droits |
|---|---|
| Admin | Gestion complète, gestion des rôles et suppressions protégées |
| Organisateur | Gestion des jeux, équipes, tournois, inscriptions, matchs et scores |
| Joueur | Consultation des ressources et accès limité aux fonctions liées aux équipes |
| Utilisateur anonyme | Inscription et connexion |

Le frontend conserve la session d'authentification dans le stockage local du navigateur. Son intercepteur HTTP ajoute le token JWT uniquement aux appels destinés à l'API. Les guards empêchent l'accès aux pages protégées lorsqu'il n'existe pas de session valide ou lorsque le rôle requis manque.

La sécurité du frontend ne remplace pas celle du backend : l'API vérifie toujours l'identité et les rôles de l'utilisateur.

## API

Toutes les routes métier nécessitent une authentification, sauf l'inscription et la connexion. Les réponses peuvent utiliser les statuts HTTP 200, 201, 204, 400, 401, 403, 404 et 409 selon le résultat de l'opération.

### Authentification

| Méthode | Route | Accès | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Créer un compte |
| POST | `/api/auth/login` | Public | Se connecter et obtenir un JWT |

### Rôles

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/roles` | Admin | Lister les rôles |
| POST | `/api/roles` | Admin | Créer un rôle |
| POST | `/api/roles/assign` | Admin | Attribuer un rôle |
| POST | `/api/roles/remove` | Admin | Retirer un rôle |

### Jeux

| Méthode | Route | Accès |
|---|---|---|
| GET | `/api/jeux` | Authentifié |
| GET | `/api/jeux/{id}` | Authentifié |
| POST | `/api/jeux` | Admin, Organisateur |
| PUT | `/api/jeux/{id}` | Admin, Organisateur |
| DELETE | `/api/jeux/{id}` | Admin |

### Équipes et joueurs

| Méthode | Route | Accès |
|---|---|---|
| GET | `/api/equipes` | Authentifié |
| GET | `/api/equipes/{id}` | Authentifié |
| POST | `/api/equipes` | Admin, Organisateur |
| PUT | `/api/equipes/{id}` | Admin, Organisateur |
| DELETE | `/api/equipes/{id}` | Admin |
| GET | `/api/equipes/{id}/joueurs` | Authentifié |
| POST | `/api/equipes/{id}/joueurs` | Admin, Organisateur |
| GET | `/api/joueurs` | Authentifié |
| GET | `/api/joueurs/{id}` | Authentifié |
| PUT | `/api/joueurs/{id}` | Admin, Organisateur |
| DELETE | `/api/joueurs/{id}` | Admin |

### Tournois, inscriptions et matchs

| Méthode | Route | Accès | Description |
|---|---|---|---|
| GET | `/api/tournois` | Authentifié | Lister les tournois, avec filtre optionnel par jeu |
| GET | `/api/tournois/{id}` | Authentifié | Consulter un tournoi |
| POST | `/api/tournois` | Admin, Organisateur | Créer un tournoi |
| PUT | `/api/tournois/{id}` | Admin, Organisateur | Modifier un tournoi |
| DELETE | `/api/tournois/{id}` | Admin | Supprimer un tournoi |
| GET | `/api/tournois/{id}/classement` | Authentifié | Consulter le classement |
| GET | `/api/tournois/{id}/inscriptions` | Authentifié | Lister les inscriptions |
| POST | `/api/tournois/{id}/inscriptions` | Admin, Organisateur | Inscrire une équipe |
| DELETE | `/api/inscriptions/{id}` | Admin, Organisateur | Désinscrire une équipe |
| GET | `/api/tournois/{id}/matchs` | Authentifié | Lister les matchs |
| GET | `/api/matchs/{id}` | Authentifié | Consulter un match |
| POST | `/api/matchs` | Admin, Organisateur | Planifier un match |
| PUT | `/api/matchs/{id}/score` | Admin, Organisateur | Saisir un score |
| DELETE | `/api/matchs/{id}` | Admin | Supprimer un match |

### Pagination

Les principales listes acceptent les paramètres `page` et `pageSize`. La réponse contient les éléments demandés ainsi que la page courante, la taille de page, le nombre total d'éléments et le nombre total de pages.

## Application Angular

Les pages disponibles sont :

| Route | Fonction | Accès |
|---|---|---|
| `/login` | Connexion | Public |
| `/register` | Inscription | Public |
| `/tournois` | Liste des tournois | Connecté |
| `/tournois/new` | Création d'un tournoi | Admin, Organisateur |
| `/tournois/:id/edit` | Modification d'un tournoi | Admin, Organisateur |
| `/tournois/:id` | Détail, inscriptions, matchs et classement | Connecté |
| `/equipes` | Liste des équipes | Connecté |
| `/equipes/new` | Création d'une équipe | Admin, Organisateur |
| `/equipes/:id` | Détail d'une équipe et joueurs | Connecté |
| `/jeux` | Liste des jeux | Connecté |
| `/jeux/new` | Création d'un jeu | Admin, Organisateur |
| `/jeux/:id` | Détail d'un jeu | Connecté |
| `/joueurs/:equipeId` | Joueurs d'une équipe | Connecté |

L'interface utilise un thème sombre avec des accents violet et cyan, des badges de statut pour les tournois et des composants réutilisables pour la pagination.

## Données initiales

Au démarrage, le seed crée les rôles et un compte administrateur définis par la configuration. Il ajoute également des données de démonstration :

- Jeux : League of Legends, Valorant, EA FC 26 et Rocket League.
- Équipes : Team Vitality, Karmine Corp, Team Liquid, G2 Esports, Fnatic et Team Falcons.
- Plusieurs joueurs associés aux équipes.
- Un tournoi `Spring Cup 2026`.
- Des inscriptions et des matchs déjà terminés pour alimenter le classement.

Le seed n'ajoute les données de démonstration que lorsque la base ne contient pas encore de jeux.

## État du projet

Les principaux éléments du cahier des charges sont présents :

- [x] Inscription et connexion avec Identity.
- [x] JWT contenant les rôles.
- [x] Autorisation par rôle.
- [x] Gestion des jeux, équipes et joueurs.
- [x] Gestion des tournois et inscriptions.
- [x] Planification des matchs et saisie des scores.
- [x] Classement des équipes.
- [x] Pagination des listes.
- [x] Migrations Entity Framework et données initiales.
- [x] Frontend Angular avec routes protégées.
- [x] Documentation Swagger en développement.

## Licence

Projet réalisé dans un cadre pédagogique — examen .NET.
