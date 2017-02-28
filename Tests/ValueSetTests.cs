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
    public class ValueSetTests
    {
        DataType dtObject, dtShort;
        ValueSet values;

        ValueKey testShort, testShort2, testObject, testHashtable;

        [OneTimeSetUp]
        public void Prepare()
        {
            dtObject = new DataType<object>("object");
            dtShort = new FixedType<short>("short", new Regex("^[0-9]+$"), s => short.Parse(s));

            testShort = new ValueKey("testShort", dtShort);
            testObject = new ValueKey("testObject", dtObject);
        }
        
        [Test]
        public void CanRetrieveSetValue()
        {
            values = new ValueSet();

            short val = 27;
            values[testShort] = val;
            
            Assert.That(values[testShort], Is.EqualTo(27));
        }

        [Test]
        public void FailToGetUnsetValue()
        {
            values = new ValueSet();

            Assert.That(() => values[testShort], Throws.TypeOf<Exception>());
        }


        [Test]
        public void CloneHasValues()
        {
            values = new ValueSet();

            short val = 27;
            values[testShort] = val;

            var clone = values.Clone();

            Assert.That(clone[testShort], Is.EqualTo(27));
        }


        [Test]
        public void CloneIsShallow()
        {
            values = new ValueSet();

            object val = new object();
            values[testObject] = val;

            var clone = values.Clone();

            Assert.That(clone[testObject], Is.Not.Null);
            Assert.That(clone[testObject], Is.EqualTo(val));
        }
    }
}
