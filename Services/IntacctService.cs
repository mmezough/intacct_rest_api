using intacct_rest_api.Models.Query;
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

    /// <summary>
    /// Obtient un token OAuth2 via Client Credentials (POST oauth2/token).
    /// </summary>
    public async Task<RestResponse> ObtenirToken()
    {
        var request = new RestRequest("oauth2/token", Method.Post);
        request.AddParameter("grant_type", "client_credentials");
        request.AddParameter("client_id", _idClient);
        request.AddParameter("client_secret", _secretClient);
        request.AddParameter("username", _utilisateur);
        return await _client.ExecuteAsync(request);
    }

    /// <summary>
    /// Exécute une requête Query (POST /services/core/query).
    /// </summary>
    public async Task<RestResponse> Query(QueryRequest queryRequest, string accessToken)
    {
        var request = new RestRequest("services/core/query", Method.Post);
        request.AddHeader("Authorization", "Bearer " + accessToken);
        request.AddJsonBody(queryRequest);
        return await _client.ExecuteAsync(request);
    }
}
