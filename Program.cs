using intacct_rest_api.Models;
using intacct_rest_api.Models.Batch;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

// Leçon 4 : Batch focus (auth + batch).
var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
var urlBase = "https://api.intacct.com/ia/api/v1/";
var idClient = config["IdClient"];
var secretClient = config["SecretClient"];
var utilisateur = config["Utilisateur"];

var intacctService = new IntacctService(urlBase, idClient!, secretClient!, utilisateur!);

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

Console.WriteLine("\nChoisissez le scénario à exécuter :");
Console.WriteLine("1 - Batch mode (GET/POST/PATCH/DELETE)");
Console.Write("\nVotre choix (1) : ");
var choix = Console.ReadLine();

if (choix == "1")
{
    await RunBatchAsync(intacctService, token);
}
else
{
    Console.WriteLine("\nChoix non reconnu, aucun scénario exécuté.");
}

Console.WriteLine("\nTerminé. Appuyez sur Entrée pour fermer.");
Console.ReadLine();

static async Task RunBatchAsync(IntacctService intacctService, Token token)
{
    var objectPath = "objects/accounts-payable/vendor";
    var suffix = DateTime.UtcNow.ToString("MMddHHmmss");
    var newVendors = new List<object>
    {
        new Dictionary<string, object> { ["id"] = $"batchv1-{suffix}", ["name"] = $"Batch Vendor 1 {suffix}" },
        new Dictionary<string, object> { ["id"] = $"batchv2-{suffix}", ["name"] = $"Batch Vendor 2 {suffix}" },
        new Dictionary<string, object> { ["id"] = $"batchv3-{suffix}", ["name"] = $"Batch Vendor 3 {suffix}" }
    };

    Console.WriteLine("\n[BATCH] 1) POST create (3 vendors)");
    var createRes = await intacctService.BatchCreate(objectPath, newVendors, token.access_token);
    Console.WriteLine("HTTP status create : " + (int)createRes.StatusCode);
    if (!createRes.IsSuccessful || string.IsNullOrWhiteSpace(createRes.Content))
    {
        Console.WriteLine("Batch create échec : " + createRes.Content);
        return;
    }

    var created = JsonConvert.DeserializeObject<BatchResponse>(createRes.Content!);
    PrintBatchSummary(created);
    var keys = created!.Result.Where(x => !string.IsNullOrWhiteSpace(x.key)).Select(x => x.key!).ToList();
    if (keys.Count == 0)
    {
        Console.WriteLine("Aucune key retournée, suite de la démo annulée.");
        return;
    }
    Console.WriteLine("Keys créées : " + string.Join(", ", keys));

    Console.WriteLine("\n[BATCH] 2) GET by keys");
    var getRes = await intacctService.BatchGetByKeys(objectPath, keys, token.access_token);
    Console.WriteLine("HTTP status get : " + (int)getRes.StatusCode);
    Console.WriteLine("Longueur payload : " + (getRes.Content?.Length ?? 0));

    Console.WriteLine("\n[BATCH] 3) PATCH non-atomic (mise à jour des noms)");
    var patchItems = keys.Select((k, i) =>
    {
        var item = new BatchPatchItem { key = k };
        item["name"] = $"Batch Vendor {i + 1} UPDATED {suffix}";
        return item;
    }).ToList();
    var patchRes = await intacctService.BatchUpdate(objectPath, patchItems, token.access_token, atomic: false);
    Console.WriteLine("HTTP status patch non-atomic : " + (int)patchRes.StatusCode);
    if (!string.IsNullOrWhiteSpace(patchRes.Content))
        PrintBatchSummary(JsonConvert.DeserializeObject<BatchResponse>(patchRes.Content!));

    Console.WriteLine("\n[BATCH] 4) PATCH atomic (1 key invalide pour illustrer l'échec transactionnel)");
    var invalidPatchItem = new BatchPatchItem { key = "999999999" };
    invalidPatchItem["name"] = "Invalid key to force atomic error";
    var atomicPatch = new List<BatchPatchItem>(patchItems) { invalidPatchItem };

    var atomicRes = await intacctService.BatchUpdate(objectPath, atomicPatch, token.access_token, atomic: true);
    Console.WriteLine("HTTP status patch atomic : " + (int)atomicRes.StatusCode);
    if (!string.IsNullOrWhiteSpace(atomicRes.Content))
        PrintBatchSummary(JsonConvert.DeserializeObject<BatchResponse>(atomicRes.Content!));

    Console.WriteLine("\n[BATCH] 5) DELETE by keys (non-atomic)");
    var deleteRes = await intacctService.BatchDeleteByKeys(objectPath, keys, token.access_token, atomic: false);
    Console.WriteLine("HTTP status delete : " + (int)deleteRes.StatusCode);
    if (!string.IsNullOrWhiteSpace(deleteRes.Content))
        PrintBatchSummary(JsonConvert.DeserializeObject<BatchResponse>(deleteRes.Content!));
    else
        Console.WriteLine("DELETE batch : payload vide (ex. 204 No Content).");
}

static void PrintBatchSummary(BatchResponse? response)
{
    if (response == null)
    {
        Console.WriteLine("Réponse batch non désérialisable.");
        return;
    }

    Console.WriteLine($"meta => totalCount={response.Meta.totalCount}, totalSuccess={response.Meta.totalSuccess}, totalError={response.Meta.totalError}");
    foreach (var (item, idx) in response.Result.Select((x, i) => (x, i + 1)))
    {
        var status = item.Status?.ToString() ?? "n/a";
        var key = string.IsNullOrWhiteSpace(item.key) ? "-" : item.key;
        var id = string.IsNullOrWhiteSpace(item.id) ? "-" : item.id;
        var message = item.Error?.message ?? "";
        Console.WriteLine($"  [{idx}] status={status}, key={key}, id={id}" + (string.IsNullOrWhiteSpace(message) ? "" : $", error={message}"));
    }
}
