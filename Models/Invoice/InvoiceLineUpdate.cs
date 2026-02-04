using Newtonsoft.Json;

namespace intacct_rest_api.Models.InvoiceLineUpdate;

/// <summary>
/// Corps minimal pour modifier une ligne de facture (PATCH /objects/accounts-receivable/invoice-line/{key}).
/// Seuls les champs renseignés sont envoyés ; les null sont ignorés à la sérialisation.
/// </summary>
public class InvoiceLineUpdate
{
    [JsonProperty("glAccount", NullValueHandling = NullValueHandling.Ignore)]
    public GlAccountRef? GlAccount { get; set; }

    [JsonProperty("txnAmount", NullValueHandling = NullValueHandling.Ignore)]
    public string? TxnAmount { get; set; }

    [JsonProperty("memo", NullValueHandling = NullValueHandling.Ignore)]
    public string? Memo { get; set; }

    [JsonProperty("dimensions", NullValueHandling = NullValueHandling.Ignore)]
    public InvoiceLineDimensions? Dimensions { get; set; }
}

/// <summary>
/// Référence compte général (key et/ou id).
/// </summary>
public class GlAccountRef
{
    [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
    public string? Key { get; set; }

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string? Id { get; set; }
}

/// <summary>
/// Dimensions de la ligne : 2 dimensions pour faciliter la compréhension (location, customer).
/// </summary>
public class InvoiceLineDimensions
{
    [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
    public KeyIdRef? Location { get; set; }

    [JsonProperty("customer", NullValueHandling = NullValueHandling.Ignore)]
    public KeyIdRef? Customer { get; set; }
}

/// <summary>
/// Référence générique key/id pour les dimensions.
/// </summary>
public class KeyIdRef
{
    [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
    public string? Key { get; set; }

    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string? Id { get; set; }
}
