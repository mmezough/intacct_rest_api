using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace intacct_rest_api.Models;

/// <summary>
/// Réponse détaillée d'une facture (GET /invoice/{key}). On ne mappe que "ia::result".
/// </summary>
public class InvoiceDetailResponse
{
    [JsonProperty("ia::result")]
    public InvoiceHeader Invoice { get; set; } = new();
}

/// <summary>
/// En-tête de facture : identifiants, dates, montants, client, devise et lignes.
/// </summary>
public class InvoiceHeader
{
    public string id { get; set; } = string.Empty;
    public string key { get; set; } = string.Empty;
    public string recordType { get; set; } = string.Empty;
    public string invoiceNumber { get; set; } = string.Empty;
    public string state { get; set; } = string.Empty;
    public string invoiceDate { get; set; } = string.Empty;
    public string dueDate { get; set; } = string.Empty;
    public string totalBaseAmount { get; set; } = string.Empty;
    public string totalBaseAmountDue { get; set; } = string.Empty;
    public string totalTxnAmount { get; set; } = string.Empty;
    public string totalTxnAmountDue { get; set; } = string.Empty;

    /// <summary>Client associé à la facture.</summary>
    public InvoiceCustomer customer { get; set; } = new();

    /// <summary>Devise de base et de transaction.</summary>
    public InvoiceCurrency currency { get; set; } = new();

    /// <summary>Lignes de facture.</summary>
    public List<InvoiceLine> lines { get; set; } = new();

    /// <summary>
    /// Champs personnalisés (ex. préfixés par nsp::) et autres propriétés non mappées.
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? CustomFields { get; set; }
}

/// <summary>Informations principales du client sur la facture.</summary>
public class InvoiceCustomer
{
    public string id { get; set; } = string.Empty;
    public string key { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;

    /// <summary>Montant dû par le client sur cette facture.</summary>
    public string customerDue { get; set; } = string.Empty;

    public string href { get; set; } = string.Empty;
}

/// <summary>Devise de base et devise de transaction de la facture.</summary>
public class InvoiceCurrency
{
    public string baseCurrency { get; set; } = string.Empty;
    public string txnCurrency { get; set; } = string.Empty;
}

/// <summary>
/// Ligne de facture : comptes, montants, dimensions principales (client, lieu).
/// </summary>
public class InvoiceLine
{
    public string id { get; set; } = string.Empty;
    public string key { get; set; } = string.Empty;
    public int lineNumber { get; set; }
    public string createdDate { get; set; } = string.Empty;
    public string baseAmount { get; set; } = string.Empty;
    public string txnAmount { get; set; } = string.Empty;
    public string? memo { get; set; }

    /// <summary>Compte de résultat / de vente.</summary>
    public InvoiceGlAccount glAccount { get; set; } = new();

    /// <summary>Dimensions principales de la ligne (lieu, client).</summary>
    public InvoiceLineDimensions dimensions { get; set; } = new();

    /// <summary>Champs personnalisés de la ligne (ex. nsp::xxx).</summary>
    [JsonExtensionData]
    public IDictionary<string, JToken>? CustomFields { get; set; }
}

/// <summary>Compte général associé à la ligne.</summary>
public class InvoiceGlAccount
{
    public string key { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
}

/// <summary>Dimensions utiles pour le cours : lieu et client.</summary>
public class InvoiceLineDimensions
{
    public InvoiceLocationDimension location { get; set; } = new();
    public InvoiceCustomerDimension customer { get; set; } = new();
}

/// <summary>Dimension lieu.</summary>
public class InvoiceLocationDimension
{
    public string key { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
}

/// <summary>Dimension client sur la ligne.</summary>
public class InvoiceCustomerDimension
{
    public string key { get; set; } = string.Empty;
    public string id { get; set; } = string.Empty;
    public string name { get; set; } = string.Empty;
}
