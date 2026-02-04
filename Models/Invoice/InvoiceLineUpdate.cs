using Newtonsoft.Json;

namespace intacct_rest_api.Models.InvoiceLineUpdate;

/// <summary>
/// Corps minimal pour modifier une ligne de facture (PATCH /objects/accounts-receivable/invoice-line/{key}).
/// Seuls les champs renseignés sont envoyés ; les null sont ignorés. Envoi via Newtonsoft pour éviter glAccount/dimensions en tableau.
/// </summary>
public class InvoiceLineUpdate
{
    [JsonProperty("glAccount", NullValueHandling = NullValueHandling.Ignore)]
    public InvoiceLineGlAccountRef? GlAccount { get; set; }

    [JsonProperty("txnAmount", NullValueHandling = NullValueHandling.Ignore)]
    public string? TxnAmount { get; set; }

    [JsonProperty("memo", NullValueHandling = NullValueHandling.Ignore)]
    public string? Memo { get; set; }

    [JsonProperty("dimensions", NullValueHandling = NullValueHandling.Ignore)]
    public InvoiceLineDimensions? Dimensions { get; set; }
}

/// <summary>
/// Référence compte général (un seul identifiant : id).
/// </summary>
public class InvoiceLineGlAccountRef
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string? Id { get; set; }
}

/// <summary>
/// Dimensions de la ligne : 2 dimensions (location, customer).
/// </summary>
public class InvoiceLineDimensions
{
    [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
    public InvoiceLineKeyIdRef? Location { get; set; }

    [JsonProperty("customer", NullValueHandling = NullValueHandling.Ignore)]
    public InvoiceLineKeyIdRef? Customer { get; set; }
}

/// <summary>
/// Référence dimension (un seul identifiant : id).
/// </summary>
public class InvoiceLineKeyIdRef
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string? Id { get; set; }
}
