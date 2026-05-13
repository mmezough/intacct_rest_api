using intacct_rest_api.Models;
using intacct_rest_api.Models.Query;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

// ========== 1. Configuration ==========
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var intacctService = new IntacctService(
    "https://api.intacct.com/ia/api/v1/",
    config["IdClient"]!,
    config["SecretClient"]!,
    config["Utilisateur"]!);

// ========== 2. Authentification (OAuth2 Client Credentials) ==========
var reponseAuth = await intacctService.ObtenirToken();
if (!reponseAuth.IsSuccessful)
{
    Console.WriteLine("Erreur d'authentification : " + reponseAuth.Content);
    return;
}

var token = new Token(reponseAuth);
Console.WriteLine("Token obtenu : " + token.access_token.Substring(0, 40) + "...");
Console.WriteLine("Expire à : " + token.DateExpiration);

// ========== 3. Query ==========
var queryRequest = new QueryRequest
{
    Object = "accounts-payable/bill",
    Fields = new List<string> { "id", "billNumber", "vendor.name", "totalTxnAmount" },
    Size = 10
};

var reponseQuery = await intacctService.Query(queryRequest, token.access_token);
if (!reponseQuery.IsSuccessful)
{
    Console.WriteLine("Erreur Query : " + reponseQuery.Content);
    return;
}

var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(reponseQuery.Content!);
Console.WriteLine($"\n{queryResponse!.Result.Count} résultat(s) sur {queryResponse.Meta.totalCount} au total");

foreach (var ligne in queryResponse.Result)
    Console.WriteLine(string.Join(", ", ligne.Select(kv => $"{kv.Key}={kv.Value}")));

Console.WriteLine("\nTerminé. Appuyez sur Entrée pour fermer.");
Console.ReadLine();
