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
    public int totalCount { get; set; }
    public int start { get; set; }
    public int pageSize { get; set; }
    public string? next { get; set; }
    public string? previous { get; set; }
}
