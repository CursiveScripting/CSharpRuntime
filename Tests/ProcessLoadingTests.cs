using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

namespace Tests
{
    public class ProcessLoadingTests
    {
        [Theory]
        [InlineData("Tests.IntegerProcesses.addOne.json")]
        public void LoadErrorFree(string resourceName)
        {
            string processJson = ReadJsonResource(resourceName);
            var workspace = new IntegerWorkspace();
            var success = workspace.LoadUserProcesses(processJson, true, out List<string> errors);

            Assert.Null(errors);
            Assert.True(success);
        }

        public static string ReadJsonResource(string resourceName)
        {
            string processJson;
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                processJson = reader.ReadToEnd();
            }

            return processJson;
        }
    }
}
