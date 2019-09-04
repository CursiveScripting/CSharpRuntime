using Cursive;
using Newtonsoft.Json;
using Xunit;

namespace Tests
{
    public class ProcessSavingTests
    {
        [Theory]
        [InlineData("Tests.IntegerProcesses.addOne.json")]
        public void LoadAndSave(string resourceName)
        {
            string sourceProcessJson = ProcessLoadingTests.ReadJsonResource(resourceName);
            var workspace = new IntegerWorkspace();
            var success = workspace.LoadUserProcesses(sourceProcessJson, out _);

            Assert.True(success);

            var processJson = JsonConvert.SerializeObject(workspace.UserProcesses, Formatting.Indented);

            Assert.NotNull(processJson);

            var validationErrors = Schemas.Processes.Value.Validate(processJson);

            Assert.Empty(validationErrors);

            Assert.Equal(processJson, sourceProcessJson);
        }
    }
}
