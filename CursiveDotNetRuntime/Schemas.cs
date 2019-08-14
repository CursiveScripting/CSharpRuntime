using NJsonSchema;
using System;
using System.IO;
using System.Threading.Tasks;

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

            var task = Task.Run(async () => await JsonSchema.FromJsonAsync(schemaJson));

            task.Wait();

            return task.Result;
        }
    }
}
