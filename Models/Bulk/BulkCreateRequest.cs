namespace intacct_rest_api.Models.Bulk;

/// <summary>
/// Corps de la partie "ia::requestBody" pour POST /services/bulk/job/create (multipart).
/// Décrit l'objet cible, l'opération, le nom du champ fichier et le type de contenu.
/// </summary>
public class BulkCreateRequest
{
    public string objectName { get; set; } = string.Empty;
    public string operation { get; set; } = string.Empty;
    public string jobFile { get; set; } = "file";
    public string fileContentType { get; set; } = "json";
    public string? callbackURL { get; set; }
}
