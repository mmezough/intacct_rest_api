namespace intacct_rest_api.Models;

/// <summary>
/// Référence par identifiant : objet JSON { "id": "..." } utilisé pour customer, glAccount, dimensions (department, location, etc.).
/// </summary>
public class IdRef
{
    public string id { get; set; } = string.Empty;
}
