using intacct_rest_api.Models;
using intacct_rest_api.Models.Bulk;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

// Leçon 5 : Bulk focus (auth + bulk create + bulk status/download).
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
Console.WriteLine("1 - Bulk create (vendors)");
Console.WriteLine("2 - Bulk get result (statut + download)");
Console.Write("\nVotre choix (1/2) : ");
var choix = Console.ReadLine();

switch (choix)
{
    case "1":
        await RunBulkAsync(intacctService, token);
        break;
    case "2":
        Console.Write("JobId (ex. copié après option 1) : ");
        var jobId = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(jobId))
            await RunBulkGetResultAsync(intacctService, token, jobId);
        else
            Console.WriteLine("JobId vide, annulé.");
        break;
    default:
        Console.WriteLine("\nChoix non reconnu, aucun scénario exécuté.");
        break;
}

Console.WriteLine("\nTerminé. Appuyez sur Entrée pour fermer.");
Console.ReadLine();

static async Task RunBulkAsync(IntacctService intacctService, Token token)
{
    var request = new BulkCreateRequest
    {
        objectName = "accounts-payable/vendor",
        operation = "create",
        jobFile = "file",
        fileContentType = "json"
    };

    var jsonBody = """
        [
            {"id":"vendor1","name":"Corner Library"},
            {"id":"vendor2","name":"Just Picked"},
            {"id":"vendor3","name":"Paper Goods"},
            {"id":"vendor4","name":"Office Furnishings"},
            {"id":"vendor5","name":"Gadget Pro"},
            {"id":"vendor6","name":"Tech Solutions"},
            {"id":"vendor7","name":"Home Essentials"},
            {"id":"vendor8","name":"Garden Supplies"},
            {"id":"vendor9","name":"Auto Parts Co."},
            {"id":"vendor10","name":"Fashion Hub"}
        ]
        """;

    var createRes = await intacctService.BulkCreate(request, jsonBody, token.access_token);
    if (!createRes.IsSuccessful)
    {
        Console.WriteLine("Bulk create échec : " + createRes.Content);
        return;
    }

    var createData = JsonConvert.DeserializeObject<BulkCreateResponse>(createRes.Content!);
    var jobId = createData!.Result.jobId;
    Console.WriteLine("Bulk envoyé. jobId : " + jobId);
    Console.WriteLine("Pour vérifier le statut et télécharger le résultat : option 2 avec ce jobId.");
}

static async Task RunBulkGetResultAsync(IntacctService intacctService, Token token, string jobId)
{
    var statusRes = await intacctService.BulkStatus(jobId, token.access_token, download: false);
    if (!statusRes.IsSuccessful)
    {
        Console.WriteLine("Bulk status échec : " + statusRes.Content);
        return;
    }
    var statusData = JsonConvert.DeserializeObject<BulkStatusResponse>(statusRes.Content!);
    Console.WriteLine("Statut : " + statusData!.Result.status + ", percentComplete : " + (statusData.Result.percentComplete?.ToString() ?? "-"));

    var downloadRes = await intacctService.BulkStatus(jobId, token.access_token, download: true);
    if (!downloadRes.IsSuccessful)
    {
        Console.WriteLine("Bulk download échec : " + downloadRes.Content);
        return;
    }
    var content = downloadRes.Content ?? "";
    try
    {
        var parsed = JsonConvert.DeserializeObject(content);
        content = JsonConvert.SerializeObject(parsed, Formatting.Indented);
    }
    catch
    {
    }
    Console.WriteLine("Résultat (download) :\n" + content);
}
