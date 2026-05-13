# Sage Intacct REST API – Cours / Atelier

Application console (.NET 8) minimale pour apprendre à appeler l'**API REST Sage Intacct** : authentification OAuth2 (**Client Credentials**) et requête **Query** (lecture de données).

---

## Prérequis

- **SDK .NET 8** – [Télécharger](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Identifiants Sage Intacct** :
  - Client ID (ex. `xxx.app.sage.com`)
  - Client Secret
  - Utilisateur Web Services (ex. `webservice@ma-societe`)

---

## Installation

### 1. Cloner le dépôt

```bash
git clone <url-du-depot>
cd intacct_rest_api
```

### 2. Créer `appsettings.json`

Le fichier `appsettings.json` n'est pas dans le dépôt (secrets). Créez-le à la racine :

```json
{
  "IdClient": "votre-client-id.app.sage.com",
  "SecretClient": "votre-secret-client",
  "Utilisateur": "webservice@votre-societe"
}
```

### 3. Restaurer et lancer

```bash
dotnet restore
dotnet run
```

L'application va : obtenir un token, exécuter une Query sur les bills, et afficher les résultats.

---

## Structure du projet

| Fichier / Dossier | Rôle |
|-------------------|------|
| **Program.cs** | Point d'entrée : configuration, auth, query. |
| **appsettings.json** | Secrets (à créer ; ignoré par git). |
| **Services/IntacctService.cs** | Client HTTP : `ObtenirToken()` et `Query()`. |
| **Models/Token.cs** | Modèle du token OAuth2 (access_token, expires_in, etc.). |
| **Models/Query/QueryRequest.cs** | Corps d'une requête Query : object, fields, filters, orderBy, start, size. |
| **Models/Query/QueryResponse.cs** | Réponse Query : Result (liste de lignes) + Meta (pagination). |

---

## Flux de l'application

### 1. Configuration

La configuration est lue depuis `appsettings.json` (IdClient, SecretClient, Utilisateur).

### 2. Authentification (OAuth2 Client Credentials)

POST vers `oauth2/token` avec `grant_type=client_credentials`. La réponse contient `access_token`, `refresh_token`, `expires_in`.

### 3. Query

POST vers `/services/core/query` avec un objet, des champs, et optionnellement des filtres, un tri et une pagination. La réponse contient les résultats (`ia::result`) et les métadonnées (`ia::meta`).

---

## Dépannage

| Problème | Solution |
|----------|----------|
| Erreur d'authentification | Vérifier IdClient, SecretClient, Utilisateur dans `appsettings.json`. |
| `appsettings.json` introuvable | Le fichier doit être à la racine (même dossier que le `.csproj`). |
| Query retourne vide | Vérifier le nom de l'objet et des champs dans la doc API Intacct. |

---

## Licence

Utilisation libre pour l'équipe et les ateliers.
