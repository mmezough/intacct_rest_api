using intacct_rest_api.Models;

namespace intacct_rest_api.Models.InvoiceCreate;

/// <summary>
/// Corps minimal pour créer une facture (POST /objects/accounts-receivable/invoice).
/// customer, glAccount et dimensions.* sont des objets { "id": "..." }. On assigne explicitement .id (ex. customer.id = "CL0170").
/// </summary>
public class InvoiceCreate
{
    public IdRef customer { get; set; } = new();
    public string invoiceDate { get; set; } = string.Empty;
    public string dueDate { get; set; } = string.Empty;
    public List<Line> lines { get; set; } = new();
}

public class Line
{
    public string txnAmount { get; set; } = string.Empty;
    public IdRef glAccount { get; set; } = new();
    public LineDimensions dimensions { get; set; } = new();
}
public class LineDimensions
{
    public IdRef? customer { get; set; }
    public IdRef? location { get; set; }
}
