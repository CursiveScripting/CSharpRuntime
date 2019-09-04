using Cursive;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class SystemProcessTests
    {
        [Theory]
        [InlineData(1, 2, "less")]
        [InlineData(5, 1, "greater")]
        [InlineData(7, 7, "equal")]
        public async Task TestReturnPath(int val1, int val2, string returnPath)
        {
            var workspace = new IntegerWorkspace();

            var inputs = new ValueSet();
            inputs.Set(workspace.CompareInput1, val1);
            inputs.Set(workspace.CompareInput2, val2);

            var result = await workspace.Compare.Run(inputs);

            Assert.Equal(returnPath, result.ReturnPath);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(0, 0, 0)]
        [InlineData(13, 7, 20)]
        public async Task TestOutputs(int val1, int val2, int sum)
        {
            var workspace = new IntegerWorkspace();

            var inputs = new ValueSet();
            inputs.Set(workspace.AddInput1, val1);
            inputs.Set(workspace.AddInput2, val2);

            var result = await workspace.Add.Run(inputs);

            ValueSet outputs = result.Outputs;
            var outValue = outputs.Get(workspace.AddOutput);

            Assert.Null(result.ReturnPath);
            Assert.Equal(sum, outValue);
        }
    }
}
