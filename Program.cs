using intacct_rest_api.Models;
using intacct_rest_api.Models.Export;
using intacct_rest_api.Models.Invoice;
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
Console.WriteLine("5 - Tous les scénarios");
Console.Write("\nVotre choix (1/2/3/4/5) : ");
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
        await RunQueryAndExportAsync(intacctService, token);
        await RunGetInvoicesAsync(intacctService, token);
        await RunInvoiceDetailAfterListAsync(intacctService, token);
        await RunInvoiceCreateAsync(intacctService, token);
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
}

static async Task RunGetInvoicesAsync(IntacctService intacctService, Token token)
{
    // ========== 6. GET factures (liste) ==========
    var reponseInvoices = await intacctService.GetInvoices(token.AccessToken);
    Console.WriteLine("\nGET invoices (liste) - Succès : " + reponseInvoices.IsSuccessful);

    if (reponseInvoices.IsSuccessful && !string.IsNullOrWhiteSpace(reponseInvoices.Content))
    {
        var invoiceList = JsonConvert.DeserializeObject<InvoiceReferenceListResponse>(reponseInvoices.Content);
        if (invoiceList != null)
        {
            var premiersInvoices = invoiceList.Result.Take(3).ToList();
            foreach (var inv in premiersInvoices)
            {
                Console.WriteLine($"  Invoice key={inv.Key}, id={inv.Id}, href={inv.Href}");
            }
        }
    }
}

static async Task RunGetInvoiceDetailAsync(IntacctService intacctService, Token token)
{
    // Pour la démo, on demande la key à l'utilisateur.
    Console.Write("\nSaisissez la key d'une facture (ou laissez vide pour annuler) : ");
    var key = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(key))
    {
        Console.WriteLine("Aucune key saisie, scénario annulé.");
        return;
    }

    Console.WriteLine($"\nRécupération du détail de la facture avec key={key} ...");

    var reponseInvoice = await intacctService.GetInvoiceByKey(key, token.AccessToken);
    Console.WriteLine("GET invoice (détail) - Succès : " + reponseInvoice.IsSuccessful);

    if (reponseInvoice.IsSuccessful && !string.IsNullOrWhiteSpace(reponseInvoice.Content))
    {
        var invoiceDetail = JsonConvert.DeserializeObject<InvoiceDetailResponse>(reponseInvoice.Content);
        if (invoiceDetail?.Invoice != null)
        {
            var h = invoiceDetail.Invoice;
            Console.WriteLine($"\nFacture {h.InvoiceNumber} pour le client {h.Customer.Name}");
            Console.WriteLine($"  Date facture : {h.InvoiceDate}, Date d'échéance : {h.DueDate}");
            Console.WriteLine($"  Montant total (base) : {h.TotalBaseAmount} {h.Currency.BaseCurrency}, Montant total (txn) : {h.TotalTxnAmount} {h.Currency.TxnCurrency}");

            if (h.Lines.Count > 0)
            {
                var l = h.Lines[0];
                Console.WriteLine("\nPremière ligne :");
                Console.WriteLine($"  Compte : {l.GlAccount.Id} - {l.GlAccount.Name}");
                Console.WriteLine($"  Montant (base) : {l.BaseAmount}, Montant (txn) : {l.TxnAmount}");
                Console.WriteLine($"  Lieu : {l.Dimensions.Location.Id} - {l.Dimensions.Location.Name}");
            }
        }
    }
}

static async Task RunInvoiceDetailAfterListAsync(IntacctService intacctService, Token token)
{
    // Variante utilisée pour le scénario "5 - Tous les scénarios" :
    // on récupère d'abord la liste puis on prend la première key.
    var reponseInvoices = await intacctService.GetInvoices(token.AccessToken);
    if (!reponseInvoices.IsSuccessful || string.IsNullOrWhiteSpace(reponseInvoices.Content))
    {
        Console.WriteLine("\nImpossible de récupérer la liste des factures pour le détail.");
        return;
    }

    var invoiceList = JsonConvert.DeserializeObject<InvoiceReferenceListResponse>(reponseInvoices.Content);
    if (invoiceList == null || invoiceList.Result.Count == 0)
    {
        Console.WriteLine("\nAucune facture trouvée pour le détail.");
        return;
    }

    var firstKey = invoiceList.Result[0].Key;
    Console.WriteLine($"\nRécupération du détail de la première facture (key={firstKey}) ...");

    var reponseInvoice = await intacctService.GetInvoiceByKey(firstKey, token.AccessToken);
    Console.WriteLine("GET invoice (détail) - Succès : " + reponseInvoice.IsSuccessful);

    if (reponseInvoice.IsSuccessful && !string.IsNullOrWhiteSpace(reponseInvoice.Content))
    {
        var invoiceDetail = JsonConvert.DeserializeObject<InvoiceDetailResponse>(reponseInvoice.Content);
        if (invoiceDetail?.Invoice != null)
        {
            var h = invoiceDetail.Invoice;
            Console.WriteLine($"\nFacture {h.InvoiceNumber} pour le client {h.Customer.Name}");
            Console.WriteLine($"  Date facture : {h.InvoiceDate}, Date d'échéance : {h.DueDate}");
            Console.WriteLine($"  Montant total (base) : {h.TotalBaseAmount} {h.Currency.BaseCurrency}, Montant total (txn) : {h.TotalTxnAmount} {h.Currency.TxnCurrency}");

            if (h.Lines.Count > 0)
            {
                var l = h.Lines[0];
                Console.WriteLine("\nPremière ligne :");
                Console.WriteLine($"  Compte : {l.GlAccount.Id} - {l.GlAccount.Name}");
                Console.WriteLine($"  Montant (base) : {l.BaseAmount}, Montant (txn) : {l.TxnAmount}");
                Console.WriteLine($"  Lieu : {l.Dimensions.Location.Id} - {l.Dimensions.Location.Name}");
            }
        }
    }
}

static async Task RunInvoiceCreateAsync(IntacctService intacctService, Token token)
{
    // POST facture : modèle minimal (customer, dates, lignes avec txnAmount, glAccount, dimensions.customer)
    var createRequest = new InvoiceCreateRequest
    {
        Customer = new InvoiceCreateCustomerRef { Id = "CL0170" },
        InvoiceDate = "2025-12-06",
        DueDate = "2025-12-31",
        Lines =
        [
            new InvoiceCreateLine
            {
                TxnAmount = "100",
                GlAccount = new InvoiceCreateLineGlAccount { Id = "701000" },
                Dimensions = new InvoiceCreateLineDimensions
                {
                    Customer = new InvoiceCreateLineDimensionCustomer { Id = "CL0170" }
                }
            }
        ]
    };

    var reponse = await intacctService.CreateInvoice(createRequest, token.AccessToken);
    Console.WriteLine("\nPOST invoice - Succès : " + reponse.IsSuccessful);
    if (!reponse.IsSuccessful && !string.IsNullOrWhiteSpace(reponse.Content))
        Console.WriteLine("Réponse : " + reponse.Content);
    if (reponse.Headers?.FirstOrDefault(h => h.Name?.Equals("Location", StringComparison.OrdinalIgnoreCase) == true)?.Value is { } location)
        Console.WriteLine("Location : " + location);
}
