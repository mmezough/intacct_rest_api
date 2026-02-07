using Newtonsoft.Json;

namespace intacct_rest_api.Models;

/// <summary>
/// Référence légère d'une facture (GET /objects/accounts-receivable/invoice).
/// Utilisée pour les opérations de test / découverte : on récupère uniquement
/// la clé technique, l'identifiant fonctionnel et le lien (href).
/// </summary>
public class InvoiceReference
{
    /// <summary>Clé technique interne Intacct (colonne "key").</summary>
    public string key { get; set; } = string.Empty;

    /// <summary>Identifiant fonctionnel de la facture (invoiceNumber / id lisible).</summary>
    public string id { get; set; } = string.Empty;

    /// <summary>Lien vers la ressource facture (GET single invoice).</summary>
    public string href { get; set; } = string.Empty;
}

/// <summary>
/// Réponse liste de factures (GET /invoice). On ne mappe que "ia::result".
/// </summary>
public class InvoiceReferenceListResponse
{
    [JsonProperty("ia::result")]
    public List<InvoiceReference> Result { get; set; } = new();
}

