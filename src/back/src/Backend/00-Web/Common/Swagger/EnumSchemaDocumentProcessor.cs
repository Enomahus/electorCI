using System.Text.Json;
using NJsonSchema;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Web.Common.Swagger
{
    public class EnumSchemaDocumentProcessor : IDocumentProcessor
    {
        public void Process(DocumentProcessorContext context)
        {
            //AddEnum<ModuleType>(context);
        }

        private static void AddEnum<T>(DocumentProcessorContext context)
            where T : struct, Enum
        {
            var schema = new JsonSchema { Type = JsonObjectType.String };
            foreach (var item in Enum.GetValues<T>())
            {
                schema.Enumeration.Add(JsonNamingPolicy.CamelCase.ConvertName(item.ToString()));
            }

            context.Document.Definitions[typeof(T).Name] = schema;
        }
    }
}
