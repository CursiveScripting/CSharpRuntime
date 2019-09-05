using Cursive;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ProcessRunningTests
    {
        [Theory]
        [InlineData("Tests.IntegerProcesses.addOne.json", 1, 2)]
        public async Task RunProcess(string resourceName, int inValue, int expectedResult)
        {
            string processJson = ProcessLoadingTests.ReadJsonResource(resourceName);
            var workspace = new IntegerWorkspace();
            var success = workspace.LoadUserProcesses(processJson, false, out _);

            Assert.True(success);

            var inputs = new ValueSet();
            inputs.Set(workspace.EntryInput, inValue);

            var result = await workspace.EntryProcess.Run(inputs);

            Assert.Null(result.ReturnPath);

            Assert.Equal(expectedResult, result.Outputs.Get(workspace.EntryOutput));
        }
    }
}
