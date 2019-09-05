using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ProcessRunningTests
    {
        [Theory]
        [InlineData("Tests.IntegerProcesses.addOne.json", 1, 2)]
        [InlineData("Tests.IntegerProcesses.thresholdCheck.json", 2, 4)]
        [InlineData("Tests.IntegerProcesses.thresholdCheck.json", 3, 3)]
        [InlineData("Tests.IntegerProcesses.thresholdCheck.json", 6, 5)]
        public async Task RunProcess(string resourceName, int inValue, int expectedResult)
        {
            string processJson = ProcessLoadingTests.ReadJsonResource(resourceName);
            var workspace = new IntegerWorkspace();
            var success = workspace.LoadUserProcesses(processJson, false, out _);

            Assert.True(success);

            var result = await workspace.ModifyNumber(inValue);

            Assert.Equal(expectedResult, result);
        }
    }
}
