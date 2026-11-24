# TicketFlow — projet de départ (backend + frontend)

Application de billetterie en trois composants :

```text
Navigateur --> frontend (React servi par nginx) --/api--> backend (API ASP.NET Core) --> PostgreSQL
```

- **Billetterie** : liste des événements, prix, places restantes, réservation.
- **Tableau de bord** (onglet « Tableau de bord », mot de passe `admin123`) : chiffre d'affaires,
  places vendues, taux de remplissage, réservations par jour, remplissage par événement,
  annulation, création d'événements, export JSON.

## Ce qui est fourni

| Chemin | Contenu |
| --- | --- |
| `backend/src/TicketFlow.Api/` | L'API : points d'entrée dans `Program.cs`, logique dans `Services/`, base dans `Data/` |
| `backend/tests/TicketFlow.Api.Tests/` | Les tests automatisés de l'API |
| `backend/TicketFlow.slnx` | La solution, qui regroupe l'API et les tests |
| `frontend/src/` | L'interface React : `Shop.jsx` (billetterie), `Dashboard.jsx` (tableau de bord), `api.js` (appels à l'API) |
| `frontend/nginx.conf` | La configuration du serveur web qui servira l'interface et relaiera `/api` vers le backend |

## Ce que tu vas écrire toi-même

`backend/Dockerfile`, `backend/.dockerignore`, `frontend/Dockerfile`, `frontend/.dockerignore`,
`docker-compose.yml`, puis plus tard `Jenkinsfile`. Suis le document « Parcours pas à pas ».

## Points d'entrée de l'API

| Méthode | Chemin | Accès |
| --- | --- | --- |
| GET | `/health` | public |
| GET | `/api/events` | public |
| GET | `/api/events/{id}` | public |
| POST | `/api/events/{id}/reservations` | public |
| POST | `/api/login`, `/api/logout` | public |
| GET | `/api/admin/stats` | administrateur |
| POST | `/api/admin/events` | administrateur |
| DELETE | `/api/admin/reservations/{id}` | administrateur |
| GET | `/api/admin/export` | administrateur |

## Configuration du backend

| Réglage | Valeur par défaut | Variable d'environnement |
| --- | --- | --- |
| Port d'écoute dans le conteneur | `8080` | `ASPNETCORE_HTTP_PORTS` |
| Connexion à la base | hôte `db`, base, utilisateur `ticketflow`, mot de passe `ticketflow123` | `ConnectionStrings__TicketFlow` |
| Mot de passe administrateur | `admin123` | `Admin__Password` |

Au démarrage, l'API attend sa base pendant une minute environ, puis s'arrête si elle ne la trouve pas.

Ce projet contient volontairement des défauts de qualité et de sécurité : les outils des étapes
suivantes vont les révéler. Ne le mets pas en ligne tel quel.
