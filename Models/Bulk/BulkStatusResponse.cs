using Newtonsoft.Json;

namespace intacct_rest_api.Models.Bulk;

/// <summary>
/// Réponse de GET /services/bulk/job/status (200).
/// </summary>
public class BulkStatusResponse
{
    [JsonProperty("ia::result")]
    public BulkStatusResult Result { get; set; } = new();
}

/// <summary>Contenu de ia::result pour le statut d'un job bulk.</summary>
public class BulkStatusResult
{
    public string status { get; set; } = string.Empty;
    public int? percentComplete { get; set; }
}
