using Newtonsoft.Json;
using RestSharp;

namespace intacct_rest_api.Models;

public class Token
{
    public Token(RestResponse restResponse)
    {
        if (string.IsNullOrWhiteSpace(restResponse.Content)) return;

        JsonConvert.PopulateObject(restResponse.Content, this);
    }

    public string token_type { get; set; } = string.Empty;
    public string access_token { get; set; } = string.Empty;
    public string refresh_token { get; set; } = string.Empty;
    public int expires_in { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.Now;
    public DateTime DateExpiration => DateCreation.AddSeconds(expires_in);
    public bool EstExpire => DateTime.Now >= DateCreation.AddSeconds(expires_in - 60);
}
