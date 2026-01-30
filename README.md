# Sage Intacct REST API – Cours / Atelier

Application console (.NET 8) pour apprendre à appeler l’**API REST Sage Intacct** : authentification OAuth2, **Query** (lecture) et **Export** (PDF, CSV, etc.). Support de cours pour ateliers et onboarding.

---

## Objectifs du cours

À l’issue de ce cours, vous saurez :

1. **Configurer** une application .NET pour appeler l’API Intacct (config, secrets).
2. **Obtenir un token** OAuth2 (client_credentials) et l’utiliser dans les requêtes.
3. **Construire une Query** : object, fields, filtres, expression, tri, pagination.
4. **Désérialiser** la réponse Query (Result + Meta) avec Newtonsoft.
5. **Exporter** le résultat d’une requête en fichier (PDF, CSV, etc.) et l’enregistrer.

---

## Prérequis

- **SDK .NET 8** – [Télécharger](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Identifiants Sage Intacct** :
  - Client ID (ex. `xxx.app.sage.com`)
  - Client Secret
  - Utilisateur Web Services (ex. `webservice@ma-societe`)

Ces identifiants se configurent dans Sage Intacct (Société > Web Services ou via votre administrateur).

---

## Installation pas à pas

### 1. Cloner le dépôt

```bash
git clone <url-du-depot>
cd intacct_rest_api
```

### 2. Créer `appsettings.json` (obligatoire)

Le fichier `appsettings.json` **n’est pas** dans le dépôt pour des raisons de sécurité (secrets). Vous devez le créer à la **racine du projet** (à côté de `Program.cs` et du `.csproj`).

**Contenu à mettre dans `appsettings.json` :**

```json
{
  "IdClient": "votre-client-id.app.sage.com",
  "SecretClient": "votre-secret-client",
  "Utilisateur": "webservice@votre-societe"
}
```

Remplacez par vos vraies valeurs. Ne commitez jamais ce fichier.

**Conseil :** vous pouvez ajouter un fichier `appsettings.example.json` (avec des valeurs factices) et le committer pour que l’équipe sache quelles clés renseigner.

### 3. Restaurer les packages et lancer

```bash
dotnet restore
dotnet run
```

L’application va successivement : obtenir un token, exécuter une Query exemple, afficher les résultats, exécuter un Export PDF et enregistrer le fichier. La fenêtre reste ouverte jusqu’à ce que vous appuyiez sur Entrée.

---

## Structure du projet

| Fichier / Dossier | Rôle |
|-------------------|------|
| **Program.cs** | Point d’entrée : configuration, auth, Query, désérialisation, Export, enregistrement du fichier. |
| **appsettings.json** | Secrets (à créer ; ignoré par git). |
| **Models/Token.cs** | Modèle du token OAuth (AccessToken, RefreshToken, DateExpiration, EstExpire). Désérialisation avec Newtonsoft. |
| **Models/QueryRequest.cs** | Corps d’une requête Query : Object, Fields, Filters, FilterExpression, FilterParameters, OrderBy, Start, Size. Sérialisé par RestSharp (System.Text.Json). |
| **Models/QueryResponse.cs** | Réponse Query : Result (liste de lignes) + Meta (TotalCount, Start, PageSize, Next, Previous). Désérialisation avec Newtonsoft. |
| **Models/Filter.cs** | Helpers pour construire les filtres (Equal, NotEqual, LessThan, GreaterThan, Between, In, Contains, etc.). Les DateTime/DateOnly sont normalisés en `yyyy-MM-dd`. |
| **Models/FilterExpression.cs** | Combinaison de filtres par index : Ref(0), Ref(1), And(left, right), Or(left, right), Build(filters, expr) pour obtenir la chaîne `"1 and 2"` et valider les indices. |
| **Models/FilterParameters.cs** | Paramètres optionnels des filtres : CaseSensitiveComparison, IncludePrivate. |
| **Models/ExportRequest.cs** | Corps interne de l’Export : Query + FileType. |
| **Models/ExportFileType.cs** | Enum des formats d’export : Pdf, Csv, Word, Xml, Xlsx. |
| **Services/IntacctService.cs** | Client HTTP (RestSharp) : ObtenirToken, RafraichirToken, RevokerToken, Query, Export. |

---

## Parcours du code (flux démo)

### Étape 1 – Configuration

La configuration est lue depuis `appsettings.json` (IdClient, SecretClient, Utilisateur). Le **IntacctService** est instancié avec l’URL de base de l’API Intacct et ces trois valeurs.

**À retenir :** ne jamais mettre les secrets dans le code ; toujours les externaliser (fichier config, variables d’environnement).

---

### Étape 2 – Authentification (OAuth2)

- **ObtenirToken()** envoie une requête POST vers `oauth2/token` avec `grant_type=client_credentials`, `client_id`, `client_secret`, `username`.
- La réponse JSON est désérialisée dans **Token** (Newtonsoft) : on récupère `AccessToken`, `RefreshToken`, `ExpiresIn`, et on calcule `DateExpiration` et `EstExpire`.
- Le token est **passé explicitement** à Query et Export (pas stocké dans le service). Cela permet plus tard de gérer plusieurs sociétés/entités en utilisant des tokens différents.

**À retenir :** chaque appel à Query ou Export doit être fait avec un token valide. Rafraîchir le token (RafraichirToken) ou en obtenir un nouveau si nécessaire.

---

### Étape 3 – Requête Query

Une **Query** permet de lire des données selon un **object** (ex. `accounts-payable/bill`), une liste de **fields**, et éventuellement des **filtres**, un **tri** et une **pagination**.

- **QueryRequest** contient :
  - **Object** (obligatoire) : nom de l’objet API.
  - **Fields** (obligatoire) : champs à retourner.
  - **Filters** (optionnel) : liste de filtres construits avec **Filter** (ex. `Filter.GreaterThan("totalTxnAmount", "100")`, `Filter.Between("postingDate", date1, date2)`). Les dates sont formatées en `yyyy-MM-dd` automatiquement.
  - **FilterExpression** (optionnel) : chaîne qui combine les filtres par **index 1-based** (ex. `"1 and 2"` = filtre 0 ET filtre 1). On la construit avec **FilterExpression.Ref(0)**, **And**, **Or**, puis **FilterExpression.Build(filters, expr)** pour valider les indices.
  - **FilterParameters** (optionnel) : sensibilité à la casse, champs privés.
  - **OrderBy** (optionnel) : liste de paires `{ "champ": "asc" }` ou `"desc"`.
  - **Start**, **Size** (optionnel) : pagination (premier enregistrement, nombre max 4000).

- **Query(request, accessToken)** envoie un POST vers `/services/core/query` avec le corps JSON et l’en-tête `Authorization: Bearer <token>`.

**À retenir :** l’ordre des filtres dans la liste détermine les numéros utilisés dans FilterExpression (1 = premier filtre, 2 = deuxième, etc.).

---

### Étape 4 – Désérialiser la réponse Query

La réponse du endpoint Query contient :

- **"ia::result"** : tableau d’objets (une ligne par enregistrement ; les clés sont les noms des champs).
- **"ia::meta"** : métadonnées (totalCount, start, pageSize, next, previous).

On utilise **Newtonsoft** : `JsonConvert.DeserializeObject<QueryResponse>(response.Content)`. Ensuite :

- **queryResponse.Result** : `List<Dictionary<string, object>>` — chaque élément est une ligne (champ → valeur).
- **queryResponse.Meta** : pagination et total.

**À retenir :** le type générique des lignes (dictionnaire) permet de gérer n’importe quel object/fields sans modèle C# spécifique par objet.

---

### Étape 5 – Export

L’**Export** réutilise la **même QueryRequest** et ajoute un **format de fichier** (Pdf, Csv, Word, Xml, Xlsx).

- **Export(request, fileType, accessToken)** envoie un POST vers `/services/core/export` avec le corps `{ "query": request, "fileType": "pdf" }` (ou autre format).
- La réponse est binaire (**RawBytes**). On l’enregistre avec `File.WriteAllBytes(cheminComplet, reponseExport.RawBytes)`.

Dans la démo, le nom du fichier suit le pattern : `object-export-ddMMyyyy-HHmmss.ext` (ex. `accounts-payable-bill-export-30012025-143052.pdf`).

**À retenir :** Query retourne du JSON ; Export retourne un fichier (PDF, CSV, etc.). La structure de la requête (object, fields, filters, etc.) est la même pour les deux.

---

## Filtres et FilterExpression (détail)

### Construire un filtre

Chaque filtre est un dictionnaire au format attendu par l’API : `{ "$op": { "champ": valeur } }`. On utilise les méthodes statiques de **Filter** pour ne pas écrire ce JSON à la main :

- Comparaisons : **Equal**, **NotEqual**, **LessThan**, **LessThanOrEqual**, **GreaterThan**, **GreaterThanOrEqual**
- Plage : **Between**, **NotBetween** (valeur = tableau de 2 éléments)
- Listes : **In**, **NotIn**
- Texte : **Contains**, **NotContains**, **StartsWith**, **NotStartsWith**, **EndsWith**, **NotEndsWith**

Vous pouvez passer des **DateTime** ou **DateOnly** directement ; ils sont convertis en `yyyy-MM-dd` dans Filter.

### Combiner les filtres (FilterExpression)

L’API attend une chaîne qui référence les filtres par **index 1-based** et les combine avec `and` / `or`, par ex. `"1 and 2"` ou `"(1 and 2) or 3"`.

- **FilterExpression.Ref(0)** = premier filtre (index 0 en C#, numéroté 1 dans la chaîne).
- **FilterExpression.And(left, right)** et **Or(left, right)** combinent deux sous-expressions.
- **FilterExpression.Build(filters, expr)** vérifie que tous les indices utilisés existent dans la liste des filtres, puis retourne la chaîne finale.

Exemple : deux filtres, on veut « filtre 1 ET filtre 2 » → `FilterExpression.Build(filters, FilterExpression.And(FilterExpression.Ref(0), FilterExpression.Ref(1)))` → `"1 and 2"`.

---

## Exemple complet : construire une Query

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

## Pour aller plus loin (non détaillé dans la démo)

- **RafraichirToken(refreshToken)** : obtenir un nouveau token à partir du refresh token (usage commenté dans `Program.cs`).
- **RevokerToken(accessToken)** : révoquer un token (usage commenté dans `Program.cs`).
- Changer de **société ou d’entité** : utiliser un token obtenu avec d’autres identifiants ou un autre utilisateur.

---

## Dépannage rapide

| Problème | Piste de solution |
|----------|-------------------|
| « Erreur lors de l'authentification » | Vérifier IdClient, SecretClient, Utilisateur dans `appsettings.json`. Vérifier que l’utilisateur Web Services est actif dans Intacct. |
| `appsettings.json` introuvable | Le fichier doit être à la racine du projet (même dossier que le `.csproj`). Il est copié en sortie grâce au `.csproj`. |
| Query retourne vide ou erreur | Vérifier le nom de l’object (ex. `accounts-payable/bill`) et les noms des champs. Consulter la doc API Intacct pour les objets et champs disponibles. |
| Export échoue | S’assurer que la QueryRequest est valide (même critères qu’une Query). Vérifier que le format demandé (Pdf, Csv, etc.) est supporté pour cet object. |

---

## Licence

Utilisation libre pour l’équipe et les ateliers. Les conditions d’utilisation de l’API Sage Intacct s’appliquent à l’API elle-même.
