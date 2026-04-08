using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace intacct_rest_api.Models.Batch;

public class BatchResponse
{
    [JsonProperty("ia::result")]
    public List<BatchResultItem> Result { get; set; } = new();

    [JsonProperty("ia::meta")]
    public BatchMeta Meta { get; set; } = new();
}

public class BatchResultItem
{
    public string? key { get; set; }
    public string? id { get; set; }
    public string? href { get; set; }

    [JsonProperty("ia::status")]
    public int? Status { get; set; }

    [JsonProperty("ia::error")]
    public BatchError? Error { get; set; }
}

public class BatchMeta
{
    public int totalCount { get; set; }
    public int totalSuccess { get; set; }
    public int totalError { get; set; }
}

public class BatchError
{
    public string? code { get; set; }
    public string? message { get; set; }
    public string? errorId { get; set; }
    public string? supportId { get; set; }
    public JToken? details { get; set; }
}

public class BatchPatchItem : Dictionary<string, object?>
{
    [JsonIgnore]
    public string? key
    {
        get => TryGetValue("key", out var value) ? value?.ToString() : null;
        set => this["key"] = value;
    }
}
