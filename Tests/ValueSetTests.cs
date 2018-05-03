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
    public class ValueSetTests
    {
        DataType<object> dtObject;
        DataType<short> dtShort;
        ValueSet values;

        ValueKey<short> testShort;
        ValueKey<object> testObject;

        [OneTimeSetUp]
        public void Prepare()
        {
            dtObject = new DataType<object>("object", Color.FromKnownColor(KnownColor.Red));
            dtShort = new FixedType<short>("short", Color.FromKnownColor(KnownColor.Yellow), new Regex("^[0-9]+$"), s => short.Parse(s));

            testShort = new ValueKey<short>("testShort", dtShort);
            testObject = new ValueKey<object>("testObject", dtObject);
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

            Assert.That(() => values.Get(testShort), Throws.TypeOf<CursiveRunException>());
        }


        [Test]
        public void CloneHasValues()
        {
            values = new ValueSet();

            short val = 27;
            values.Set(testShort, val);

            var clone = values.Clone();

            Assert.That(clone.Get(testShort), Is.EqualTo(27));
        }


        [Test]
        public void CloneIsShallow()
        {
            values = new ValueSet();

            object val = new object();
            values.Set(testObject, val);

            var clone = values.Clone();

            Assert.That(clone.Get(testObject), Is.Not.Null);
            Assert.That(clone.Get(testObject), Is.EqualTo(val));
        }
    }
}
