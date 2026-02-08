namespace intacct_rest_api.Models.Composite;

/// <summary>
/// Une sous-requête dans un appel composite (POST /services/core/composite).
/// method + path obligatoires ; body pour POST/PATCH ; resultReference pour réutilisation (ex. @{vendor.1.key}) ; headers optionnel (ex. Idempotency-Key).
/// </summary>
public class CompositeSubRequest
{
    /// <summary>Méthode HTTP : GET, POST, PATCH, DELETE.</summary>
    public string method { get; set; } = string.Empty;

    /// <summary>Chemin de la ressource (ex. /objects/accounts-receivable/invoice).</summary>
    public string path { get; set; } = string.Empty;

    /// <summary>Corps pour POST ou PATCH (optionnel).</summary>
    public object? body { get; set; }

    /// <summary>Identifiant pour réutiliser le résultat dans une sous-requête suivante (ex. path = "/objects/.../@{vendor.1.key}").</summary>
    public string? resultReference { get; set; }

    /// <summary>En-têtes optionnels (ex. Idempotency-Key pour éviter les doublons).</summary>
    public Dictionary<string, string>? headers { get; set; }
}
