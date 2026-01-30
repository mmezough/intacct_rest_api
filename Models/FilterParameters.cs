using System.Text.Json.Serialization;

namespace intacct_rest_api.Models;

/// <summary>
/// Paramètres optionnels pour l'évaluation des filtres.
/// </summary>
public class FilterParameters
{
    [JsonPropertyName("caseSensitiveComparison")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? CaseSensitiveComparison { get; set; }

    [JsonPropertyName("includePrivate")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IncludePrivate { get; set; }
}
