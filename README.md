# Sage Intacct REST API – Cours / Atelier

Application console (.NET 8) pour apprendre à appeler l’**API REST Sage Intacct** : authentification OAuth2, **Query** (lecture), **Export** (PDF, CSV, etc.), **GET** (liste / détail de factures), **Bulk** (create asynchrone + statut, callback URL optionnel). Support de cours pour ateliers et onboarding.

---

## Objectifs du cours

À l’issue de ce cours, vous saurez :

1. **Configurer** une application .NET pour appeler l’API Intacct (config, secrets).
2. **Obtenir un token** OAuth2 (client_credentials) et l’utiliser dans les requêtes.
3. **Construire une Query** : object, fields, filtres, expression, tri, pagination.
4. **Désérialiser** la réponse Query (Result + Meta) avec Newtonsoft.
5. **Exporter** le résultat d’une requête en fichier (PDF, CSV, etc.) et l’enregistrer.
6. Utiliser des **endpoints GET** pour lister des factures (références légères) et lire le **détail d’une facture** (en-tête + lignes).

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
| **Program.cs** | Point d’entrée : configuration, auth, Query, Export, GET factures, POST/PATCH facture, PATCH ligne de bill (6), PATCH ligne de facture (7), DELETE facture (8), Tous les scénarios (9), **Bulk create + statut (10)**. |
| **appsettings.json** | Secrets (à créer ; ignoré par git). |
| **Models/Token.cs** | Modèle du token OAuth (AccessToken, RefreshToken, DateExpiration, EstExpire). Désérialisation avec Newtonsoft. |
| **Models/QueryRequest.cs** | Corps d’une requête Query : Object, Fields, Filters, FilterExpression, FilterParameters, OrderBy, Start, Size. Sérialisé par RestSharp (System.Text.Json). |
| **Models/QueryResponse.cs** | Réponse Query : Result (liste de lignes) + Meta (TotalCount, Start, PageSize, Next, Previous). Désérialisation avec Newtonsoft. |
| **Models/Filter.cs** | Helpers pour construire les filtres (Equal, NotEqual, LessThan, GreaterThan, Between, In, Contains, etc.). Les DateTime/DateOnly sont normalisés en `yyyy-MM-dd`. |
| **Models/FilterExpression.cs** | Combinaison de filtres par index : Ref(0), Ref(1), And(left, right), Or(left, right), Build(filters, expr) pour obtenir la chaîne `"1 and 2"` et valider les indices. |
| **Models/FilterParameters.cs** | Paramètres optionnels des filtres : CaseSensitiveComparison, IncludePrivate. |
| **Models/ExportRequest.cs** | Corps interne de l’Export : Query + FileType. |
| **Models/ExportFileType.cs** | Enum des formats d’export : Pdf, Csv, Word, Xml, Xlsx. |
| **Models/InvoiceReference.cs** | Modèles pour la liste de factures : `InvoiceReference` (key, id, href), `InvoiceReferenceListResponse` (Result uniquement). |
| **Models/InvoiceDetail.cs** | Modèles pour le détail d’une facture (en-tête + quelques lignes) : `InvoiceDetailResponse`, `InvoiceHeader`, `InvoiceLine`, etc. |
| **Models/Invoice/InvoiceCreate.cs** | Modèle minimal POST facture : `InvoiceCreate`, `IdRef`, `Line`, `LineDimensions` (customer, glAccount, dimensions.* = objets `{ "id": "..." }`). |
| **Models/Invoice/InvoiceUpdate.cs** | Modèle PATCH facture : `InvoiceUpdate` (referenceNumber, description, dueDate ; camelCase, minimal). |
| **Models/Invoice/BillLineUpdate.cs** | Modèle minimal PATCH ligne de bill : `BillLineUpdate` (glAccount, txnAmount, memo, dimensions) ; refs avec un seul `id`. Dimensions : department, location. |
| **Models/Invoice/InvoiceLineUpdate.cs** | Modèle minimal PATCH ligne de facture : `InvoiceLineUpdate` (glAccount, txnAmount, memo, dimensions) ; réutilise `IdRef` de InvoiceCreate. Dimensions : location, customer. |
| **Services/IntacctService.cs** | Client HTTP (RestSharp) : ObtenirToken, RafraichirToken, RevokerToken, Query, Export, GetInvoices, GetInvoiceByKey, CreateInvoice, UpdateInvoice, UpdateInvoiceLine, UpdateBillLine, DeleteInvoice, **BulkCreate**, **BulkStatus**. Corps PATCH ligne sérialisés avec Newtonsoft + AddStringBody. |

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

## Exemple complet : construire une Query (comme dans Program.cs)

```csharp
var queryObject = "accounts-payable/bill";
var queryFields = new List<string> { "id", "billNumber", "vendor.id", "vendor.name", "postingDate", "totalTxnAmount" };

var queryFilters = new List<Dictionary<string, object>>
{
    Filter.GreaterThan("totalTxnAmount", "100"),
    Filter.Between("postingDate", new DateTime(2025, 1, 1), new DateTime(2025, 1, 31))
};

var queryFilterExpression = FilterExpression.And(FilterExpression.Ref(0), FilterExpression.Ref(1));
var queryFilterExpressionString = FilterExpression.Build(queryFilters, queryFilterExpression);

var queryFilterParam = new FilterParameters { CaseSensitiveComparison = false, IncludePrivate = false };
var querySort = new List<Dictionary<string, string>> { new() { ["totalTxnAmount"] = "desc" } };

var queryRequest = new QueryRequest
{
    Object = queryObject,
    Fields = queryFields,
    Filters = queryFilters,
    FilterExpression = queryFilterExpressionString,
    FilterParameters = queryFilterParam,
    OrderBy = querySort,
    Start = 1,
    Size = 100
};

var reponseQuery = await intacctService.Query(queryRequest, token.AccessToken);

if (reponseQuery.IsSuccessful && !string.IsNullOrWhiteSpace(reponseQuery.Content))
{
    var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(reponseQuery.Content);
    if (queryResponse != null)
    {
        // queryResponse.Result = lignes, queryResponse.Meta = pagination
        Console.WriteLine("Résultats : " + queryResponse.Result.Count + " enregistrement(s)");
        Console.WriteLine("Meta - TotalCount : " + queryResponse.Meta.TotalCount + ", Start : " + queryResponse.Meta.Start + ", PageSize : " + queryResponse.Meta.PageSize);
    }
}
```

---

## GET factures : liste + détail

En plus de Query / Export, le projet montre deux endpoints **GET** sur l’objet facture (`accounts-receivable/invoice`) :

- **GET liste de factures** : `/objects/accounts-receivable/invoice`
- **GET détail d’une facture par key** : `/objects/accounts-receivable/invoice/{key}`

Ces endpoints renvoient un schéma **spécifique à l’objet** (ici : facture). Ils sont pratiques pour :

- Vérifier que le token fonctionne.
- Parcourir rapidement quelques factures existantes.
- Montrer la différence entre un **endpoint GET orienté ressource** et le **service Query** plus générique.

> ⚠️ **Limites** : la liste de factures n’est pas pensée pour de vraies extractions.
>
>- Pas de paramètres de pagination dans l’URL.
>- Champs retournés limités (clé, id, href).
>- Pour filtrer, paginer et choisir les champs retournés, **Sage recommande d’utiliser Query**.

### Modèles côté C#

- `InvoiceReference`, `InvoiceReferenceListResponse` (fichier `Models/InvoiceReference.cs`) : on ne mappe que `ia::result` (liste de références key, id, href).
- `InvoiceDetailResponse`, `InvoiceHeader`, `InvoiceLine`, etc. (fichier `Models/InvoiceDetail.cs`) : on ne mappe que `ia::result` (en-tête + lignes). Pas de meta.
  - En-tête : id, key, invoiceNumber, state, dates, montants, client, devise, et un dictionnaire `CustomFields` pour les champs personnalisés (ex. `nsp::REF_ERP`).
  - Lignes : compte de résultat, compte client, montants, lieu (dimension location), client (dimension customer), etc., plus un dictionnaire `CustomFields` pour les champs personnalisés de ligne.

On utilise **Newtonsoft.Json** (`[JsonProperty]`, `[JsonExtensionData]`) pour mapper exactement les noms de champs retournés par l’API (`"ia::result"`, `"invoiceNumber"`, champs commençant par `"nsp::"`, etc.) et exposer les champs personnalisés dans des dictionnaires clé/valeur.

### Exemple de flux dans `Program.cs`

Après la démo Query + Export, le programme :

1. Appelle **GetInvoices(token.AccessToken)** :
   - Désérialise la réponse dans `InvoiceReferenceListResponse` (Result uniquement).
   - Liste les 3 premières factures (key, id, href).
2. Récupère la **clé** (`key`) de la première facture.
3. Appelle **GetInvoiceByKey(key, token.AccessToken)** :
   - Désérialise la réponse dans `InvoiceDetailResponse`.
   - Affiche un résumé de la facture : numéro, nom du client, dates, montants, devise.
   - Affiche un extrait de la première ligne : compte comptable, montant, lieu.

Ce flux permet de comparer visuellement :

- **Query** : très générique (n’importe quel object / fields), filtre / tri / pagination, export possible.
- **GET facture** : schéma spécialisé, pratique pour lire un enregistrement précis ou en parcourir quelques-uns, mais pas pour les extractions avec critères métier.

---

## Conventions du projet (patterns)

Pour garder le code cohérent et facile à maintenir :

| Règle | Usage |
|-------|--------|
| **Modèles de requête (API)** | **camelCase** (ex. InvoiceUpdate, InvoiceLineUpdate) pour coller au JSON sans attributs, ou PascalCase + `[JsonProperty]` (ex. BillLineUpdate) si besoin de NullValueHandling. |
| **Champs optionnels (PATCH)** | Propriétés **nullable** (`string?`) + **`NullValueHandling = NullValueHandling.Ignore`** pour n’envoyer que les champs renseignés. |
| **Modèles de réponse** | Même principe : PascalCase + `[JsonProperty]` pour mapper `"ia::result"`, noms API, etc. (voir `InvoiceDetail`, `Token`). |
| **Namespaces** | Un dossier = un sous-namespace : `Models.InvoiceCreate`, `Models.InvoiceUpdate`, `Models.InvoiceLineUpdate`, `Models.BillLineUpdate`, `Models.Query`, `Models.Export`. Modèles partagés (réponses liste/détail) dans `Models` sans sous-dossier. |
| **Service** | Une méthode par opération (Get, Create, Update, Query, Export). Commentaires XML décrivant l’endpoint et le corps attendu. |
| **Program.cs** | Un scénario = une méthode `RunXxxAsync`. Messages console explicites (ex. « PATCH invoice - Succès » et non « POST » pour une mise à jour). |

---

## POST facture (création)

Le projet permet de **créer une facture** via **POST** `/objects/accounts-receivable/invoice` avec un **modèle minimal** :

- **En-tête** : `customer` (objet `{ "id": "..." }`), `invoiceDate`, `dueDate`.
- **Lignes** : pour chaque ligne : `txnAmount`, `glAccount` (objet), `dimensions` (customer, location ; optionnels avec `NullValueHandling.Ignore`).

Les modèles sont dans **Models/Invoice/InvoiceCreate.cs** (`InvoiceCreate`, `IdRef`, `Line`, `LineDimensions`). L’API attend des **objets** `{ "id": "..." }` pour customer, glAccount et chaque dimension.

**CreateInvoice(request, accessToken)** envoie un POST avec le corps JSON et l’en-tête `Authorization: Bearer <token>`. En démo (option 4), on construit un exemple avec client CL0170, dates 2025-12-06 / 2025-12-31, une ligne de 100 avec compte 701000 et dimension client CL0170.

---

## PATCH facture (mise à jour)

Le projet permet de **modifier une facture** via **PATCH** `/objects/accounts-receivable/invoice/{key}` avec un **corps partiel** :

- Champs supportés dans **Models/Invoice/InvoiceUpdate.cs** : `referenceNumber`, `description`, `dueDate` (camelCase, minimal).

**UpdateInvoice(request, key, accessToken)** envoie un PATCH avec le corps JSON. En démo (option 5), on met à jour la facture de key exemple « 11 » avec un numéro de référence, une description et une nouvelle date d’échéance.

---

## PATCH ligne de bill (bill-line)

Mise à jour d’une **ligne de bill** (comptes fournisseurs) via **PATCH** `/objects/accounts-payable/bill-line/{key}`. La `key` est celle de la ligne.

- **Modèle** : **Models/Invoice/BillLineUpdate.cs** (`BillLineUpdate`, `GlAccountRef`, `BillLineDimensions`, `KeyIdRef`). Références avec un seul identifiant : **id**.
- **Champs** (tous optionnels) : glAccount (objet `{ "id": "..." }`), txnAmount, memo, dimensions (department, location).
- **Sérialisation** : le corps est sérialisé avec **Newtonsoft** et `NullValueHandling.Ignore`, puis envoyé via `AddStringBody`, pour que seul le JSON des champs renseignés soit envoyé (comme en Postman). RestSharp utilise par défaut System.Text.Json pour `AddJsonBody`, ce qui peut produire un payload différent.

**UpdateBillLine(request, lineKey, accessToken)**. Démo : option **6**.

---

## PATCH ligne de facture (invoice-line)

Mise à jour d’une **ligne de facture** (comptes clients) via **PATCH** `/objects/accounts-receivable/invoice-line/{key}`. La `key` est celle de la ligne.

- **Modèle** : **Models/Invoice/InvoiceLineUpdate.cs** (`InvoiceLineUpdate`, `InvoiceLineGlAccountRef`, `InvoiceLineDimensions`, `InvoiceLineKeyIdRef`). Références avec un seul identifiant : **id**.
- **Champs** (tous optionnels) : glAccount, txnAmount, memo, dimensions (location, customer).
- **Sérialisation** : comme pour bill-line, corps sérialisé avec Newtonsoft + `AddStringBody` pour n’envoyer que les champs renseignés.

**UpdateInvoiceLine(request, lineKey, accessToken)**. Démo : option **7**.

---

## DELETE facture

Suppression d’une facture via **DELETE** `/objects/accounts-receivable/invoice/{key}`.

**DeleteInvoice(key, accessToken)**. Démo : option **8**. (Non inclus dans « Tous les scénarios » car destructif.)

---

## Bulk (create + statut)

Le projet permet d’envoyer une **requête bulk** (traitement asynchrone) puis de **vérifier le statut** jusqu’à completion.

- **BulkCreate(objectName, operation, jobFile, jsonArrayBody, accessToken, callbackUrl = null)** : POST multipart vers `/services/bulk/job/create` (corps `ia::requestBody` + fichier JSON). Retourne un **jobId**. **callbackUrl** est optionnel : si fourni, Intacct appellera cette URL (POST) quand le job sera terminé (realtime, ex. webhook externe).
- **BulkStatus(jobId, accessToken, download = false)** : GET `/services/bulk/job/status?jobId=...` ; avec `download=true` une fois le statut `completed`, retourne le fichier résultat (JSON).

En démo (option **10**), **RunBulkAsync** envoie un bulk **create** sur `accounts-payable/vendor` avec un JSON hardcodé (2 fournisseurs), affiche le jobId, puis **poll** le statut toutes les 2 secondes jusqu’à `completed` ou `failed`, et affiche le résultat du download si terminé avec succès. Le callback URL peut être défini dans `Program.cs` (variable `callbackUrl`) pour recevoir la notification en temps réel sur un serveur externe.

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
