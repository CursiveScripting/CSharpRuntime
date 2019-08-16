using Cursive;
using NUnit.Framework;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class SystemProcessTests
    {
        static DataType<int> number;
        private static Parameter<int> value1;
        private static Parameter<int> value2;
        private static Parameter<int> value3;

        public static SystemProcess IsEqual, Add;

        [OneTimeSetUp]
        public void Prepare()
        {
            number = new FixedType<int>("number", Color.FromKnownColor(KnownColor.Red), new Regex("[0-9]+"), s => int.Parse(s));
            value1 = new Parameter<int>("value1", number);
            value2 = new Parameter<int>("value2", number);
            value3 = new Parameter<int>("value3", number);

            IsEqual = new SystemProcess(
                "Is equal",
                "Test to see if two values are equal.",
                (ValueSet inputs) =>
                {
                    return Response.SyncTask(inputs.Get(value1) == inputs.Get(value2) ? "yes" : "no");
                },
                new Parameter[] { value1, value2 },
                null,
                new string[] { "yes", "no" }
            );

            Add = new SystemProcess(
                "Add",
                "Test to see if two values are equal.",
                (ValueSet inputs) =>
                {
                    var outputs = new ValueSet();
                    int i1 = inputs.Get(value1);
                    int i2 = inputs.Get(value2);
                    outputs.Set(value3, i1 + i2);
                    return Response.SyncTask(outputs);
                },
                new Parameter[] { value1, value2 },
                new Parameter[] { value3 },
                null
            );
        }

        [Test]
        public async Task TestReturnPath()
        {
            var inputs = new ValueSet();
            inputs.Set(value1, 1);
            inputs.Set(value2, 2);

            Assert.That((await IsEqual.Run(inputs)).ReturnPath, Is.EqualTo("no"));
        }
        
        [Test]
        public async Task TestOutputs()
        {
            var inputs = new ValueSet();
            inputs.Set(value1, 1);
            inputs.Set(value2, 2);
            
            ValueSet outputs = (await Add.Run(inputs)).Outputs;

            Assert.That(outputs.Get(value3), Is.EqualTo(3));
        }
    }
}
