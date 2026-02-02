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
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    /// <summary>Identifiant fonctionnel de la facture (invoiceNumber / id lisible).</summary>
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>Lien vers la ressource facture (GET single invoice).</summary>
    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;
}

/// <summary>
/// Métadonnées pour la réponse de liste de factures.
/// Même idée que pour Query : total, pagination, taille de page.
/// Cette liste est surtout prévue pour des tests, pas pour de vraies extractions.
/// </summary>
public class InvoiceReferenceListMeta
{
    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }

    [JsonProperty("start")]
    public int Start { get; set; }

    [JsonProperty("pageSize")]
    public int PageSize { get; set; }
}

/// <summary>
/// Réponse complète de la liste des factures (GET /invoice).
/// "ia::result" contient les références, "ia::meta" les métadonnées.
/// </summary>
public class InvoiceReferenceListResponse
{
    [JsonProperty("ia::result")]
    public List<InvoiceReference> Result { get; set; } = new();

    [JsonProperty("ia::meta")]
    public InvoiceReferenceListMeta Meta { get; set; } = new();
}

