# Sage Intacct REST API – Démo / Atelier

Application console (.NET 8) qui montre comment appeler l’**API REST Sage Intacct** : authentification (OAuth2), **Query** (lecture de données) et **Export** (PDF, CSV, etc.). Conçue pour les ateliers et l’onboarding de l’équipe.

---

## Prérequis

- **SDK .NET 8** – [Télécharger](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Identifiants Sage Intacct** : Client ID, Client Secret et un utilisateur Web Services

---

## Installation

### 1. Cloner le dépôt

```bash
git clone <url-du-depot>
cd intacct_rest_api
```

### 2. Ajouter `appsettings.json` (obligatoire)

Le fichier `appsettings.json` **n’est pas** dans le dépôt (il contient des secrets et est dans `.gitignore`). Il faut le créer à la racine du projet.

**Créer un fichier `appsettings.json`** avec :

```json
{
  "IdClient": "votre-client-id.app.sage.com",
  "SecretClient": "votre-secret-client",
  "Utilisateur": "webservice@votre-societe"
}
```

Remplacer par vos vraies valeurs Sage Intacct (Société > Web Services ou votre admin).

**Optionnel :** ajouter un `appsettings.example.json` avec des valeurs factices et le committer pour que l’équipe sache quelles clés renseigner.

### 3. Restaurer et lancer

```bash
dotnet restore
dotnet run
```

L’app va : obtenir un token, exécuter un Query exemple, désérialiser la réponse, exécuter un Export (PDF) et enregistrer le fichier dans le dossier de l’exe.

---

## Structure du projet

```
intacct_rest_api/
├── Program.cs                 # Point d'entrée : config, auth, query, export, enregistrement fichier
├── appsettings.json           # Vos secrets (à créer ; pas dans git)
├── intacct_rest_api.csproj
├── Models/
│   ├── Token.cs               # Token OAuth (access, refresh, expiration)
│   ├── QueryRequest.cs        # Corps Query : object, fields, filters, orderBy, start, size...
│   ├── QueryResponse.cs       # Réponse Query : Result (lignes) + Meta (pagination)
│   ├── Filter.cs              # Helpers pour les filtres (Equal, Between, GreaterThan...)
│   ├── FilterExpression.cs    # Combiner les filtres avec And/Or (Ref(0), Ref(1)...)
│   ├── FilterParameters.cs    # caseSensitiveComparison, includePrivate
│   ├── ExportRequest.cs       # Interne : query + fileType pour Export
│   └── ExportFileType.cs      # Pdf, Csv, Word, Xml, Xlsx
└── Services/
    └── IntacctService.cs      # Client HTTP : ObtenirToken, Query, Export, RafraichirToken, RevokerToken
```

---

## Vue d’ensemble du code

### 1. Configuration et auth (`Program.cs`)

- La **configuration** est lue depuis `appsettings.json` (IdClient, SecretClient, Utilisateur).
- **IntacctService** est créé avec l’URL de base `https://api.intacct.com/ia/api/v1/` et ces valeurs.
- **ObtenirToken()** appelle l’endpoint OAuth2 token (client_credentials). La réponse est désérialisée en **Token** (Newtonsoft) pour récupérer `AccessToken`, `RefreshToken`, `DateExpiration`, etc.
- Le token est passé explicitement à Query et Export (pour pouvoir changer de société/entité plus tard en utilisant un autre token).

### 2. Query (`IntacctService.Query`)

- **QueryRequest** décrit la requête :
  - **Object** (obligatoire) : objet API, ex. `"accounts-payable/bill"`.
  - **Fields** (obligatoire) : liste des champs à retourner.
  - **Filters** (optionnel) : liste de filtres construits avec **Filter** (Equal, Between, GreaterThan, etc.). Les dates (DateTime/DateOnly) sont normalisées en `yyyy-MM-dd` dans Filter.
  - **FilterExpression** (optionnel) : chaîne qui combine les filtres par index, ex. `"1 and 2"`. Construite avec **FilterExpression.Ref(0)**, **And**, **Or**, puis **FilterExpression.Build(filters, expr)** pour valider les indices.
  - **FilterParameters** (optionnel) : caseSensitiveComparison, includePrivate.
  - **OrderBy** (optionnel) : liste de `{ "champ": "asc" }` ou `"desc"`.
  - **Start**, **Size** (optionnel) : pagination.

- **Query(request, accessToken)** envoie un POST vers `/service/core/query` avec ce corps et retourne la réponse brute.

### 3. Désérialiser la réponse Query

- Le corps de la réponse contient `"ia::result"` (tableau de lignes) et `"ia::meta"` (totalCount, start, pageSize, next, previous).
- Utiliser **Newtonsoft** : `JsonConvert.DeserializeObject<QueryResponse>(response.Content)`.
- **QueryResponse.Result** est une `List<Dictionary<string, object>>` : chaque ligne est un dictionnaire (nom du champ → valeur). **QueryResponse.Meta** contient la pagination.

### 4. Export (`IntacctService.Export`)

- Même **QueryRequest** plus **ExportFileType** (Pdf, Csv, Word, Xml, Xlsx).
- **Export(request, fileType, accessToken)** envoie un POST vers `/service/core/export` avec le corps `{ "query": request, "fileType": "pdf" }` et retourne les octets du fichier.
- L’exemple dans `Program.cs` enregistre le fichier dans le dossier de l’exe avec un nom du type `object-export-ddMMyyyy-HHmmss.ext`.

### 5. Filter et FilterExpression

- Les méthodes **Filter** retournent un `Dictionary<string, object>` au format attendu par l’API (ex. `{ "$gt": { "totalTxnAmount": "100" } }`). On s’en sert pour construire **QueryRequest.Filters**.
- **FilterExpression.Ref(0)**, **Ref(1)** désignent le premier et le deuxième filtre de la liste. **And(left, right)** et **Or(left, right)** les combinent. **Build(filters, expr)** valide les indices et retourne la chaîne (ex. `"1 and 2"`) pour **QueryRequest.FilterExpression**.

### 6. Rafraîchir / révoquer le token (pour plus tard)

- **RafraichirToken(refreshToken)** et **RevokerToken(accessToken)** sont implémentés dans **IntacctService** ; leur utilisation est commentée dans `Program.cs` pour les flux refresh/revoke.

---

## Exemple : construire une Query

```csharp
var filters = new List<Dictionary<string, object>>
{
    Filter.GreaterThan("totalTxnAmount", "100"),
    Filter.Between("postingDate", new DateTime(2025, 1, 1), new DateTime(2025, 1, 31))
};
var expr = FilterExpression.And(FilterExpression.Ref(0), FilterExpression.Ref(1));
var filterExpressionString = FilterExpression.Build(filters, expr);  // "1 and 2"

var request = new QueryRequest
{
    Object = "accounts-payable/bill",
    Fields = new List<string> { "id", "billNumber", "postingDate", "totalTxnAmount" },
    Filters = filters,
    FilterExpression = filterExpressionString,
    FilterParameters = new FilterParameters { CaseSensitiveComparison = false, IncludePrivate = false },
    OrderBy = new List<Dictionary<string, string>> { new() { ["totalTxnAmount"] = "desc" } },
    Start = 1,
    Size = 100
};

var response = await intacctService.Query(request, token.AccessToken);
var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(response.Content);
// queryResponse.Result = lignes, queryResponse.Meta = pagination
```

---

## Licence

Utilisation libre pour l’équipe et les ateliers. Les conditions d’utilisation de l’API Sage Intacct s’appliquent à l’API elle-même.
