using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.Invoice;

/// <summary>
/// Corps minimal pour créer une facture (POST /objects/accounts-receivable/invoice).
/// Champs simplifiés : customer, glAccount, dimensions.customer sont des chaînes (id).
/// </summary>
public class InvoiceCreateRequest
{
    [JsonPropertyName("customer")]
    public string Customer { get; set; } = string.Empty;

    [JsonPropertyName("invoiceDate")]
    public string InvoiceDate { get; set; } = string.Empty;

    [JsonPropertyName("dueDate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("lines")]
    public List<InvoiceCreateLine> Lines { get; set; } = new();
}

/// <summary>Ligne de facture minimale : montant, compte (id), dimension client (id).</summary>
public class InvoiceCreateLine
{
    [JsonPropertyName("txnAmount")]
    public string TxnAmount { get; set; } = string.Empty;

    [JsonPropertyName("glAccount")]
    public string GlAccount { get; set; } = string.Empty;

    [JsonPropertyName("dimensions")]
    public InvoiceCreateLineDimensions Dimensions { get; set; } = new();
}

/// <summary>Dimensions de la ligne : customer = id (string).</summary>
public class InvoiceCreateLineDimensions
{
    [JsonPropertyName("customer")]
    public string Customer { get; set; } = string.Empty;
}
