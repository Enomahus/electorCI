using System.Linq.Expressions;
using System.Reflection;

namespace Application.FluentValidation;

public static class CustomDisplayNameResolver
{
    public static string? Resolve(Type type, MemberInfo memberInfo, LambdaExpression expression)
    {
        if (memberInfo != null)
        {
            if (
                memberInfo
                    .GetCustomAttributes(typeof(ValidationDisplayNameAttribute), false)
                    .FirstOrDefault()
                is ValidationDisplayNameAttribute displayNameAttribute
            )
            {
                return displayNameAttribute.DisplayName;
            }

            return memberInfo.Name;
        }

        return null;
    }
}
