using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.Invoice;

/// <summary>
/// Corps minimal pour créer une facture (POST /objects/accounts-receivable/invoice).
/// customer, glAccount et dimensions.* sont des objets { "id": "..." }. On peut assigner une string (ex. invoice.Customer = "CL0170").
/// </summary>
public class InvoiceCreateRequest
{
    /// <summary>Client : objet { "id": "..." }. Assigner une string : Customer = "CL0170".</summary>
    [JsonPropertyName("customer")]
    public InvoiceCreateIdRef Customer { get; set; }

    [JsonPropertyName("invoiceDate")]
    public string InvoiceDate { get; set; } = string.Empty;

    [JsonPropertyName("dueDate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("lines")]
    public List<InvoiceCreateLine> Lines { get; set; } = new();
}

/// <summary>
/// Référence par id : sérialise en { "id": "..." }.
/// Permet d'assigner une string : IdRef = "CL0170" équivaut à new InvoiceCreateIdRef { Id = "CL0170" }.
/// </summary>
public class InvoiceCreateIdRef
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    public static implicit operator InvoiceCreateIdRef(string id) => new() { Id = id ?? string.Empty };
}

/// <summary>Ligne de facture minimale : montant, glAccount (objet), dimensions (objets).</summary>
public class InvoiceCreateLine
{
    [JsonPropertyName("txnAmount")]
    public string TxnAmount { get; set; } = string.Empty;

    /// <summary>Compte : objet { "id": "..." }. Assigner une string : GlAccount = "701000".</summary>
    [JsonPropertyName("glAccount")]
    public InvoiceCreateIdRef GlAccount { get; set; }

    [JsonPropertyName("dimensions")]
    public InvoiceCreateLineDimensions Dimensions { get; set; } = new();
}

/// <summary>
/// Dimensions de la ligne : customer, location, department = objets { "id": "..." }.
/// Assigner une string : Dimensions.Customer = "CL0170", Dimensions.Location = "DEMO_1".
/// </summary>
public class InvoiceCreateLineDimensions
{
    [JsonPropertyName("customer")]
    public InvoiceCreateIdRef Customer { get; set; }

    /// <summary>Dimension lieu (optionnel).</summary>
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InvoiceCreateIdRef? Location { get; set; }

    /// <summary>Dimension département (optionnel).</summary>
    [JsonPropertyName("department")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InvoiceCreateIdRef? Department { get; set; }
}
