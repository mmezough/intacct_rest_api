# AGENTS.md

## Cursor Cloud specific instructions

### Project overview

Minimal .NET 8 console app that demonstrates calling the Sage Intacct REST API: OAuth2 authentication (Client Credentials) and Query (read data). See `README.md` for setup instructions.

### Structure (5 source files, ~170 lines total)

- `Program.cs` — config, auth, query, display results
- `Services/IntacctService.cs` — `ObtenirToken()` + `Query()`
- `Models/Token.cs` — OAuth2 token model
- `Models/Query/QueryRequest.cs` — Query request body
- `Models/Query/QueryResponse.cs` — Query response (result + meta)

### Build and run

```bash
dotnet restore   # install NuGet packages
dotnet build     # compile
dotnet run       # run (requires appsettings.json with valid credentials)
```

There are no automated tests, no linter, and no CI/CD in this repository.

### Credentials (`appsettings.json`)

The app requires `appsettings.json` at the project root (gitignored):

```json
{
  "IdClient": "<sage-intacct-client-id>",
  "SecretClient": "<sage-intacct-client-secret>",
  "Utilisateur": "<webservices-user@company>"
}
```

Without valid credentials, the app exits with `invalid_grant`. This is expected.

### Non-interactive execution

The app reads from stdin (`Console.ReadLine()` at the end). Pipe empty input to skip the final pause: `echo "" | dotnet run`.
