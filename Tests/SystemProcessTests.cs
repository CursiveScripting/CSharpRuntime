using Cursive;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class SystemProcessTests
    {
        static DataType<int> number;
        private static ValueKey<int> value1;
        private static ValueKey<int> value2;
        private static ValueKey<int> value3;

        public static SystemProcess IsEqual, Add;

        [OneTimeSetUp]
        public void Prepare()
        {
            number = new FixedType<int>("number", Color.FromKnownColor(KnownColor.Red), new Regex("[0-9]+"), s => int.Parse(s));
            value1 = new ValueKey<int>("value1", number);
            value2 = new ValueKey<int>("value2", number);
            value3 = new ValueKey<int>("value3", number);

            IsEqual = new SystemProcess(
                (ValueSet inputs) =>
                {
                    return Response.Task(inputs.Get(value1) == inputs.Get(value2) ? "yes" : "no");
                },
                "Test to see if two values are equal.",
                new Cursive.ValueKey[] { value1, value2 },
                null,
                new string[] { "yes", "no" }
            );

            Add = new SystemProcess(
                (ValueSet inputs) =>
                {
                    var outputs = new ValueSet();
                    int i1 = inputs.Get(value1);
                    int i2 = inputs.Get(value2);
                    outputs.Set(value3, i1 + i2);
                    return Response.Task(outputs);
                },
                "Test to see if two values are equal.",
                new Cursive.ValueKey[] { value1, value2 },
                new Cursive.ValueKey[] { value3 },
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
