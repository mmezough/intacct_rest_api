namespace intacct_rest_api.Models.Bulk;

/// <summary>
/// Corps de la partie "ia::requestBody" pour POST /services/bulk/job/create (multipart).
/// Décrit l'objet cible, l'opération, le nom du champ fichier et le type de contenu.
/// </summary>
public class BulkCreateRequest
{
    /// <summary>Objet API (ex. accounts-payable/vendor).</summary>
    public string objectName { get; set; } = string.Empty;

    /// <summary>Opération (ex. create, update).</summary>
    public string operation { get; set; } = string.Empty;

    /// <summary>Nom de la partie multipart qui contient le fichier JSON (ex. file). Doit correspondre à la clé du fichier envoyé.</summary>
    public string jobFile { get; set; } = "file";

    /// <summary>Type MIME du fichier (ex. json).</summary>
    public string fileContentType { get; set; } = "json";

    /// <summary>Optionnel : URL de callback que Intacct appellera (POST) quand le job est terminé.</summary>
    public string? callbackURL { get; set; }
}
