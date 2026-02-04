using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.InvoiceUpdate;

/// <summary>
/// Corps minimal pour modifier une facture (PATCH /objects/accounts-receivable/invoice/key).
/// </summary>
public class InvoiceUpdate
{
    public string referenceNumber { get; set; }
    public string description { get; set; }
    public string dueDate { get; set; }
}
