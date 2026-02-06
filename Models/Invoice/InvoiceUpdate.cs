using Newtonsoft.Json;

namespace intacct_rest_api.Models.InvoiceUpdate;

/// <summary>
/// Corps pour modifier une facture (PATCH /objects/accounts-receivable/invoice/{key}).
/// Seuls les champs renseignés sont envoyés ; les null sont ignorés à la sérialisation.
/// </summary>
public class InvoiceUpdate
{
    public string? referenceNumber { get; set; }
    public string? description { get; set; }
    public string? dueDate { get; set; }
}

