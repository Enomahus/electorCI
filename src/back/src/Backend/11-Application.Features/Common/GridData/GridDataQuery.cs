namespace Application.Features.Common.GridData
{
    /// <summary>
    /// Spécification réutilisable d'une requête de grille : filtrage, recherche globale,
    /// tri multi-colonnes et pagination.
    /// Chaque requête de grille (ex: <c>GetUsersQuery</c>) hérite de cette classe puis
    /// implémente <c>IRequest&lt;Result&lt;GridDataResponse&lt;T&gt;&gt;&gt;</c>.
    /// L'application concrète des opérations se fait via <see cref="GridDataExtensions.ApplyGrid"/>.
    /// </summary>
    public abstract class GridDataQuery
    {
        /// <summary>Filtres par colonne (combinés en ET logique).</summary>
        public List<GridFilter> Filters { get; set; } = [];

        /// <summary>Tris appliqués dans l'ordre (le premier est le tri primaire).</summary>
        public List<GridSort> Sorts { get; set; } = [];

        /// <summary>
        /// Terme de recherche globale appliqué (en OU logique) sur les champs de <see cref="SearchFields"/>.
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Champs texte ciblés par la recherche globale. Si null/vide, tous les champs de type
        /// <c>string</c> du modèle de réponse sont utilisés.
        /// </summary>
        public List<string>? SearchFields { get; set; }

        /// <summary>Nombre d'éléments à ignorer (pagination).</summary>
        public int? Skip { get; set; }

        /// <summary>Nombre d'éléments à retourner (pagination).</summary>
        public int? Take { get; set; }
    }
}
