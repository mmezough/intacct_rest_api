using System.Text.Json.Serialization;

namespace intacct_rest_api.Models.Invoice;

/// <summary>
/// Corps minimal pour créer une facture (POST /objects/accounts-receivable/invoice).
/// customer, glAccount et dimensions.* sont des objets { "id": "..." }. On assigne explicitement .Id (ex. Customer.Id = "CL0170").
/// </summary>
public class CreateInvoice
{
    /// <summary>Client : objet { "id": "..." }. Assigner : Customer.Id = "CL0170".</summary>
    [JsonPropertyName("customer")]
    public IdRef Customer { get; set; } = new();

    [JsonPropertyName("invoiceDate")]
    public string InvoiceDate { get; set; } = string.Empty;

    [JsonPropertyName("dueDate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonPropertyName("lines")]
    public List<Line> Lines { get; set; } = new();
}

/// <summary>Référence par id : sérialise en { "id": "..." }. Réutilisable (invoice, bill, etc.).</summary>
public class IdRef
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

/// <summary>Ligne de facture minimale : montant, glAccount (objet .Id), dimensions (objets .Id).</summary>
public class Line
{
    [JsonPropertyName("txnAmount")]
    public string TxnAmount { get; set; } = string.Empty;

    /// <summary>Compte : objet { "id": "..." }. Assigner : GlAccount.Id = "701000".</summary>
    [JsonPropertyName("glAccount")]
    public IdRef GlAccount { get; set; } = new();

    [JsonPropertyName("dimensions")]
    public LineDimensions Dimensions { get; set; } = new();
}

/// <summary>
/// Dimensions de la ligne : customer, location, department = objets { "id": "..." }.
/// Assigner : Dimensions.Customer.Id = "CL0170", Dimensions.Location.Id = "DEMO_1".
/// </summary>
public class LineDimensions
{
    [JsonPropertyName("customer")]
    public IdRef Customer { get; set; } = new();

    /// <summary>Dimension lieu (optionnel).</summary>
    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdRef? Location { get; set; }

    /// <summary>Dimension département (optionnel).</summary>
    [JsonPropertyName("department")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdRef? Department { get; set; }
}
