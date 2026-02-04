using Newtonsoft.Json;

namespace intacct_rest_api.Models.BillLineUpdate;

/// <summary>
/// Corps minimal pour modifier une ligne de bill (PATCH /objects/accounts-payable/bill-line/{key}).
/// Seuls les champs renseignés sont envoyés ; les null sont ignorés à la sérialisation.
/// </summary>
public class BillLineUpdate
{
    [JsonProperty("glAccount", NullValueHandling = NullValueHandling.Ignore)]
    public GlAccountRef? GlAccount { get; set; }

    [JsonProperty("txnAmount", NullValueHandling = NullValueHandling.Ignore)]
    public string? TxnAmount { get; set; }

    [JsonProperty("memo", NullValueHandling = NullValueHandling.Ignore)]
    public string? Memo { get; set; }

    [JsonProperty("dimensions", NullValueHandling = NullValueHandling.Ignore)]
    public BillLineDimensions? Dimensions { get; set; }
}

/// <summary>
/// Référence compte général (un seul identifiant : id).
/// </summary>
public class GlAccountRef
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string? Id { get; set; }
}

/// <summary>
/// Dimensions de la ligne : 2 dimensions pour faciliter la compréhension (department, location).
/// </summary>
public class BillLineDimensions
{
    [JsonProperty("department", NullValueHandling = NullValueHandling.Ignore)]
    public KeyIdRef? Department { get; set; }

    [JsonProperty("location", NullValueHandling = NullValueHandling.Ignore)]
    public KeyIdRef? Location { get; set; }
}

/// <summary>
/// Référence dimension (un seul identifiant : id).
/// </summary>
public class KeyIdRef
{
    [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
    public string? Id { get; set; }
}
