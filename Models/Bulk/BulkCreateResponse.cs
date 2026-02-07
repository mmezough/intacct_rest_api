using Newtonsoft.Json;

namespace intacct_rest_api.Models.Bulk;

/// <summary>
/// Réponse de POST /services/bulk/job/create (201).
/// </summary>
public class BulkCreateResponse
{
    [JsonProperty("ia::result")]
    public BulkCreateResult Result { get; set; } = new();
}

public class BulkCreateResult
{
    public string jobId { get; set; } = string.Empty;
}
