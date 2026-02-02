using Newtonsoft.Json;

namespace intacct_rest_api.Models.Query;

/// <summary>
/// Réponse générique d'une requête Query. Désérialiser avec Newtonsoft (ex. JsonConvert.DeserializeObject&lt;QueryResponse&gt;(content)).
/// </summary>
public class QueryResponse
{
    [JsonProperty("ia::result")]
    public List<Dictionary<string, object>> Result { get; set; } = new();

    [JsonProperty("ia::meta")]
    public QueryResponseMeta Meta { get; set; } = new();
}

/// <summary>Métadonnées de la réponse (pagination, total).</summary>
public class QueryResponseMeta
{
    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }

    [JsonProperty("start")]
    public int Start { get; set; }

    [JsonProperty("pageSize")]
    public int PageSize { get; set; }

    [JsonProperty("next")]
    public string? Next { get; set; }

    [JsonProperty("previous")]
    public string? Previous { get; set; }
}
