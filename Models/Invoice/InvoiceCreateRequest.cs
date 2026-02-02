using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.Invoice;

/// <summary>
/// Corps minimal pour créer une facture (POST /objects/accounts-receivable/invoice).
/// Cohérent avec Query/Export : sérialisation System.Text.Json (JsonPropertyName).
/// </summary>
public class InvoiceCreateRequest
{
    [JsonPropertyName("customer")]
    public InvoiceCreateCustomerRef Customer { get; set; } = new();

    [JsonPropertyName("invoiceDate")]
    public string InvoiceDate { get; set; } = string.Empty;

    [JsonPropertyName("dueDate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("lines")]
    public List<InvoiceCreateLine> Lines { get; set; } = new();
}

/// <summary>Référence client sur l'en-tête (id uniquement).</summary>
public class InvoiceCreateCustomerRef
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>Ligne de facture minimale : montant, compte, dimension client.</summary>
public class InvoiceCreateLine
{
    [JsonPropertyName("txnAmount")]
    public string TxnAmount { get; set; } = string.Empty;

    [JsonPropertyName("glAccount")]
    public InvoiceCreateLineGlAccount GlAccount { get; set; } = new();

    [JsonPropertyName("dimensions")]
    public InvoiceCreateLineDimensions Dimensions { get; set; } = new();
}

/// <summary>Compte général de la ligne (id uniquement).</summary>
public class InvoiceCreateLineGlAccount
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>Dimensions de la ligne (client uniquement).</summary>
public class InvoiceCreateLineDimensions
{
    [JsonPropertyName("customer")]
    public InvoiceCreateLineDimensionCustomer Customer { get; set; } = new();
}

/// <summary>Dimension client sur la ligne (id uniquement).</summary>
public class InvoiceCreateLineDimensionCustomer
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}
