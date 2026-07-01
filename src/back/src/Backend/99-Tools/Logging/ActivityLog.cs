using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Tools.Serialization;

namespace Tools.Logging;

[ExcludeFromCodeCoverage]
public class ActivityLog(Activity? activity) : IDisposable
{
    public ActivityLog AddSerializedParameters(params object[] parameters)
    {
        if (parameters?.Length > 0)
        {
            var settings = new JsonSerializerSettings() { ContractResolver = new SensitiveDataResolver() };
            var parametersJson = parameters.Select(p => JsonConvert.SerializeObject(p, settings)).ToList();
            activity = activity?.AddTag("parameters", string.Join(", ", parametersJson));
        }

        return this;
    }

    public ActivityLog AddParameter(string tag, string parameter)
    {
        activity = activity?.AddTag(tag, parameter);
        return this;
    }

    public ActivityLog AddParameter<T, T_Property>(T obj, Expression<Func<T, T_Property>> propertySelector)
    {
        if (propertySelector.Body is MemberExpression memberExpression)
        {
            var tag = memberExpression.Member.Name;
            var parameter = propertySelector.Compile().Invoke(obj)?.ToString();

            AddParameter(tag, parameter ?? "null");
        }
        else
        {
            throw new ArgumentException("propertySelector must be a MemberExpression");
        }

        return this;
    }

    public ActivityLog AddParameters<T, T_Property>(
        T obj,
        Expression<Func<T, IEnumerable<T_Property>>> propertySelector
    )
    {
        if (propertySelector.Body is MemberExpression memberExpression)
        {
            var tag = memberExpression.Member.Name;
            var parameters = propertySelector.Compile().Invoke(obj);
            if (parameters is not null)
            {
                foreach (var parameter in parameters)
                {
                    AddParameter(tag, parameter?.ToString() ?? "null");
                }
            }
        }
        else
        {
            throw new ArgumentException("propertySelector must be a MemberExpression");
        }
        return this;
    }

    public void AddEvent(string message)
    {
        activity?.AddEvent(new ActivityEvent(message));
    }

    public void SetException(Exception ex)
    {
        activity?.AddException(ex);
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    }

    public void AddTag(string key, string value)
    {
        activity?.AddTag(key, value);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        activity?.Dispose();
    }
}
