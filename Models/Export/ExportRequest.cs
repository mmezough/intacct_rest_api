using intacct_rest_api.Models.Query;
using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.Export;

/// <summary>
/// Corps de la requête Export : la requête Query + le format de fichier.
/// </summary>
internal class ExportRequest
{
    [JsonPropertyName("query")]
    public QueryRequest Query { get; set; } = null!;

    [JsonPropertyName("fileType")]
    public string FileType { get; set; } = string.Empty;
}
