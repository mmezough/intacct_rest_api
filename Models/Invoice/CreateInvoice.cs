using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.Invoice;

/// <summary>
/// Corps minimal pour créer une facture (POST /objects/accounts-receivable/invoice).
/// customer, glAccount et dimensions.* sont des objets { "id": "..." }. On assigne explicitement .Id (ex. Customer.Id = "CL0170").
/// </summary>
public class CreateInvoice
{
    public IdRef customer { get; set; } = new();
    public string invoiceDate { get; set; } = string.Empty;
    public string dueDate { get; set; } = string.Empty;
    public List<Line> lines { get; set; } = new();
}

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
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdRef? customer { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdRef? location { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdRef? department { get; set; }
}
