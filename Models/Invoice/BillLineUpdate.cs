using intacct_rest_api.Models;

namespace intacct_rest_api.Models.BillLineUpdate;

/// <summary>
/// Corps minimal pour modifier une ligne de bill (PATCH /objects/accounts-payable/bill-line/{key}).
/// Seuls les champs renseignés sont envoyés ; les null sont ignorés à la sérialisation (service).
/// </summary>
public class BillLineUpdate
{
    public IdRef? glAccount { get; set; }
    public string? txnAmount { get; set; }
    public string? memo { get; set; }
    public BillLineDimensions? dimensions { get; set; }
}

/// <summary>
/// Dimensions de la ligne : 2 dimensions (department, location).
/// </summary>
public class BillLineDimensions
{
    public IdRef? department { get; set; }
    public IdRef? location { get; set; }
}
