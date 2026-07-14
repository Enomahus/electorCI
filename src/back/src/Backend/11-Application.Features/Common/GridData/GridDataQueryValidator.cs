using Application.Models.Errors;
using FluentValidation;

namespace Application.Features.Common.GridData
{
    /// <summary>
    /// Validation réutilisable des paramètres de pagination d'une grille.
    /// Les validateurs concrets (ex: <c>GetUsersQueryValidator</c>) héritent de cette classe.
    /// </summary>
    /// <typeparam name="T">Type concret de la requête de grille.</typeparam>
    public class GridDataQueryValidator<T> : AbstractValidator<T>
        where T : GridDataQuery
    {
        /// <summary>Borne haute de sécurité pour la taille de page.</summary>
        public const int MaxTake = 500;

        public GridDataQueryValidator()
        {
            RuleFor(x => x.Skip)
                .GreaterThanOrEqualTo(0)
                .WithMessage(ValidationErrorCode.PositiveNumber.ToString())
                .When(x => x.Skip.HasValue);

            RuleFor(x => x.Take)
                .InclusiveBetween(1, MaxTake)
                .WithMessage(ValidationErrorCode.PositiveNumber.ToString())
                .When(x => x.Take.HasValue);
        }
    }
}
