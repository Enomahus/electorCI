using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Application.Features.Common.GridData
{
    /// <summary>
    /// Applique une <see cref="GridDataQuery"/> (filtrage, recherche globale, tri, pagination)
    /// sur n'importe quelle source <see cref="IQueryable{T}"/>.
    /// Les champs sont résolus par réflexion à partir de leur nom, ce qui rend la
    /// spécification réutilisable pour toutes les grilles sans code spécifique.
    /// </summary>
    public static class GridDataExtensions
    {
        private static readonly MethodInfo StringContains = typeof(string).GetMethod(
            nameof(string.Contains),
            [typeof(string)]
        )!;
        private static readonly MethodInfo StringStartsWith = typeof(string).GetMethod(
            nameof(string.StartsWith),
            [typeof(string)]
        )!;
        private static readonly MethodInfo StringEndsWith = typeof(string).GetMethod(
            nameof(string.EndsWith),
            [typeof(string)]
        )!;
        private static readonly MethodInfo StringToLower = typeof(string).GetMethod(
            nameof(string.ToLower),
            Type.EmptyTypes
        )!;

        /// <summary>
        /// Applique le filtrage, la recherche globale, le tri puis la pagination.
        /// Le total est calculé après filtrage/recherche mais avant pagination.
        /// </summary>
        public static GridDataResponse<T> ApplyGrid<T>(this IQueryable<T> source, GridDataQuery query)
        {
            // 1. Filtres par colonne (ET logique)
            foreach (var filter in query.Filters ?? [])
            {
                var predicate = BuildFilter<T>(filter);
                if (predicate != null)
                {
                    source = source.Where(predicate);
                }
            }

            // 2. Recherche globale (OU logique sur les champs texte)
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var predicate = BuildSearch<T>(query.Search!, query.SearchFields);
                if (predicate != null)
                {
                    source = source.Where(predicate);
                }
            }

            // 3. Total (avant pagination)
            var total = source.Count();

            // 4. Tri multi-colonnes
            if (query.Sorts is { Count: > 0 })
            {
                for (var i = 0; i < query.Sorts.Count; i++)
                {
                    var sort = query.Sorts[i];
                    var descending = sort.Direction == GridSortDirection.Descending;
                    source = ApplyOrderStep(source, sort.Field, descending, first: i == 0);
                }
            }

            // 5. Pagination
            if (query.Skip is > 0)
            {
                source = source.Skip(query.Skip.Value);
            }
            if (query.Take is > 0)
            {
                source = source.Take(query.Take.Value);
            }

            return new GridDataResponse<T> { Data = source.ToList(), Total = total };
        }

        private static Expression<Func<T, bool>>? BuildFilter<T>(GridFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.Field))
            {
                return null;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var member = BuildMemberAccess(parameter, filter.Field, out var memberType);
            var underlyingType = Nullable.GetUnderlyingType(memberType) ?? memberType;

            Expression predicate;

            var isStringOperator =
                filter.Operator
                    is GridFilterOperator.Contains
                        or GridFilterOperator.StartsWith
                        or GridFilterOperator.EndsWith
                || (
                    memberType == typeof(string)
                    && filter.Operator is GridFilterOperator.Equals or GridFilterOperator.NotEquals
                );

            if (isStringOperator)
            {
                if (memberType != typeof(string))
                {
                    // Opérateur texte sur un champ non-texte : filtre ignoré.
                    return null;
                }

                var loweredValue = Expression.Constant(filter.Value?.ToLower() ?? string.Empty);
                var loweredMember = Expression.Call(member, StringToLower);
                Expression comparison = filter.Operator switch
                {
                    GridFilterOperator.Contains => Expression.Call(loweredMember, StringContains, loweredValue),
                    GridFilterOperator.StartsWith => Expression.Call(loweredMember, StringStartsWith, loweredValue),
                    GridFilterOperator.EndsWith => Expression.Call(loweredMember, StringEndsWith, loweredValue),
                    GridFilterOperator.NotEquals => Expression.NotEqual(loweredMember, loweredValue),
                    _ => Expression.Equal(loweredMember, loweredValue),
                };

                // Protection contre les valeurs nulles (LINQ to Objects).
                var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                predicate =
                    filter.Operator == GridFilterOperator.NotEquals
                        ? comparison // une valeur nulle est bien "différente"
                        : Expression.AndAlso(notNull, comparison);
            }
            else
            {
                var value = ConvertValue(filter.Value, underlyingType);
                var constant = Expression.Constant(value, memberType);
                predicate = filter.Operator switch
                {
                    GridFilterOperator.Equals => Expression.Equal(member, constant),
                    GridFilterOperator.NotEquals => Expression.NotEqual(member, constant),
                    GridFilterOperator.GreaterThan => Expression.GreaterThan(member, constant),
                    GridFilterOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(member, constant),
                    GridFilterOperator.LessThan => Expression.LessThan(member, constant),
                    GridFilterOperator.LessThanOrEqual => Expression.LessThanOrEqual(member, constant),
                    _ => Expression.Equal(member, constant),
                };
            }

            return Expression.Lambda<Func<T, bool>>(predicate, parameter);
        }

        private static Expression<Func<T, bool>>? BuildSearch<T>(
            string term,
            IEnumerable<string>? searchFields
        )
        {
            var fields =
                searchFields?.Where(f => !string.IsNullOrWhiteSpace(f)).ToList()
                ?? typeof(T)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.PropertyType == typeof(string))
                    .Select(p => p.Name)
                    .ToList();

            if (fields.Count == 0)
            {
                return null;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var loweredTerm = Expression.Constant(term.ToLower());
            Expression? body = null;

            foreach (var field in fields)
            {
                var member = BuildMemberAccess(parameter, field, out var memberType);
                if (memberType != typeof(string))
                {
                    continue;
                }

                var loweredMember = Expression.Call(member, StringToLower);
                var contains = Expression.Call(loweredMember, StringContains, loweredTerm);
                var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
                var expression = Expression.AndAlso(notNull, contains);
                body = body == null ? expression : Expression.OrElse(body, expression);
            }

            return body == null ? null : Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        private static IQueryable<T> ApplyOrderStep<T>(
            IQueryable<T> source,
            string field,
            bool descending,
            bool first
        )
        {
            if (string.IsNullOrWhiteSpace(field))
            {
                return source;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var member = BuildMemberAccess(parameter, field, out var keyType);
            var keySelector = Expression.Lambda(member, parameter);

            var methodName = (first, descending) switch
            {
                (true, false) => nameof(Queryable.OrderBy),
                (true, true) => nameof(Queryable.OrderByDescending),
                (false, false) => nameof(Queryable.ThenBy),
                (false, true) => nameof(Queryable.ThenByDescending),
            };

            var method = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), keyType);

            return (IQueryable<T>)method.Invoke(null, [source, keySelector])!;
        }

        /// <summary>
        /// Résout un accès à une propriété par son nom, avec support des chemins imbriqués (ex: "District.Wording").
        /// </summary>
        private static Expression BuildMemberAccess(
            Expression parameter,
            string propertyPath,
            out Type memberType
        )
        {
            Expression body = parameter;
            foreach (var name in propertyPath.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                var property =
                    body.Type.GetProperty(
                        name,
                        BindingFlags.Public
                            | BindingFlags.Instance
                            | BindingFlags.IgnoreCase
                    )
                    ?? throw new ArgumentException(
                        $"Le champ '{propertyPath}' est introuvable sur le type '{body.Type.Name}'."
                    );
                body = Expression.Property(body, property);
            }

            memberType = body.Type;
            return body;
        }

        private static object? ConvertValue(string? raw, Type targetType)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return null;
            }

            if (targetType == typeof(string))
            {
                return raw;
            }
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, raw, ignoreCase: true);
            }
            if (targetType == typeof(Guid))
            {
                return Guid.Parse(raw);
            }
            if (targetType == typeof(bool))
            {
                return bool.Parse(raw);
            }
            if (targetType == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(raw, CultureInfo.InvariantCulture);
            }
            if (targetType == typeof(DateTime))
            {
                return DateTime.Parse(raw, CultureInfo.InvariantCulture);
            }

            return Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
        }
    }
}
