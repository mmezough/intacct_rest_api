using Newtonsoft.Json;

namespace intacct_rest_api.Models;

/// <summary>
/// Réponse détaillée d'une facture (GET /objects/accounts-receivable/invoice/{key}).
/// On ne projette que les champs intéressants pour le cours : en-tête + quelques lignes.
/// </summary>
public class InvoiceDetailResponse
{
    /// <summary>En-tête de la facture (correspond à "ia::result").</summary>
    [JsonProperty("ia::result")]
    public InvoiceHeader Invoice { get; set; } = new();

    /// <summary>Métadonnées de la réponse (compteurs succès / erreurs).</summary>
    [JsonProperty("ia::meta")]
    public InvoiceDetailMeta Meta { get; set; } = new();
}

/// <summary>Métadonnées pour la réponse de facture unique.</summary>
public class InvoiceDetailMeta
{
    [JsonProperty("totalCount")]
    public int TotalCount { get; set; }

    [JsonProperty("totalSuccess")]
    public int TotalSuccess { get; set; }

    [JsonProperty("totalError")]
    public int TotalError { get; set; }
}

/// <summary>
/// En-tête de facture : identifiants, dates, montants, client, devise et lignes.
/// </summary>
public class InvoiceHeader
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("recordType")]
    public string RecordType { get; set; } = string.Empty;

    [JsonProperty("invoiceNumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [JsonProperty("state")]
    public string State { get; set; } = string.Empty;

    [JsonProperty("invoiceDate")]
    public string InvoiceDate { get; set; } = string.Empty;

    [JsonProperty("dueDate")]
    public string DueDate { get; set; } = string.Empty;

    [JsonProperty("totalBaseAmount")]
    public string TotalBaseAmount { get; set; } = string.Empty;

    [JsonProperty("totalBaseAmountDue")]
    public string TotalBaseAmountDue { get; set; } = string.Empty;

    [JsonProperty("totalTxnAmount")]
    public string TotalTxnAmount { get; set; } = string.Empty;

    [JsonProperty("totalTxnAmountDue")]
    public string TotalTxnAmountDue { get; set; } = string.Empty;

    [JsonProperty("moduleKey")]
    public string ModuleKey { get; set; } = string.Empty;

    [JsonProperty("webURL")]
    public string WebUrl { get; set; } = string.Empty;

    /// <summary>Exemple de champ personnalisé (namespace nsp::).</summary>
    [JsonProperty("nsp::REF_ERP")]
    public string? NspRefErp { get; set; }

    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;

    /// <summary>Client associé à la facture.</summary>
    [JsonProperty("customer")]
    public InvoiceCustomer Customer { get; set; } = new();

    /// <summary>Devise de base et de transaction.</summary>
    [JsonProperty("currency")]
    public InvoiceCurrency Currency { get; set; } = new();

    /// <summary>Lignes de facture. On ne garde que quelques champs clés.</summary>
    [JsonProperty("lines")]
    public List<InvoiceLine> Lines { get; set; } = new();
}

/// <summary>Informations principales du client sur la facture.</summary>
public class InvoiceCustomer
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Montant dû par le client sur cette facture.</summary>
    [JsonProperty("customerDue")]
    public string CustomerDue { get; set; } = string.Empty;

    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;
}

/// <summary>Devise de base et devise de transaction de la facture.</summary>
public class InvoiceCurrency
{
    [JsonProperty("baseCurrency")]
    public string BaseCurrency { get; set; } = string.Empty;

    [JsonProperty("txnCurrency")]
    public string TxnCurrency { get; set; } = string.Empty;
}

/// <summary>
/// Ligne de facture : comptes, montants, dimensions principales (client, lieu).
/// </summary>
public class InvoiceLine
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("lineNumber")]
    public int LineNumber { get; set; }

    [JsonProperty("createdDate")]
    public string CreatedDate { get; set; } = string.Empty;

    [JsonProperty("baseAmount")]
    public string BaseAmount { get; set; } = string.Empty;

    [JsonProperty("txnAmount")]
    public string TxnAmount { get; set; } = string.Empty;

    [JsonProperty("memo")]
    public string? Memo { get; set; }

    /// <summary>Compte de résultat / de vente.</summary>
    [JsonProperty("glAccount")]
    public InvoiceGlAccount GlAccount { get; set; } = new();

    /// <summary>Compte client (compte de contrepartie).</summary>
    [JsonProperty("overrideOffsetGLAccount")]
    public InvoiceOffsetGlAccount OverrideOffsetGlAccount { get; set; } = new();

    /// <summary>Dimensions principales de la ligne (lieu, client).</summary>
    [JsonProperty("dimensions")]
    public InvoiceLineDimensions Dimensions { get; set; } = new();
}

/// <summary>Compte général associé à la ligne.</summary>
public class InvoiceGlAccount
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;
}

/// <summary>Compte de contrepartie (client) associé à la ligne.</summary>
public class InvoiceOffsetGlAccount
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;
}

/// <summary>Dimensions utiles pour le cours : lieu et client.</summary>
public class InvoiceLineDimensions
{
    [JsonProperty("location")]
    public InvoiceLocationDimension Location { get; set; } = new();

    [JsonProperty("customer")]
    public InvoiceCustomerDimension Customer { get; set; } = new();
}

/// <summary>Dimension lieu.</summary>
public class InvoiceLocationDimension
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;
}

/// <summary>Dimension client sur la ligne (peut différer du client de l'en-tête).</summary>
public class InvoiceCustomerDimension
{
    [JsonProperty("key")]
    public string Key { get; set; } = string.Empty;

    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("href")]
    public string Href { get; set; } = string.Empty;
}

