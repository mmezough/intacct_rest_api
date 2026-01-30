namespace intacct_rest_api.Models;

/// <summary>
/// Construit une expression de filtre à partir de références aux filtres (indices 0-based).
/// Valide que tous les indices existent dans la liste des filtres.
/// Produit une chaîne 1-based pour l'API, ex. "(1 and 2) or 3".
/// </summary>
/// <example>
/// Utilisation :
///   var filters = new List&lt;...&gt; { Filter.Equal("status", "active"), Filter.Equal("billingType", "openItem"), ... };
///   var expr = FilterExpression.Or(
///       FilterExpression.And(FilterExpression.Ref(0), FilterExpression.Ref(1)),
///       FilterExpression.Ref(2));
///   request.FilterExpression = FilterExpression.Build(filters, expr);  // "(1 and 2) or 3"
/// </example>
public static class FilterExpression
{
    /// <summary>Référence au filtre à l'index donné (0-based).</summary>
    public static FilterExpressionNode Ref(int zeroBasedIndex) =>
        new RefNode(zeroBasedIndex);

    /// <summary>Combine deux expressions avec "and".</summary>
    public static FilterExpressionNode And(FilterExpressionNode left, FilterExpressionNode right) =>
        new AndNode(left, right);

    /// <summary>Combine deux expressions avec "or".</summary>
    public static FilterExpressionNode Or(FilterExpressionNode left, FilterExpressionNode right) =>
        new OrNode(left, right);

    /// <summary>
    /// Valide que tous les indices de l'expression sont dans la liste des filtres,
    /// puis retourne la chaîne 1-based pour l'API.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Un index est hors plage.</exception>
    public static string Build(IList<Dictionary<string, object>> filters, FilterExpressionNode node)
    {
        var indices = node.CollectIndices().ToList();
        var count = filters.Count;
        foreach (var i in indices)
        {
            if (i < 0 || i >= count)
                throw new ArgumentOutOfRangeException(nameof(filters), $"Index de filtre {i} hors plage (0..{count - 1}).");
        }
        return node.ToApiString();
    }
}

/// <summary>Nœud d'une expression (référence, and, or).</summary>
public abstract class FilterExpressionNode
{
    internal abstract IEnumerable<int> CollectIndices();
    internal abstract string ToApiString();
}

internal sealed class RefNode : FilterExpressionNode
{
    private readonly int _index;

    internal RefNode(int index) => _index = index;

    internal override IEnumerable<int> CollectIndices() => new[] { _index };

    internal override string ToApiString() => (_index + 1).ToString();
}

internal sealed class AndNode : FilterExpressionNode
{
    private readonly FilterExpressionNode _left;
    private readonly FilterExpressionNode _right;

    internal AndNode(FilterExpressionNode left, FilterExpressionNode right)
    {
        _left = left;
        _right = right;
    }

    internal override IEnumerable<int> CollectIndices() =>
        _left.CollectIndices().Concat(_right.CollectIndices());

    internal override string ToApiString() =>
        $"{_left.ToApiString()} and {_right.ToApiString()}";
}

internal sealed class OrNode : FilterExpressionNode
{
    private readonly FilterExpressionNode _left;
    private readonly FilterExpressionNode _right;

    internal OrNode(FilterExpressionNode left, FilterExpressionNode right)
    {
        _left = left;
        _right = right;
    }

    internal override IEnumerable<int> CollectIndices() =>
        _left.CollectIndices().Concat(_right.CollectIndices());

    internal override string ToApiString()
    {
        var leftStr = _left.ToApiString();
        var rightStr = _right.ToApiString();
        var left = leftStr.Contains(' ') ? $"({leftStr})" : leftStr;
        var right = rightStr.Contains(' ') ? $"({rightStr})" : rightStr;
        return $"{left} or {right}";
    }
}
