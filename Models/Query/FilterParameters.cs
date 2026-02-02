using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.Query;

/// <summary>
/// Paramètres optionnels pour l'évaluation des filtres.
/// </summary>
public class FilterParameters
{
    [JsonPropertyName("caseSensitiveComparison")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? CaseSensitiveComparison { get; set; }

    [JsonPropertyName("includePrivate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IncludePrivate { get; set; }
}
