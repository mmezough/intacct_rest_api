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
        var requete = new RestRequest("oauth2/token", Method.Post);

        requete.AddParameter("grant_type", "client_credentials");
        requete.AddParameter("client_id", _idClient);
        requete.AddParameter("client_secret", _secretClient);
        requete.AddParameter("username", _utilisateur);

        var reponse = await _client.ExecuteAsync(requete);
        return reponse;
    }

    public async Task<RestResponse> RafraichirToken(string refreshToken)
    {
        var requete = new RestRequest("oauth2/token", Method.Post);

        requete.AddParameter("grant_type", "refresh_token");
        requete.AddParameter("client_id", _idClient);
        requete.AddParameter("client_secret", _secretClient);
        requete.AddParameter("refresh_token", refreshToken);

        var reponse = await _client.ExecuteAsync(requete);
        return reponse;
    }

    public async Task<bool> RevokerToken(string token)
    {
        var requete = new RestRequest("oauth2/revoke", Method.Post);

        requete.AddParameter("client_id", _idClient);
        requete.AddParameter("client_secret", _secretClient);
        requete.AddParameter("token", token);

        var reponse = await _client.ExecuteAsync(requete);
        return reponse.IsSuccessful;
    }

    /// <summary>
    /// Exécute une requête sur l'API Intacct (endpoint /service/core/query).
    /// </summary>
    public async Task<RestResponse> Query(QueryRequest request, string accessToken)
    {
        var requete = new RestRequest("services/core/query", Method.Post);
        requete.AddHeader("Authorization", "Bearer " + accessToken);
        requete.AddJsonBody(request);
        return await _client.ExecuteAsync(requete);
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
        var requete = new RestRequest("services/core/export", Method.Post);
        requete.AddHeader("Authorization", "Bearer " + accessToken);
        requete.AddJsonBody(body);
        return await _client.ExecuteAsync(requete);
    }

    /// <summary>
    /// Récupère une liste de factures (références légères) via GET /objects/accounts-receivable/invoice.
    /// Cette opération est surtout utile pour des tests / découvertes ;
    /// pour la pagination et les filtres, Sage recommande d'utiliser le service Query.
    /// </summary>
    public async Task<RestResponse> GetInvoices(string accessToken)
    {
        var requete = new RestRequest("objects/accounts-receivable/invoice", Method.Get);
        requete.AddHeader("Authorization", "Bearer " + accessToken);
        return await _client.ExecuteAsync(requete);
    }

    /// <summary>
    /// Récupère le détail d'une facture par sa clé via GET /objects/accounts-receivable/invoice/{key}.
    /// Le schéma de réponse est spécifique à l'objet facture (en-tête + lignes).
    /// </summary>
    public async Task<RestResponse> GetInvoiceByKey(string key, string accessToken)
    {
        var requete = new RestRequest($"objects/accounts-receivable/invoice/{key}", Method.Get);
        requete.AddHeader("Authorization", "Bearer " + accessToken);
        return await _client.ExecuteAsync(requete);
    }

    /// <summary>
    /// Crée une facture via POST /objects/accounts-receivable/invoice.
    /// Corps minimal : customer (objet { id }), invoiceDate, dueDate, lines (txnAmount, glAccount objet, dimensions.customer/location/department objets). En C# on peut écrire .Customer = "CL0170", .Dimensions.Location = "DEMO_1".
    /// </summary>
    public async Task<RestResponse> CreateInvoice(InvoiceCreate request, string accessToken)
    {
        var requete = new RestRequest("objects/accounts-receivable/invoice", Method.Post);
        requete.AddHeader("Authorization", "Bearer " + accessToken);
        requete.AddJsonBody(request);
        return await _client.ExecuteAsync(requete);
    }

    /// <summary>
    /// Met à jour une facture via PATCH /objects/accounts-receivable/invoice/{key}.
    /// Corps partiel : referenceNumber, description, dueDate (seuls les champs renseignés sont envoyés).
    /// </summary>
    public async Task<RestResponse> UpdateInvoice(InvoiceUpdate request, string key, string accessToken)
    {
        var requete = new RestRequest($"objects/accounts-receivable/invoice/{key}", Method.Patch);
        requete.AddHeader("Authorization", "Bearer " + accessToken);
        requete.AddJsonBody(request);
        return await _client.ExecuteAsync(requete);
    }

    /// <summary>
    /// Met à jour une ligne de facture via PATCH /objects/accounts-receivable/invoice-line/{key}. Body sérialisé avec Newtonsoft (NullValueHandling.Ignore).
    /// </summary>
    public async Task<RestResponse> UpdateInvoiceLine(InvoiceLineUpdate request, string lineKey, string accessToken)
    {
        var requete = new RestRequest($"objects/accounts-receivable/invoice-line/{lineKey}", Method.Patch);
        requete.AddHeader("Authorization", "Bearer " + accessToken);
        var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        requete.AddStringBody(json, DataFormat.Json);
        return await _client.ExecuteAsync(requete);
    }

    /// <summary>
    /// Met à jour une ligne de bill via PATCH /objects/accounts-payable/bill-line/{key}. Body sérialisé avec Newtonsoft (NullValueHandling.Ignore).
    /// </summary>
    public async Task<RestResponse> UpdateBillLine(BillLineUpdate request, string lineKey, string accessToken)
    {
        var requete = new RestRequest($"objects/accounts-payable/bill-line/{lineKey}", Method.Patch);
        requete.AddHeader("Authorization", "Bearer " + accessToken);
        var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        requete.AddStringBody(json, DataFormat.Json);
        return await _client.ExecuteAsync(requete);
    }
}