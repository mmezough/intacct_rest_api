using Newtonsoft.Json;

namespace intacct_rest_api.Models.InvoiceUpdate;

/// <summary>
/// Corps pour modifier une facture (PATCH /objects/accounts-receivable/invoice/{key}).
/// Seuls les champs renseignés sont envoyés ; les null sont ignorés à la sérialisation.
/// </summary>
public class InvoiceUpdate
{
    [JsonProperty("referenceNumber", NullValueHandling = NullValueHandling.Ignore)]
    public string? ReferenceNumber { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    [JsonProperty("dueDate", NullValueHandling = NullValueHandling.Ignore)]
    public string? DueDate { get; set; }
}
