using intacct_rest_api.Models;
using intacct_rest_api.Models.Export;
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
}