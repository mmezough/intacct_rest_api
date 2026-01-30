using intacct_rest_api.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

// ========== 1. Configuration ==========
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var urlBase = "https://api.intacct.com/ia/api/v1/";
var idClient = config["IdClient"];
var secretClient = config["SecretClient"];
var utilisateur = config["Utilisateur"];

var intacctService = new IntacctService(urlBase, idClient!, secretClient!, utilisateur!);

// ========== 2. Auth : obtenir le token ==========
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

// Plus tard : rafraîchir / révoquer
// var reponseRafraichir = await intacctService.RafraichirToken(token.RefreshToken);
// var revokeOk = await intacctService.RevokerToken(token.AccessToken);

// ========== 3. Requête Query ==========
var filtres = new List<Dictionary<string, object>>
{
    Filter.Contains("id", "170"),
    Filter.GreaterThan("totalDue", "0"),
};
var filtreExpression = FilterExpression.Or(FilterExpression.Ref(0), FilterExpression.Ref(1));
var filterString = FilterExpression.Build(filtres, filtreExpression);

var queryRequest = new QueryRequest
{
    Object = "accounts-payable/bill",
    Fields = new List<string> { "key", "id", "dueDate", "postingDate", "totalTxnAmount", "totalTxnAmountDue" },
    Filters = filtres,
    FilterExpression = filterString,
    FilterParameters = new FilterParameters { CaseSensitiveComparison = false, IncludePrivate = false },
    OrderBy = new List<Dictionary<string, string>> { new() { ["name"] = "asc" } },
    Start = 1,
    Size = 100
};

var reponseQuery = await intacctService.Query(queryRequest, token.AccessToken);
Console.WriteLine("\nRequête - Succès : " + reponseQuery.IsSuccessful);

// ========== 4. Désérialiser la réponse Query ==========
if (reponseQuery.IsSuccessful && !string.IsNullOrWhiteSpace(reponseQuery.Content))
{
    var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(reponseQuery.Content);
    if (queryResponse != null)
    {
        Console.WriteLine("Résultats : " + queryResponse.Result.Count + " enregistrement(s)");
        Console.WriteLine("Meta - TotalCount : " + queryResponse.Meta.TotalCount + ", Start : " + queryResponse.Meta.Start + ", PageSize : " + queryResponse.Meta.PageSize);
        if (queryResponse.Result.Count > 0)
        {
            var premier = queryResponse.Result[0];
            Console.WriteLine("Premier enregistrement - clés : " + string.Join(", ", premier.Keys));
            Console.WriteLine("  id : " + premier.GetValueOrDefault("id") + ", dueDate : " + premier.GetValueOrDefault("dueDate"));
        }
    }
}

// ========== 5. Exportation (PDF) ==========
var reponseExport = await intacctService.Export(queryRequest, ExportFileType.Pdf, token.AccessToken);
Console.WriteLine("\nExportation PDF - Succès : " + reponseExport.IsSuccessful);
if (reponseExport.IsSuccessful && reponseExport.RawBytes?.Length > 0)
    Console.WriteLine("Exportation PDF - Taille : " + reponseExport.RawBytes!.Length + " octets");

Console.ReadLine();
