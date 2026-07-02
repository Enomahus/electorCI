namespace Application.FluentValidation;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class ValidationDisplayNameAttribute(string displayName) : Attribute
{
    public string DisplayName { get; } = displayName;
}
