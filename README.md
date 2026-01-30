# Sage Intacct REST API – Demo / Workshop

Console application (.NET 8) demonstrating how to call the **Sage Intacct REST API**: authentication (OAuth2), **Query** (read data), and **Export** (PDF, CSV, etc.). Built for workshops and team onboarding.

---

## Requirements

- **.NET 8 SDK** – [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Sage Intacct** credentials: Client ID, Client Secret, and a web services user

---

## Setup

### 1. Clone the repository

```bash
git clone <repository-url>
cd intacct_rest_api
```

### 2. Add `appsettings.json` (required)

The file `appsettings.json` is **not** in the repository (it contains secrets and is in `.gitignore`). You must create it at the project root.

**Create a file named `appsettings.json`** with:

```json
{
  "IdClient": "your-client-id.app.sage.com",
  "SecretClient": "your-client-secret",
  "Utilisateur": "webservice@your-company"
}
```

Replace with your real values from Sage Intacct (Company > Web Services or your admin).

**Optional:** Add an `appsettings.example.json` with placeholder values and commit it so the team knows which keys to set.

### 3. Restore and run

```bash
dotnet restore
dotnet run
```

The app will: get a token, run a sample Query, deserialize the response, run a sample Export (PDF), and save the file in the executable folder.

---

## Project structure

```
intacct_rest_api/
├── Program.cs                 # Entry point: config, auth, query, export, file save
├── appsettings.json          # Your secrets (create it; not in git)
├── intacct_rest_api.csproj
├── Models/
│   ├── Token.cs              # OAuth token (access, refresh, expiry)
│   ├── QueryRequest.cs       # Query body: object, fields, filters, orderBy, start, size...
│   ├── QueryResponse.cs      # Query response: Result (rows) + Meta (pagination)
│   ├── Filter.cs             # Helpers to build filters (Equal, Between, GreaterThan...)
│   ├── FilterExpression.cs   # Combine filters with And/Or (Ref(0), Ref(1)...)
│   ├── FilterParameters.cs   # caseSensitiveComparison, includePrivate
│   ├── ExportRequest.cs      # Internal: query + fileType for Export
│   └── ExportFileType.cs     # Pdf, Csv, Word, Xml, Xlsx
└── Services/
    └── IntacctService.cs     # HTTP client: ObtenirToken, Query, Export, RafraichirToken, RevokerToken
```

---

## Code overview

### 1. Configuration and auth (`Program.cs`)

- **Configuration** is read from `appsettings.json` (IdClient, SecretClient, Utilisateur).
- **IntacctService** is created with base URL `https://api.intacct.com/ia/api/v1/` and those values.
- **ObtenirToken()** calls the OAuth2 token endpoint (client_credentials). The response is deserialized into **Token** (Newtonsoft) to get `AccessToken`, `RefreshToken`, `DateExpiration`, etc.
- The token is passed explicitly to Query and Export (so you can switch company/entity later by using another token).

### 2. Query (`IntacctService.Query`)

- **QueryRequest** describes the query:
  - **Object** (required): API object, e.g. `"accounts-payable/bill"`.
  - **Fields** (required): list of field names to return.
  - **Filters** (optional): list of filter objects built with **Filter** (Equal, Between, GreaterThan, etc.). Dates (DateTime/DateOnly) are normalized to `yyyy-MM-dd` inside Filter.
  - **FilterExpression** (optional): string that combines filters by index, e.g. `"1 and 2"`. Built with **FilterExpression.Ref(0)**, **And**, **Or**, then **FilterExpression.Build(filters, expr)** so indices are validated.
  - **FilterParameters** (optional): caseSensitiveComparison, includePrivate.
  - **OrderBy** (optional): list of `{ "field": "asc" }` or `"desc"`.
  - **Start**, **Size** (optional): pagination.

- **Query(request, accessToken)** sends a POST to `/service/core/query` with that body and returns the raw response.

### 3. Deserializing the Query response

- The response body has `"ia::result"` (array of rows) and `"ia::meta"` (totalCount, start, pageSize, next, previous).
- Use **Newtonsoft**: `JsonConvert.DeserializeObject<QueryResponse>(response.Content)`.
- **QueryResponse.Result** is `List<Dictionary<string, object>>`: each row is a dictionary (field name → value). **QueryResponse.Meta** holds pagination info.

### 4. Export (`IntacctService.Export`)

- Same **QueryRequest** plus **ExportFileType** (Pdf, Csv, Word, Xml, Xlsx).
- **Export(request, fileType, accessToken)** sends a POST to `/service/core/export` with body `{ "query": request, "fileType": "pdf" }` and returns the file bytes.
- The sample in `Program.cs` saves the file in the executable folder with a name like `object-export-ddMMyyyy-HHmmss.ext`.

### 5. Filter and FilterExpression

- **Filter** methods return a `Dictionary<string, object>` in the shape the API expects (e.g. `{ "$gt": { "totalTxnAmount": "100" } }`). Use them to build **QueryRequest.Filters**.
- **FilterExpression.Ref(0)**, **Ref(1)** refer to the first and second filter in the list. **And(left, right)** and **Or(left, right)** combine them. **Build(filters, expr)** validates indices and returns the string (e.g. `"1 and 2"`) for **QueryRequest.FilterExpression**.

### 6. Token refresh and revoke (for later)

- **RafraichirToken(refreshToken)** and **RevokerToken(accessToken)** are implemented in **IntacctService**; usage is commented in `Program.cs` for when you need refresh/revoke flows.

---

## Example: building a Query

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
// queryResponse.Result = rows, queryResponse.Meta = pagination
```

---

## License

Use as needed for your team and workshops. Sage Intacct API terms apply to the API itself.
