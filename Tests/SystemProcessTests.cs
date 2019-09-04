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
            var compareProcess = new IntegerWorkspace().Compare;

            var inputs = new ValueSet();
            inputs.Set(compareProcess.Inputs[0] as Parameter<int>, val1);
            inputs.Set(compareProcess.Inputs[1] as Parameter<int>, val2);

            var result = await compareProcess.Run(inputs);

            Assert.Equal(returnPath, result.ReturnPath);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(0, 0, 0)]
        [InlineData(13, 7, 20)]
        public async Task TestOutputs(int val1, int val2, int sum)
        {
            var addProcess = new IntegerWorkspace().Add;

            var inputs = new ValueSet();
            inputs.Set(addProcess.Inputs[0] as Parameter<int>, val1);
            inputs.Set(addProcess.Inputs[1] as Parameter<int>, val2);

            var result = await addProcess.Run(inputs);

            ValueSet outputs = result.Outputs;
            var outValue = outputs.Get(addProcess.Outputs[0] as Parameter<int>);

            Assert.Equal(sum, outValue);
        }
    }
}
