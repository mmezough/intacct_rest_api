namespace intacct_rest_api.Models;

/// <summary>Helper pour construire un filtre (un dictionnaire { "$op": { "champ": valeur } }) sans logique de sérialisation.</summary>
public static class Filter
{
    public static Dictionary<string, object> Equal(string champ, object valeur) =>
        CreateFilter("$eq", champ, valeur);

    public static Dictionary<string, object> NotEqual(string champ, object valeur) =>
        CreateFilter("$ne", champ, valeur);

    public static Dictionary<string, object> LessThan(string champ, object valeur) =>
        CreateFilter("$lt", champ, valeur);

    public static Dictionary<string, object> LessThanOrEqual(string champ, object valeur) =>
        CreateFilter("$lte", champ, valeur);

    public static Dictionary<string, object> GreaterThan(string champ, object valeur) =>
        CreateFilter("$gt", champ, valeur);

    public static Dictionary<string, object> GreaterThanOrEqual(string champ, object valeur) =>
        CreateFilter("$gte", champ, valeur);

    public static Dictionary<string, object> In(string champ, IEnumerable<object> valeurs) =>
        CreateFilter("$in", champ, valeurs.ToList());

    public static Dictionary<string, object> NotIn(string champ, IEnumerable<object> valeurs) =>
        CreateFilter("$notIn", champ, valeurs.ToList());

    /// <summary>Between attend une valeur = tableau de 2 éléments [min, max].</summary>
    public static Dictionary<string, object> Between(string champ, object valeur1, object valeur2) =>
        CreateFilter("$between", champ, new object[] { valeur1, valeur2 });

    /// <summary>NotBetween attend une valeur = tableau de 2 éléments [min, max].</summary>
    public static Dictionary<string, object> NotBetween(string champ, object valeur1, object valeur2) =>
        CreateFilter("$notBetween", champ, new object[] { valeur1, valeur2 });

    public static Dictionary<string, object> Contains(string champ, object valeur) =>
        CreateFilter("$contains", champ, valeur);

    public static Dictionary<string, object> NotContains(string champ, object valeur) =>
        CreateFilter("$notContains", champ, valeur);

    public static Dictionary<string, object> StartsWith(string champ, object valeur) =>
        CreateFilter("$startsWith", champ, valeur);

    public static Dictionary<string, object> NotStartsWith(string champ, object valeur) =>
        CreateFilter("$notStartsWith", champ, valeur);

    public static Dictionary<string, object> EndsWith(string champ, object valeur) =>
        CreateFilter("$endsWith", champ, valeur);

    public static Dictionary<string, object> NotEndsWith(string champ, object valeur) =>
        CreateFilter("$notEndsWith", champ, valeur);

    /// <summary>Construit une entrée de filtre : { "$op": { "champ": valeur } }.</summary>
    private static Dictionary<string, object> CreateFilter(string op, string champ, object valeur) =>
        new() { [op] = new Dictionary<string, object> { [champ] = valeur } };
}
