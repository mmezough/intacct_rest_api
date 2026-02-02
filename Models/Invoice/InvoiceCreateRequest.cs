using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.Invoice;

/// <summary>
/// Corps minimal pour créer une facture (POST /objects/accounts-receivable/invoice).
/// customer, glAccount et dimensions.* sont des objets { "id": "..." }. On assigne explicitement .Id (ex. Customer.Id = "CL0170").
/// </summary>
public class InvoiceCreateRequest
{
    /// <summary>Client : objet { "id": "..." }. Assigner : Customer.Id = "CL0170".</summary>
    [JsonPropertyName("customer")]
    public InvoiceCreateIdRef Customer { get; set; } = new();

    [JsonPropertyName("invoiceDate")]
    public string InvoiceDate { get; set; } = string.Empty;

    [JsonPropertyName("dueDate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("lines")]
    public List<InvoiceCreateLine> Lines { get; set; } = new();
}

/// <summary>Référence par id : sérialise en { "id": "..." }. Assigner explicitement .Id.</summary>
public class InvoiceCreateIdRef
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>Ligne de facture minimale : montant, glAccount (objet .Id), dimensions (objets .Id).</summary>
public class InvoiceCreateLine
{
    [JsonPropertyName("txnAmount")]
    public string TxnAmount { get; set; } = string.Empty;

    /// <summary>Compte : objet { "id": "..." }. Assigner : GlAccount.Id = "701000".</summary>
    [JsonPropertyName("glAccount")]
    public InvoiceCreateIdRef GlAccount { get; set; } = new();

    [JsonPropertyName("dimensions")]
    public InvoiceCreateLineDimensions Dimensions { get; set; } = new();
}

/// <summary>
/// Dimensions de la ligne : customer, location, department = objets { "id": "..." }.
/// Assigner : Dimensions.Customer.Id = "CL0170", Dimensions.Location.Id = "DEMO_1".
/// </summary>
public class InvoiceCreateLineDimensions
{
    [JsonPropertyName("customer")]
    public InvoiceCreateIdRef Customer { get; set; } = new();

    /// <summary>Dimension lieu (optionnel).</summary>
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InvoiceCreateIdRef? Location { get; set; }

    /// <summary>Dimension département (optionnel).</summary>
    [JsonPropertyName("department")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InvoiceCreateIdRef? Department { get; set; }
}
