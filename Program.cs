using intacct_rest_api.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Globalization;

//var culture = CultureInfo.GetCultureInfo("en-US");
//Thread.CurrentThread.CurrentCulture = culture;
//Thread.CurrentThread.CurrentUICulture = culture;


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
            Console.WriteLine("  billNumber : " + premier.GetValueOrDefault("billNumber") + ", vendor : " + premier.GetValueOrDefault("vendor") + ", totalTxnAmount : " + premier.GetValueOrDefault("totalTxnAmount"));
        }
    }
}

// ========== 5. Exportation ==========
var fileType = ExportFileType.Pdf;
var reponseExport = await intacctService.Export(queryRequest, fileType, token.AccessToken);

if (reponseExport.IsSuccessful && reponseExport.RawBytes?.Length > 0)
{
    var nomFichier = $"{queryRequest.Object.Replace("/", "-")}-export-{DateTime.Now:ddMMyyyy-HHmmss}.{fileType.ToString().ToLowerInvariant()}";
    var cheminComplet = Path.Combine("C:\\temp", nomFichier);
    File.WriteAllBytes(cheminComplet, reponseExport.RawBytes);
    Console.WriteLine("\nFichier enregistré : " + cheminComplet);
}

Console.ReadLine();
