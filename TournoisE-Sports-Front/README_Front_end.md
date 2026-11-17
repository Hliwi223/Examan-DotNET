# 🖥 Frontend — Application Angular Tournois d'e-sport

Application **Angular** du projet *Tournois d'e-sport* (Examen .NET · Sujet 2). Elle consomme l'API REST réelle avec l'authentification **JWT** : connexion, liste paginée, création, modification et suppression, avec un affichage adapté au rôle de l'utilisateur et un thème visuel e-sport.

🏠 [README principal](../README.md) · ⚙️ [Documentation de l'API backend](../backend/README.md)

> Le front-end est évalué sur son **fonctionnement**, pas sur son design : le thème e-sport reste volontairement léger.

---

## 📑 Sommaire

1. [Stack technique](#-stack-technique)
2. [Prérequis](#-prérequis)
3. [Installation et lancement](#-installation-et-lancement)
4. [Configuration](#-configuration)
5. [Pages et routes](#-pages-et-routes)
6. [Authentification](#-authentification)
7. [Appels à l'API](#-appels-à-lapi)
8. [Gestion des erreurs](#-gestion-des-erreurs)
9. [Design et thème e-sport](#-design-et-thème-e-sport)
10. [Structure du projet](#-structure-du-projet)
11. [Scripts](#-scripts)
12. [Dépannage](#-dépannage)
13. [Checklist front-end](#-checklist-front-end)

---

## 🧰 Stack technique

| Domaine | Technologie |
|---|---|
| Framework | Angular 17 ou supérieur (composants **standalone**) |
| Langage | TypeScript |
| Routage | `@angular/router` avec guards fonctionnels |
| Appels HTTP | `HttpClient` + intercepteur fonctionnel (token JWT) |
| Styles | SCSS avec variables CSS |
| Bibliothèque UI | Au choix : Angular Material, Bootstrap ou Tailwind CSS |

---

## ✅ Prérequis

- [Node.js](https://nodejs.org) en version LTS compatible avec votre Angular CLI (vérifier avec `ng version`) et npm
- Angular CLI :
  ```bash
  npm install -g @angular/cli
  ```
- L'**API backend lancée** (voir [`backend/README.md`](../backend/README.md)), par exemple sur `https://localhost:7000`

---

## 🚀 Installation et lancement

```bash
cd frontend
npm install
ng serve
```

L'application est disponible sur `http://localhost:4200`.

**Compte de test** : se connecter avec le compte administrateur créé par le seed de l'API (`SeedAdmin`, voir le README du backend).

---

## ⚙️ Configuration

### URL de l'API

Générer les fichiers d'environnement :

```bash
ng generate environments
```

`src/environments/environment.development.ts` (utilisé par `ng serve`) :

```ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7000/api'   // à adapter au port de l'API
};
```

`src/environments/environment.ts` (utilisé par `ng build`) :

```ts
export const environment = {
  production: true,
  apiUrl: 'https://<url-de-l-api-deployee>/api'
};
```

> ⚠️ Les fichiers d'environnement sont visibles dans le navigateur : **aucun secret** dedans.

### CORS

L'API doit autoriser l'origine `http://localhost:4200` (voir la section *CORS* du README backend). Sans cela, le navigateur bloque toutes les requêtes.

---

## 🧭 Pages et routes

| Route | Page | Gardes |
|---|---|---|
| `/login` | Connexion | — |
| `/register` | Inscription | — |
| `/tournois` | Liste paginée des tournois, avec suppression (confirmation) | `authGuard` |
| `/tournois/new` | Création d'un tournoi | `authGuard` + `roleGuard` (Admin, Organisateur) |
| `/tournois/:id/edit` | Modification d'un tournoi | `authGuard` + `roleGuard` (Admin, Organisateur) |
| `/tournois/:id` | Détail : onglets *Inscriptions*, *Matchs*, *Classement* | `authGuard` |
| `/equipes` | Liste et CRUD des équipes (avec leurs joueurs) | `authGuard` |
| `/jeux` | Liste et CRUD des jeux | `authGuard` |

> 📌 Adapter ce tableau aux pages réellement implémentées. Le minimum exigé : connexion, liste, création, modification, suppression.

Configuration des routes :

```ts
const organisateurs = { roles: ['Admin', 'Organisateur'] };

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: 'tournois',
    canActivate: [authGuard],
    children: [
      { path: '', component: TournoiListComponent },
      { path: 'new', component: TournoiFormComponent, canActivate: [roleGuard], data: organisateurs },
      { path: ':id/edit', component: TournoiFormComponent, canActivate: [roleGuard], data: organisateurs },
      { path: ':id', component: TournoiDetailComponent }
    ]
  },
  { path: '', pathMatch: 'full', redirectTo: 'tournois' },
  { path: '**', redirectTo: 'tournois' }
];
```

### Affichage selon le rôle

| Élément | Visible pour |
|---|---|
| Lecture des listes et des détails | Tous les utilisateurs connectés |
| Boutons *Créer* et *Modifier* | Admin, Organisateur |
| Bouton *Supprimer* | Admin |

```html
@if (auth.hasAnyRole(['Admin', 'Organisateur'])) {
  <a routerLink="/tournois/new" class="btn">Nouveau tournoi</a>
}
```

> 🔒 Masquer un bouton est uniquement cosmétique : la vraie sécurité est assurée par l'API (`401` / `403`).

---

## 🔐 Authentification

**Principe** : `login` renvoie le token JWT, sa date d'expiration, l'email et les rôles. La session est stockée dans le `localStorage`, l'intercepteur ajoute le token à chaque appel de l'API et les guards protègent les routes.

```
Connexion → session stockée → intercepteur (Authorization: Bearer ...) → API
                  │
                  └→ guards (connecté ? rôle requis ?) → routes
```

`AuthService` :

```ts
export interface AuthSession {
  token: string;
  expiration: string;   // date UTC ISO 8601
  email: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storageKey = 'esport_session';

  login(email: string, password: string) {
    return this.http
      .post<AuthSession>(`${environment.apiUrl}/auth/login`, { email, password })
      .pipe(tap(session => localStorage.setItem(this.storageKey, JSON.stringify(session))));
  }

  register(email: string, password: string) {
    return this.http.post(`${environment.apiUrl}/auth/register`, { email, password });
  }

  logout() {
    localStorage.removeItem(this.storageKey);
    this.router.navigate(['/login']);
  }

  get session(): AuthSession | null {
    const raw = localStorage.getItem(this.storageKey);
    return raw ? (JSON.parse(raw) as AuthSession) : null;
  }

  get token(): string | null {
    return this.session?.token ?? null;
  }

  isLoggedIn(): boolean {
    const session = this.session;
    return !!session && new Date(session.expiration) > new Date();
  }

  hasAnyRole(roles: string[]): boolean {
    return this.session?.roles.some(role => roles.includes(role)) ?? false;
  }
}
```

Intercepteur HTTP (le token n'est envoyé qu'à l'API, jamais à un autre domaine) :

```ts
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.token;
  const isApiCall = req.url.startsWith(environment.apiUrl);

  const authReq = token && isApiCall
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // 401 hors connexion / inscription : session expirée ou token invalide
      if (error.status === 401 && !req.url.includes('/auth/')) {
        auth.logout();
      }
      return throwError(() => error);
    })
  );
};
```

Guards :

```ts
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isLoggedIn() ? true : router.createUrlTree(['/login']);
};

export const roleGuard: CanActivateFn = route => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const roles = route.data['roles'] as string[] | undefined;
  return !roles || auth.hasAnyRole(roles) ? true : router.createUrlTree(['/tournois']);
};
```

Enregistrement dans `app.config.ts` :

```ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
};
```

> ℹ️ Le `localStorage` est simple et suffisant pour cet examen. En production, on préférerait un cookie `HttpOnly` pour limiter l'impact d'une faille XSS.

---

## 📡 Appels à l'API

Un service par ressource (`TournoiService`, `EquipeService`, `JeuService`, `MatchService`, ...) et des modèles TypeScript typés.

Toutes les listes sont **paginées** par l'API :

```ts
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
```

Exemple de service :

```ts
@Injectable({ providedIn: 'root' })
export class TournoiService {
  private readonly http = inject(HttpClient);
  private readonly url = `${environment.apiUrl}/tournois`;

  getAll(page = 1, pageSize = 10, jeuId?: number) {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (jeuId) {
      params = params.set('jeuId', jeuId);
    }
    return this.http.get<PagedResult<Tournoi>>(this.url, { params });
  }

  getById(id: number) {
    return this.http.get<Tournoi>(`${this.url}/${id}`);
  }

  create(dto: TournoiForm) {
    return this.http.post<Tournoi>(this.url, dto);
  }

  update(id: number, dto: TournoiForm) {
    return this.http.put<void>(`${this.url}/${id}`, dto);
  }

  delete(id: number) {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
```

Un composant de pagination réutilisable (dossier `shared/`) prend la page courante et le nombre de pages, et émet la page demandée :

```html
<app-paginator
  [page]="result.page"
  [totalPages]="result.totalPages"
  (pageChange)="load($event)" />
```

---

## ⚠️ Gestion des erreurs

| Code | Comportement du front |
|---|---|
| `400` | Afficher les messages de validation sous les champs concernés (objet `errors` de la réponse) |
| `401` | Déconnexion et redirection vers `/login` (géré par l'intercepteur) |
| `403` | Message « Accès refusé : droits insuffisants » |
| `404` | Message « Ressource introuvable » |
| `409` | Afficher le `detail` renvoyé par l'API (ex. équipe déjà inscrite) |

Extraction des messages d'une réponse d'erreur :

```ts
export function extractErrors(error: HttpErrorResponse): string[] {
  const body = error.error;
  if (body?.errors) {
    return Object.values(body.errors as Record<string, string[]>).flat();
  }
  return [body?.detail ?? body?.title ?? 'Une erreur est survenue.'];
}
```

> 📌 À adapter au format exact des erreurs renvoyées par votre API (voir *Codes HTTP et erreurs* dans le README backend).

Toute suppression demande une **confirmation** avant l'appel à l'API.

---

## 🎨 Design et thème e-sport

Le design reste **cohérent avec le sujet** : une plateforme de tournois d'e-sport.

### Identité visuelle

| Élément | Proposition |
|---|---|
| Mode | Thème sombre par défaut |
| Couleurs | Fond bleu nuit avec accents néon (violet, cyan) |
| Titres | Police « gaming » (*Orbitron* ou *Rajdhani*) |
| Texte | Police lisible (*Inter*) |
| Effets | Légère lueur au survol des boutons et des cartes, sans animation lourde |

Polices à ajouter dans `src/index.html` :

```html
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;600&family=Orbitron:wght@600;800&display=swap" rel="stylesheet">
```

Styles globaux dans `src/styles.scss` :

```scss
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
  margin: 0;
  background: var(--bg);
  color: var(--text);
  font-family: 'Inter', system-ui, sans-serif;
}

h1, h2, h3 {
  font-family: 'Orbitron', 'Inter', sans-serif;
  letter-spacing: 0.04em;
}

.card {
  background: var(--surface);
  border: 1px solid rgba(139, 92, 246, 0.25);
  border-radius: 12px;
  transition: box-shadow 0.2s;

  &:hover {
    box-shadow: 0 0 16px rgba(139, 92, 246, 0.35);
  }
}
```

### Badges de statut

| Statut renvoyé par l'API | Libellé affiché | Couleur |
|---|---|---|
| `AVenir` | À venir | Cyan (`--accent-2`) |
| `EnCours` | En cours | Violet (`--accent`) |
| `Termine` | Terminé | Gris (`--muted`) |

```scss
.badge {
  padding: 0.2rem 0.6rem;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 600;

  &--avenir  { color: var(--accent-2); border: 1px solid var(--accent-2); }
  &--encours { color: var(--accent);   border: 1px solid var(--accent); }
  &--termine { color: var(--muted);    border: 1px solid var(--muted); }
}
```

### Rendu par page

| Page | Idée de rendu |
|---|---|
| Connexion | Formulaire centré sur fond sombre |
| Liste des tournois | Cartes avec nom du jeu, dates, dotation et badge de statut |
| Détail d'un tournoi | Onglets *Inscriptions*, *Matchs*, *Classement* |
| Matchs | Carte « Équipe A **VS** Équipe B » avec le score et le vainqueur mis en avant |
| Classement | Tableau (rang, équipe, victoires, différence) avec podium 🥇🥈🥉 pour le top 3 |
| Équipes | Cartes avec le tag de l'équipe et la liste des pseudos des joueurs |

### Recommandations

- Interface **responsive** (mobile et desktop).
- Contraste suffisant du texte sur fond sombre et focus clavier visible.
- Messages d'erreur de validation visibles et confirmation avant toute suppression.

---

## 🗂 Structure du projet

```
frontend/
├── src/
│   ├── app/
│   │   ├── core/
│   │   │   ├── guards/         # authGuard, roleGuard
│   │   │   ├── interceptors/   # authInterceptor
│   │   │   ├── models/         # interfaces TypeScript (Tournoi, Equipe, PagedResult<T>, ...)
│   │   │   └── services/       # AuthService, TournoiService, EquipeService, ...
│   │   ├── features/
│   │   │   ├── auth/           # connexion, inscription
│   │   │   ├── tournois/       # liste, formulaire (création / modification), détail
│   │   │   ├── equipes/
│   │   │   ├── jeux/
│   │   │   └── matchs/
│   │   ├── shared/             # paginator, boîte de confirmation, badge de statut
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   ├── environments/
│   ├── index.html
│   └── styles.scss
├── angular.json
├── package.json
└── README.md
```

> Les noms de fichiers générés par `ng generate` (avec ou sans suffixe `.component`, `.service`) dépendent de la version du CLI.

---

## 📜 Scripts

| Commande | Rôle |
|---|---|
| `ng serve` (ou `npm start`) | Serveur de développement sur `http://localhost:4200` |
| `ng build` (ou `npm run build`) | Build de production dans `dist/` |
| `ng test` (ou `npm test`) | Tests unitaires |

---

## 🛠 Dépannage

| Problème | Solution |
|---|---|
| Erreur CORS dans la console du navigateur | Vérifier l'origine autorisée côté API (`http://localhost:4200`) et l'ordre `UseCors` avant `UseAuthentication`, puis relancer l'API |
| `ERR_CERT_AUTHORITY_INVALID` | Approuver le certificat de développement : `dotnet dev-certs https --trust` |
| Aucune donnée affichée | Vérifier `apiUrl` dans `environment.development.ts` (port de l'API) et que l'API est démarrée |
| `401` sur toutes les requêtes | L'intercepteur n'est pas enregistré dans `app.config.ts`, ou le token est expiré : se reconnecter |
| Redirection permanente vers `/login` | La session a expiré (`expiration` dépassée) ou `expiration` n'est pas une date UTC valide |
| `403` | Le compte n'a pas le rôle requis : se connecter avec un compte `Admin` ou `Organisateur` |
| `ng : commande introuvable` | Installer le CLI : `npm install -g @angular/cli` |
| Erreur de version de Node | Installer une version LTS compatible avec votre Angular CLI |

---

## 📋 Checklist front-end

- [ ] Connexion (et inscription) avec l'API réelle
- [ ] Token JWT envoyé automatiquement par l'intercepteur
- [ ] Guards : utilisateur connecté et rôle requis
- [ ] Liste **paginée** (tournois, équipes, jeux)
- [ ] Création
- [ ] Modification
- [ ] Suppression avec confirmation
- [ ] Boutons affichés selon le rôle
- [ ] Gestion des erreurs `400`, `401`, `403`, `404`, `409`
- [ ] Thème e-sport (sombre, accents néon, badges de statut)
- [ ] Interface responsive
