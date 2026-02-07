using intacct_rest_api.Models.InvoiceCreate;

namespace intacct_rest_api.Models.InvoiceLineUpdate;

public class InvoiceLineUpdate
{
    public IdRef? glAccount { get; set; }
    public string? txnAmount { get; set; }
    public string? memo { get; set; }
    public InvoiceLineDimensions? dimensions { get; set; }
}

public class InvoiceLineDimensions
{
    public IdRef? location { get; set; }
    public IdRef? customer { get; set; }
}