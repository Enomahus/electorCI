using System.Text.Json;
using System.Text.Json.Serialization;

namespace Web.Common.Converters
{
    public sealed class BaseEnumJsonConverter : JsonConverter<Enum>
    {
        private readonly JsonNamingPolicy? _namingPolicy;

        public BaseEnumJsonConverter(JsonNamingPolicy? namingPolicy = null)
        {
            _namingPolicy = namingPolicy;
        }

        public override Enum Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            throw new NotSupportedException("Deserialization of base Enum type is not supported.");
        }

        public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
        {
            var name = value.ToString();
            if (_namingPolicy != null)
            {
                name = _namingPolicy.ConvertName(name);
            }
            writer.WriteStringValue(name);
        }
    }
}
