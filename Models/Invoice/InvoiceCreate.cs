using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.InvoiceCreate;

/// <summary>
/// Corps minimal pour créer une facture (POST /objects/accounts-receivable/invoice).
/// customer, glAccount et dimensions.* sont des objets { "id": "..." }. On assigne explicitement .Id (ex. Customer.Id = "CL0170").
/// </summary>
public class InvoiceCreate
{
    public IdRef customer { get; set; } = new();
    public string invoiceDate { get; set; } = string.Empty;
    public string dueDate { get; set; } = string.Empty;
    public List<Line> lines { get; set; } = new();
}

// Pour avoir la syntaxe "object": { "id": "..." } ex "customer": { "id": "CL0170" }
public class IdRef
{
    public string id { get; set; } = string.Empty;
}

public class Line
{
    public string txnAmount { get; set; } = string.Empty;
    public IdRef glAccount { get; set; } = new();
    public LineDimensions dimensions { get; set; } = new();
}
public class LineDimensions
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IdRef? customer { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public IdRef? location { get; set; }
}
