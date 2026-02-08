using intacct_rest_api.Models;
using intacct_rest_api.Models.Export;
using intacct_rest_api.Models.Query;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
Console.WriteLine("Token d'accès : " + token.access_token.Substring(0, 40) + "...");
Console.WriteLine("Date d'expiration : " + token.DateExpiration);
Console.WriteLine("Est expiré ? : " + token.EstExpire);

// Plus tard : rafraîchir / révoquer
// var reponseRafraichir = await intacctService.RafraichirToken(token.refresh_token);
// var revokeOk = await intacctService.RevokerToken(token.access_token);

// ========== Menu de démo (Query + Export uniquement) ==========
Console.WriteLine("\nChoisissez le scénario à exécuter :");
Console.WriteLine("1 - Query + Export (bill)");
Console.WriteLine("9 - Idem (tous les scénarios de cette leçon)");
Console.Write("\nVotre choix (1/9) : ");
var choix = Console.ReadLine();

switch (choix)
{
    case "1":
    case "9":
        await RunQueryAndExportAsync(intacctService, token);
        break;
    default:
        Console.WriteLine("\nChoix non reconnu, aucun scénario exécuté.");
        break;
}

Console.WriteLine("\nTerminé. Appuyez sur Entrée pour fermer.");
Console.ReadLine();

// === Méthodes de démo (Query + Export uniquement) ===

static async Task RunQueryAndExportAsync(IntacctService intacctService, Token token)
{
    // ========== 3. Requête Query ==========
    var queryObject = "accounts-payable/bill";
    var queryFields = new List<string> { "id", "billNumber", "vendor.id", "vendor.name", "postingDate", "totalTxnAmount", "entity.id" };

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

    var reponseQuery = await intacctService.Query(queryRequest, token.access_token);
    Console.WriteLine("\nRequête - Succès : " + reponseQuery.IsSuccessful);

    // Désérialiser + afficher
    var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(reponseQuery.Content!);
    Console.WriteLine("Résultats : " + queryResponse!.Result.Count);
    if (queryResponse.Result.Count > 0)
        Console.WriteLine("Premier : " + string.Join(", ", queryResponse.Result[0].Select(kv => kv.Key + "=" + kv.Value)));

    // Export
    var fileType = ExportFileType.Pdf;
    var reponseExport = await intacctService.Export(queryRequest, fileType, token.access_token);
    var nomFichier = $"{queryRequest.Object.Replace("/", "-")}-export-{DateTime.Now:ddMMyyyy-HHmmss}.pdf";
    File.WriteAllBytes(Path.Combine("C:\\temp", nomFichier), reponseExport.RawBytes!);
    Console.WriteLine("Fichier : C:\\temp\\" + nomFichier);
}
