using Cursive;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class SystemProcessTests
    {
        [Test]
        public async Task TestReturnPath()
        {
            var compareProcess = new IntegerWorkspace().Compare;

            var inputs = new ValueSet();
            inputs.Set(compareProcess.Inputs[0] as Parameter<int>, 1);
            inputs.Set(compareProcess.Inputs[1] as Parameter<int>, 2);

            var result = await compareProcess.Run(inputs);

            Assert.That(result.ReturnPath, Is.EqualTo("less"));
        }
        
        [Test]
        public async Task TestOutputs()
        {
            var addProcess = new IntegerWorkspace().Compare;

            var inputs = new ValueSet();
            inputs.Set(addProcess.Inputs[0] as Parameter<int>, 1);
            inputs.Set(addProcess.Inputs[1] as Parameter<int>, 2);

            var result = await addProcess.Run(inputs);

            ValueSet outputs = result.Outputs;
            var outValue = inputs.Get(addProcess.Outputs[0] as Parameter<int>);

            Assert.That(outValue, Is.EqualTo(3));
        }
    }
}
