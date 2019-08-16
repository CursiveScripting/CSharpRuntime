using Cursive;
using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Tests
{
    [TestFixture]
    public class ValueSetTests
    {
        DataType<object> dtObject;
        DataType<short> dtShort;
        ValueSet values;

        Parameter<short> testShort;

        [OneTimeSetUp]
        public void Prepare()
        {
            dtObject = new DataType<object>("object", Color.FromKnownColor(KnownColor.Red));
            dtShort = new FixedType<short>("short", Color.FromKnownColor(KnownColor.Yellow), new Regex("^[0-9]+$"), s => short.Parse(s));

            testShort = new Parameter<short>("testShort", dtShort);
        }
        
        [Test]
        public void CanRetrieveSetValue()
        {
            values = new ValueSet();

            short val = 27;
            values.Set(testShort, val);
            
            Assert.That(values.Get(testShort), Is.EqualTo(27));
        }

        [Test]
        public void FailToGetUnsetValue()
        {
            values = new ValueSet();

            Assert.That(() => values.Get(testShort), Throws.TypeOf<KeyNotFoundException>());
        }
    }
}
