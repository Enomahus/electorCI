namespace Application.Features.Common.GridData
{
    /// <summary>
    /// Décrit un tri sur un champ (colonne) de la grille.
    /// </summary>
    public class GridSort
    {
        /// <summary>Nom du champ (propriété) à trier. Supporte les chemins imbriqués (ex: "District.Wording").</summary>
        public required string Field { get; set; }

        public GridSortDirection Direction { get; set; } = GridSortDirection.Ascending;
    }
}
