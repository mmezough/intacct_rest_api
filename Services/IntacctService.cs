using intacct_rest_api.Models;
using intacct_rest_api.Models.Export;
using intacct_rest_api.Models.InvoiceCreate;
using intacct_rest_api.Models.BillLineUpdate;
using intacct_rest_api.Models.InvoiceLineUpdate;
using intacct_rest_api.Models.InvoiceUpdate;
using intacct_rest_api.Models.Query;
using Newtonsoft.Json;
using RestSharp;

public class IntacctService
{
    private readonly RestClient _client;
    private readonly string _idClient;
    private readonly string _secretClient;
    private readonly string _utilisateur;

    public IntacctService(string urlBase, string idClient, string secretClient, string utilisateur)
    {
        _client = new RestClient(urlBase);
        _idClient = idClient;
        _secretClient = secretClient;
        _utilisateur = utilisateur;
    }

    public async Task<RestResponse> ObtenirToken()
    {
        var restRequest = new RestRequest("oauth2/token", Method.Post);

        restRequest.AddParameter("grant_type", "client_credentials");
        restRequest.AddParameter("client_id", _idClient);
        restRequest.AddParameter("client_secret", _secretClient);
        restRequest.AddParameter("username", _utilisateur);

        var reponse = await _client.ExecuteAsync(restRequest);
        return reponse;
    }

    public async Task<RestResponse> RafraichirToken(string refreshToken)
    {
        var restRequest = new RestRequest("oauth2/token", Method.Post);

        restRequest.AddParameter("grant_type", "refresh_token");
        restRequest.AddParameter("client_id", _idClient);
        restRequest.AddParameter("client_secret", _secretClient);
        restRequest.AddParameter("refresh_token", refreshToken);

        var reponse = await _client.ExecuteAsync(restRequest);
        return reponse;
    }

    public async Task<bool> RevokerToken(string token)
    {
        var restRequest = new RestRequest("oauth2/revoke", Method.Post);

        restRequest.AddParameter("client_id", _idClient);
        restRequest.AddParameter("client_secret", _secretClient);
        restRequest.AddParameter("token", token);

        var reponse = await _client.ExecuteAsync(restRequest);
        return reponse.IsSuccessful;
    }

    /// <summary>
    /// Exécute une requête sur l'API Intacct (endpoint /service/core/query).
    /// </summary>
    public async Task<RestResponse> Query(QueryRequest request, string accessToken)
    {
        var restRequest = new RestRequest("services/core/query", Method.Post);
        restRequest.AddHeader("Authorization", "Bearer " + accessToken);
        restRequest.AddJsonBody(request);
        return await _client.ExecuteAsync(restRequest);
    }

    /// <summary>
    /// Exporte le résultat d'une requête en fichier (endpoint /service/core/export).
    /// Même requête que Query, avec un format de fichier (pdf, csv, word, xml, xlsx).
    /// </summary>
    public async Task<RestResponse> Export(QueryRequest request, ExportFileType fileType, string accessToken)
    {
        var body = new ExportRequest
        {
            Query = request,
            FileType = fileType.ToString().ToLowerInvariant()
        };
        var restRequest = new RestRequest("services/core/export", Method.Post);
        restRequest.AddHeader("Authorization", "Bearer " + accessToken);
        restRequest.AddJsonBody(body);
        return await _client.ExecuteAsync(restRequest);
    }

    /// <summary>
    /// Récupère une liste de factures (références légères) via GET /objects/accounts-receivable/invoice.
    /// Cette opération est surtout utile pour des tests / découvertes ;
    /// pour la pagination et les filtres, Sage recommande d'utiliser le service Query.
    /// </summary>
    public async Task<RestResponse> GetInvoices(string accessToken)
    {
        var restRequest = new RestRequest("objects/accounts-receivable/invoice", Method.Get);
        restRequest.AddHeader("Authorization", "Bearer " + accessToken);
        return await _client.ExecuteAsync(restRequest);
    }

    /// <summary>
    /// Récupère le détail d'une facture par sa clé via GET /objects/accounts-receivable/invoice/{key}.
    /// Le schéma de réponse est spécifique à l'objet facture (en-tête + lignes).
    /// </summary>
    public async Task<RestResponse> GetInvoiceByKey(string key, string accessToken)
    {
        var restRequest = new RestRequest($"objects/accounts-receivable/invoice/{key}", Method.Get);
        restRequest.AddHeader("Authorization", "Bearer " + accessToken);
        return await _client.ExecuteAsync(restRequest);
    }

    /// <summary>
    /// Crée une facture via POST /objects/accounts-receivable/invoice.
    /// Corps minimal : customer (objet { id }), invoiceDate, dueDate, lines (txnAmount, glAccount objet, dimensions.customer/location/department objets). En C# on peut écrire .Customer = "CL0170", .Dimensions.Location = "DEMO_1".
    /// </summary>
    public async Task<RestResponse> CreateInvoice(InvoiceCreate invoiceCreate, string accessToken)
    {
        var restRequest = new RestRequest("objects/accounts-receivable/invoice", Method.Post);
        restRequest.AddHeader("Authorization", "Bearer " + accessToken);
        restRequest.AddJsonBody(invoiceCreate);
        return await _client.ExecuteAsync(restRequest);
    }

    /// <summary>
    /// Met à jour une facture via PATCH /objects/accounts-receivable/invoice/{key}.
    /// Corps partiel : referenceNumber, description, dueDate (seuls les champs renseignés sont envoyés).
    /// </summary>
    public async Task<RestResponse> UpdateInvoice(InvoiceUpdate invoiceUpdate, string key, string accessToken)
    {
        var restRequest = new RestRequest($"objects/accounts-receivable/invoice/{key}", Method.Patch);
        restRequest.AddHeader("Authorization", "Bearer " + accessToken);
        restRequest.AddJsonBody(invoiceUpdate);
        return await _client.ExecuteAsync(restRequest);
    }

    public async Task<RestResponse> UpdateInvoiceLine(InvoiceLineUpdate invoiceLineUpdate, string lineKey, string accessToken)
    {
        var restRequest = new RestRequest($"objects/accounts-receivable/invoice-line/{lineKey}", Method.Patch);
        restRequest.AddHeader("Authorization", "Bearer " + accessToken);
        var json = JsonConvert.SerializeObject(invoiceLineUpdate, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        restRequest.AddStringBody(json, DataFormat.Json);
        return await _client.ExecuteAsync(restRequest);
    }

    /// <summary>
    /// Met à jour une ligne de bill via PATCH /objects/accounts-payable/bill-line/{key}. Body sérialisé avec Newtonsoft (NullValueHandling.Ignore).
    /// </summary>
    public async Task<RestResponse> UpdateBillLine(BillLineUpdate billLineUpdate, string lineKey, string accessToken)
    {
        var restRequest = new RestRequest($"objects/accounts-payable/bill-line/{lineKey}", Method.Patch);
        restRequest.AddHeader("Authorization", "Bearer " + accessToken);
        var json = JsonConvert.SerializeObject(billLineUpdate, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        restRequest.AddStringBody(json, DataFormat.Json);
        return await _client.ExecuteAsync(restRequest);
    }

    public Task<RestResponse> DeleteInvoice(string key, string accessToken)
    {
        var restRequest = new RestRequest($"objects/accounts-receivable/invoice/{key}", Method.Delete);
        restRequest.AddHeader("Authorization", "Bearer " + accessToken);
        return _client.ExecuteAsync(restRequest);
    }
}