using Newtonsoft.Json;

namespace intacct_rest_api.Models.Composite;

/// <summary>
/// Réponse de POST /services/core/composite (200).
/// ia::result = un élément par sous-requête (chaque élément contient ia::result, ia::meta, ia::status).
/// </summary>
public class CompositeResponse
{
    [JsonProperty("ia::result")]
    public List<object> Result { get; set; } = new();

    [JsonProperty("ia::meta")]
    public CompositeMeta Meta { get; set; } = new();
}

/// <summary>Métadonnées globales de la réponse composite.</summary>
public class CompositeMeta
{
    public int totalCount { get; set; }
    public int totalSuccess { get; set; }
    public int totalError { get; set; }
}
