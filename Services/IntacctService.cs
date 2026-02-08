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
}
