namespace intacct_rest_api.Models.BillLineUpdate;

/// <summary>
/// Corps minimal pour modifier une ligne de bill (PATCH /objects/accounts-payable/bill-line/{key}).
/// Seuls les champs renseignés sont envoyés ; les null sont ignorés à la sérialisation (service).
/// </summary>
public class BillLineUpdate
{
    public GlAccountRef? glAccount { get; set; }
    public string? txnAmount { get; set; }
    public string? memo { get; set; }
    public BillLineDimensions? dimensions { get; set; }
}

/// <summary>
/// Référence compte général (un seul identifiant : id).
/// </summary>
public class GlAccountRef
{
    public string? id { get; set; }
}

/// <summary>
/// Dimensions de la ligne : 2 dimensions (department, location).
/// </summary>
public class BillLineDimensions
{
    public KeyIdRef? department { get; set; }
    public KeyIdRef? location { get; set; }
}

/// <summary>
/// Référence dimension (un seul identifiant : id).
/// </summary>
public class KeyIdRef
{
    public string? id { get; set; }
}
