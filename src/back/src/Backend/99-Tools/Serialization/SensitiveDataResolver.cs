using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Tools.Serialization;

public class SensitiveDataResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization
    )
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (member is PropertyInfo prop)
        {
            var isSensitiveData = Attribute.IsDefined(prop, typeof(SensitiveDataAttribute));

            if (isSensitiveData)
                property.ValueProvider = new StringValueProvider("SensitiveData");
        }
        return property;
    }
}
