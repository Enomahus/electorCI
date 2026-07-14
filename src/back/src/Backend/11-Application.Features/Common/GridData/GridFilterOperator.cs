namespace Application.Features.Common.GridData
{
    /// <summary>
    /// Opérateur appliqué à un filtre de colonne de grille.
    /// </summary>
    public enum GridFilterOperator
    {
        Equals = 0,
        NotEquals,
        Contains,
        StartsWith,
        EndsWith,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
    }
}
