using Newtonsoft.Json;
using RestSharp;

class IntacctToken
{
    public IntacctToken(RestResponse restResponse)
    {
        if (string.IsNullOrWhiteSpace(restResponse.Content)) return;

        // Remplir les propriétés de l'objet courant avec les données JSON de la réponse
        JsonConvert.PopulateObject(restResponse.Content, this);
    }

    [JsonProperty("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonProperty("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    public DateTime DateCreation { get; set; } = DateTime.Now;
    public DateTime DateExpiration => DateCreation.AddSeconds(ExpiresIn);
    public bool EstExpire => DateTime.Now >= DateCreation.AddSeconds(ExpiresIn - 60);
}