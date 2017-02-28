using Cursive;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tests
{
    [TestFixture]
    public class SystemProcessTests
    {
        static DataType number;
        private static ValueKey value1;
        private static ValueKey value2;
        private static ValueKey value3;

        public static SystemProcess IsEqual, Add;

        [OneTimeSetUp]
        public void Prepare()
        {
            number = new FixedType<int>("number", new Regex("[0-9]+"), s => int.Parse(s));
            value1 = new ValueKey("value1", number);
            value2 = new ValueKey("value2", number);
            value3 = new ValueKey("value3", number);

            IsEqual = new SystemProcess(
                (ValueSet inputs, out ValueSet outputs) =>
                {
                    outputs = null;
                    return inputs[value1].Equals(inputs[value2]) ? "yes" : "no";
                },
                "Test to see if two values are equal.",
                new Cursive.ValueKey[] { value1, value2 },
                null,
                new string[] { "yes", "no" }
            );

            Add = new SystemProcess(
                (ValueSet inputs, out ValueSet outputs) =>
                {
                    outputs = new ValueSet();
                    int i1 = (int)inputs[value1];
                    int i2 = (int)inputs[value2];
                    outputs[value3] = i1 + i2;
                    return null;
                },
                "Test to see if two values are equal.",
                new Cursive.ValueKey[] { value1, value2 },
                new Cursive.ValueKey[] { value3 },
                null
            );
        }

        [Test]
        public void TestReturnPath()
        {
            var inputs = new ValueSet();
            inputs[value1] = 1;
            inputs[value2] = 2;

            Assert.That(IsEqual.Run(inputs), Is.EqualTo("no"));
        }
        
        [Test]
        public void TestOutputs()
        {
            var inputs = new ValueSet();
            inputs[value1] = 1;
            inputs[value2] = 2;

            ValueSet outputs;
            Add.Run(inputs, out outputs);
            
            Assert.That(outputs[value3], Is.EqualTo(3));
        }
    }
}
