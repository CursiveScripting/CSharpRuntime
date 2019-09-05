using Manatee.Json;
using Manatee.Json.Schema;
using Manatee.Json.Serialization;
using System;
using System.IO;

namespace Cursive
{
    public static class Schemas
    {
        public static readonly Lazy<JsonSchema> Workspace = new Lazy<JsonSchema>(() => LoadSchema("Cursive.workspace.json"));

        public static readonly Lazy<JsonSchema> Processes = new Lazy<JsonSchema>(() => LoadSchema("Cursive.processes.json"));

        private static JsonSchema LoadSchema(string schemaResourceName)
        {
            string schemaJson;
            using (Stream stream = typeof(Schemas).Assembly.GetManifestResourceStream(schemaResourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                schemaJson = reader.ReadToEnd();
            }

            var jsonData = JsonValue.Parse(schemaJson);

            var serializer = new JsonSerializer();

            return serializer.Deserialize<JsonSchema>(jsonData);
        }

        public static SchemaValidationResults Validate(Lazy<JsonSchema> schema, string json)
        {
            var jsonData = JsonValue.Parse(json);
            return schema.Value.Validate(jsonData);
        }
    }
}
