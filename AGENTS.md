# AGENTS.md

## Cursor Cloud specific instructions

### Project overview

This is a .NET 8 console application that demonstrates calling the Sage Intacct REST API (OAuth2 auth, Query, Export, CRUD, Batch, Bulk, Composite). See `README.md` for full documentation (in French).

### Build and run

- **Build:** `dotnet build`
- **Run:** `dotnet run`
- **Restore packages:** `dotnet restore`

There are no automated tests, no linter configuration, and no CI/CD pipeline in this repository.

### Credentials (`appsettings.json`)

The app requires an `appsettings.json` at the project root (gitignored) with three keys:

```json
{
  "IdClient": "<sage-intacct-client-id>",
  "SecretClient": "<sage-intacct-client-secret>",
  "Utilisateur": "<webservices-user@company>"
}
```

Without valid Sage Intacct API credentials, the app will start but immediately fail at the authentication step with `invalid_grant`. This is expected behavior for dummy/placeholder credentials.

If the secrets `INTACCT_CLIENT_ID`, `INTACCT_CLIENT_SECRET`, and `INTACCT_USER` are set as environment variables, you can generate `appsettings.json` from them:

```bash
cat > appsettings.json <<EOF
{
  "IdClient": "$INTACCT_CLIENT_ID",
  "SecretClient": "$INTACCT_CLIENT_SECRET",
  "Utilisateur": "$INTACCT_USER"
}
EOF
```

### Interactive console

The application is an interactive console app that reads from stdin (`Console.ReadLine()`). When running non-interactively, pipe input or use `echo "1" | dotnet run` to select a menu option. Piping empty input (`echo "" | dotnet run`) will cause it to exit after the auth step.

### Export path caveat

The Export feature (menu option 1) writes files to `C:\temp` (hardcoded Windows path in `Program.cs`). On Linux, this path will fail. To test exports on Linux, that path would need to be changed to e.g. `/tmp`.
