using intacct_rest_api.Models;
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
}