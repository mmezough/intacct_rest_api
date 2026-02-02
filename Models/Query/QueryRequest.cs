using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace intacct_rest_api.Models.Query;

public class QueryRequest
{

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("fields")]
    public List<string> Fields { get; set; } = new();

    /// <summary>Filtres : tableau de { "$op": { "champ": valeur } } (optionnel).</summary>
    [JsonPropertyName("filters")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, object>>? Filters { get; set; }

    /// <summary>Expression combinant les filtres par index (1-based), ex. "(1 and 2) or 3" (optionnel).</summary>
    [JsonPropertyName("filterExpression")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FilterExpression { get; set; }

    /// <summary>Paramètres des filtres : caseSensitiveComparison, includePrivate (optionnel).</summary>
    [JsonPropertyName("filterParameters")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FilterParameters? FilterParameters { get; set; }

    /// <summary>Tri : tableau de { "champ": "asc" } ou { "champ": "desc" } (optionnel).</summary>
    [JsonPropertyName("orderBy")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Dictionary<string, string>>? OrderBy { get; set; }

    /// <summary>Premier enregistrement du jeu de résultats à inclure (optionnel).</summary>
    [JsonPropertyName("start")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Start { get; set; }

    /// <summary>Nombre d'enregistrements à inclure dans le jeu de résultats, 4000 maximum (optionnel).</summary>
    [JsonPropertyName("size")]
    [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Size { get; set; }
}
