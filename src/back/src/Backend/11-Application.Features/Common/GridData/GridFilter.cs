namespace Application.Features.Common.GridData
{
    /// <summary>
    /// Décrit un filtre appliqué à un champ (colonne) de la grille.
    /// </summary>
    public class GridFilter
    {
        /// <summary>Nom du champ (propriété) à filtrer. Supporte les chemins imbriqués (ex: "District.Wording").</summary>
        public required string Field { get; set; }

        public GridFilterOperator Operator { get; set; } = GridFilterOperator.Equals;

        /// <summary>
        /// Valeur du filtre sous forme de chaîne. Elle est convertie vers le type du champ ciblé
        /// (enum, Guid, bool, nombre, date, ...) au moment de l'application du filtre.
        /// </summary>
        public string? Value { get; set; }
    }
}
