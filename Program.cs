using intacct_rest_api.Models;
using intacct_rest_api.Models.Export;
using intacct_rest_api.Models.InvoiceCreate;
using intacct_rest_api.Models.BillLineUpdate;
using intacct_rest_api.Models.InvoiceLineUpdate;
using intacct_rest_api.Models.InvoiceUpdate;
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
Console.WriteLine("Token d'accès : " + token.AccessToken.Substring(0, 40) + "...");
Console.WriteLine("Date d'expiration : " + token.DateExpiration);
Console.WriteLine("Est expiré ? : " + token.EstExpire);

// Plus tard : rafraîchir / révoquer
// var reponseRafraichir = await intacctService.RafraichirToken(token.RefreshToken);
// var revokeOk = await intacctService.RevokerToken(token.AccessToken);

// ========== Menu de démo ==========
Console.WriteLine("\nChoisissez le scénario à exécuter :");
Console.WriteLine("1 - Query + Export (bill)");
Console.WriteLine("2 - GET factures (liste)");
Console.WriteLine("3 - GET facture (détail)");
Console.WriteLine("4 - POST facture (création)");
Console.WriteLine("5 - PATCH facture (mise à jour)");
Console.WriteLine("6 - PATCH ligne de bill (mise à jour)");
Console.WriteLine("7 - PATCH ligne de facture (mise à jour)");
Console.WriteLine("8 - Tous les scénarios");
Console.Write("\nVotre choix (1/2/3/4/5/6/7/8) : ");
var choix = Console.ReadLine();

switch (choix)
{
    case "1":
        await RunQueryAndExportAsync(intacctService, token);
        break;
    case "2":
        await RunGetInvoicesAsync(intacctService, token);
        break;
    case "3":
        await RunGetInvoiceDetailAsync(intacctService, token);
        break;
    case "4":
        await RunInvoiceCreateAsync(intacctService, token);
        break;
    case "5":
        await RunInvoiceUpdateAsync(intacctService, token);
        break;
    case "6":
        await RunBillLineUpdateAsync(intacctService, token);
        break;
    case "7":
        await RunInvoiceLineUpdateAsync(intacctService, token);
        break;
    case "8":
        await RunQueryAndExportAsync(intacctService, token);
        await RunGetInvoicesAsync(intacctService, token);
        await RunInvoiceDetailAfterListAsync(intacctService, token);
        await RunInvoiceCreateAsync(intacctService, token);
        await RunInvoiceUpdateAsync(intacctService, token);
        await RunBillLineUpdateAsync(intacctService, token);
        await RunInvoiceLineUpdateAsync(intacctService, token);
        break;
    default:
        Console.WriteLine("\nChoix non reconnu, aucun scénario exécuté.");
        break;
}

Console.WriteLine("\nTerminé. Appuyez sur Entrée pour fermer.");
Console.ReadLine();

// === Méthodes de démo ===

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

    var reponseQuery = await intacctService.Query(queryRequest, token.AccessToken);
    Console.WriteLine("\nRequête - Succès : " + reponseQuery.IsSuccessful);

    // Désérialiser + afficher
    var queryResponse = JsonConvert.DeserializeObject<QueryResponse>(reponseQuery.Content!);
    Console.WriteLine("Résultats : " + queryResponse!.Result.Count);
    if (queryResponse.Result.Count > 0)
        Console.WriteLine("Premier : " + string.Join(", ", queryResponse.Result[0].Select(kv => kv.Key + "=" + kv.Value)));

    // Export
    var fileType = ExportFileType.Pdf;
    var reponseExport = await intacctService.Export(queryRequest, fileType, token.AccessToken);
    var nomFichier = $"{queryRequest.Object.Replace("/", "-")}-export-{DateTime.Now:ddMMyyyy-HHmmss}.pdf";
    File.WriteAllBytes(Path.Combine("C:\\temp", nomFichier), reponseExport.RawBytes!);
    Console.WriteLine("Fichier : C:\\temp\\" + nomFichier);
}

static async Task RunGetInvoicesAsync(IntacctService intacctService, Token token)
{
    var reponse = await intacctService.GetInvoices(token.AccessToken);
    var list = JsonConvert.DeserializeObject<InvoiceReferenceListResponse>(reponse.Content!);
    foreach (var inv in list!.Result.Take(3))
        Console.WriteLine($"key={inv.Key}, id={inv.Id}");
}

static async Task RunGetInvoiceDetailAsync(IntacctService intacctService, Token token)
{
    var key = "11"; // key facture démo
    var reponse = await intacctService.GetInvoiceByKey(key, token.AccessToken);
    var detail = JsonConvert.DeserializeObject<InvoiceDetailResponse>(reponse.Content!);
    var h = detail!.Invoice;
    Console.WriteLine($"Facture {h.InvoiceNumber}, client {h.Customer.Name}, total {h.TotalTxnAmount}");
    var l = h.Lines[0];
    Console.WriteLine($"Ligne 1 : {l.GlAccount.Id}, {l.TxnAmount}, lieu {l.Dimensions.Location.Id}");
}

static async Task RunInvoiceDetailAfterListAsync(IntacctService intacctService, Token token)
{
    var list = JsonConvert.DeserializeObject<InvoiceReferenceListResponse>((await intacctService.GetInvoices(token.AccessToken)).Content!);
    var key = list!.Result[0].Key;
    var detail = JsonConvert.DeserializeObject<InvoiceDetailResponse>((await intacctService.GetInvoiceByKey(key, token.AccessToken)).Content!);
    var h = detail!.Invoice;
    Console.WriteLine($"Facture {h.InvoiceNumber}, total {h.TotalTxnAmount}; ligne 1 key={h.Lines[0].Key}");
}

static async Task RunInvoiceCreateAsync(IntacctService intacctService, Token token)
{
    // POST facture : on assigne explicitement .Id (Customer.Id, GlAccount.Id, Dimensions.Customer.Id, Dimensions.Location.Id).
    var createRequest = new InvoiceCreate
    {
        customer = { id = "CL0170" },
        invoiceDate = "2025-12-06",
        dueDate = "2025-12-31",
        lines =
        [
            new Line
            {
                txnAmount = "100",
                glAccount = { id = "701000" },
                dimensions =
                {
                    customer = new IdRef { id = "CL0170" },
                    location = new IdRef { id = "DEMO_1" }
                }
            }
        ]
    };

    Console.WriteLine("Json => \n"+ JsonConvert.SerializeObject(createRequest, Formatting.Indented));

    var reponse = await intacctService.CreateInvoice(createRequest, token.AccessToken);
    Console.WriteLine("POST invoice - Succès : " + reponse.IsSuccessful);
}

static async Task RunInvoiceUpdateAsync(IntacctService intacctService, Token token)
{
    var key = "11";
    var updateRequest = new InvoiceUpdate
    {
        referenceNumber = "PO-UPDATED-99",
        description = "Modifié par Atelier",
        dueDate = "2026-01-15",
    };

    var reponse = await intacctService.UpdateInvoice(updateRequest, key, token.AccessToken);
    
    Console.WriteLine("PATCH invoice - Succès : " + reponse.IsSuccessful);
}

static async Task RunBillLineUpdateAsync(IntacctService intacctService, Token token)
{
    var lineKey = "3"; // key ligne bill démo
    var updateRequest = new BillLineUpdate
    {
        TxnAmount = "150.00",
        Memo = "Démo bill line",
        Dimensions = new BillLineDimensions
        {
            Department = new KeyIdRef { Id = "922" },
            Location = new KeyIdRef { Id = "DEMO_1" }
        }
    };

    var reponse = await intacctService.UpdateBillLine(updateRequest, lineKey, token.AccessToken);
    
    Console.WriteLine("PATCH bill-line - Succès : " + reponse.IsSuccessful);
}

static async Task RunInvoiceLineUpdateAsync(IntacctService intacctService, Token token)
{
    var lineKey = "11"; // key ligne facture démo
    var updateRequest = new InvoiceLineUpdate
    {
        txnAmount = "150.00",
        memo = "Démo invoice line"
    };

    var reponse = await intacctService.UpdateInvoiceLine(updateRequest, lineKey, token.AccessToken);
    
    Console.WriteLine("PATCH invoice-line - Succès : " + reponse.IsSuccessful);
}
