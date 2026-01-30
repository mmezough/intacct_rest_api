using intacct_rest_api.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Net.Http.Headers;

// --- Configuration ---
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var urlBase = "https://api.intacct.com/ia/api/v1/";
var idClient = config["IdClient"];
var secretClient = config["SecretClient"];
var utilisateur = config["Utilisateur"];

var intacctService = new IntacctService(urlBase, idClient!, secretClient!, utilisateur!);

// --- 1. Auth : obtenir le token ---
var reponseAuth = await intacctService.ObtenirToken();
if (!reponseAuth.IsSuccessful)
{
    Console.WriteLine("Erreur lors de l'authentification : " + reponseAuth.Content);
    return;
}

var token = new Token(reponseAuth);
Console.WriteLine("Token d'accès : " + token.AccessToken.Substring(0, 40) + "...");
Console.WriteLine("Date d'expiration : " + token.DateExpiration);
Console.WriteLine("Est expiré ? : " + token.EstExpire);

// --- Plus tard : rafraîchir / révoquer ---
// var reponseRafraichir = await intacctService.RafraichirToken(token.RefreshToken);
// var revokeOk = await intacctService.RevokerToken(token.AccessToken);

// --- 2. Exemple requête ---
var filtres = new List<Dictionary<string, object>>
{
    Filter.Contains("id", "170"),
    Filter.GreaterThan("totalDue", "0"),
};
var filtreExpression = FilterExpression.Or(FilterExpression.Ref(0), FilterExpression.Ref(1));
var filterString = FilterExpression.Build(filtres, filtreExpression);

var filterParameters = new FilterParameters
{
    CaseSensitiveComparison = false,
    IncludePrivate = false
};
var orderBy = new List<Dictionary<string, string>>
{
    new Dictionary<string, string> { { "name", "asc" } }
};

var queryRequest = new QueryRequest
{
    Object = "accounts-payable/bill",
    Fields = new List<string> { "key", "id", "dueDate", "postingDate", "totalTxnAmount", "totalTxnAmountDue" },
    //Filters = filtres,
    //FilterExpression = filterString,
    FilterParameters = filterParameters,
    //OrderBy = orderBy,
    //Start = 0,
    //Size = 2
};
var reponseQuery = await intacctService.Query(queryRequest, token.AccessToken);
Console.WriteLine("\nRequête - Succès : " + reponseQuery.IsSuccessful);
if (reponseQuery.IsSuccessful)
    Console.WriteLine("Requête - Contenu : " + (reponseQuery.Content ?? "").Substring(0, Math.Min(200, reponseQuery.Content?.Length ?? 0)) + "...");

// --- 3. Exemple exportation (PDF) ---
var reponseExport = await intacctService.Export(queryRequest, ExportFileType.Pdf, token.AccessToken);
Console.WriteLine("\nExportation PDF - Succès : " + reponseExport.IsSuccessful);
if (reponseExport.IsSuccessful && reponseExport.RawBytes?.Length > 0)
    Console.WriteLine("Exportation PDF - Taille : " + reponseExport.RawBytes!.Length + " octets");

Console.ReadLine();
