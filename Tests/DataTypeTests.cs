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
    public class DataTypeTests
    {
        DataType dtObject, dtHashtable, dtShort, dtInteger, dtLong, dtString;

        [OneTimeSetUp]
        public void Prepare()
        {
            dtObject = new DataType<object>("object");
            dtHashtable = new DataType<Hashtable>("hashtable", dtObject, () => new Hashtable());
            dtShort = new FixedType<short>("short", new Regex("^[0-9]+$"), s => short.Parse(s));
            dtInteger = new FixedType<int>("integer", new Regex("^[0-9]+$"), s => int.Parse(s), () => -1, dtShort);
            dtLong = new FixedType<long>("long", new Regex("^[0-9]+$"), s => long.Parse(s), null, dtInteger);
            dtString = new FixedType<string>("string", new Regex(".*"), s => s, () => string.Empty);
        }
        
        [Test]
        public void DataTypeIsAssignableFromParent()
        {
            Assert.That(dtHashtable.IsAssignableFrom(dtObject));
        }

        [Test]
        public void DataTypeIsNotAssignableFromChild()
        {
            Assert.That(!dtObject.IsAssignableFrom(dtHashtable));
        }

        [Test]
        public void FixedTypeIsAssignableFromParent()
        {
            Assert.That(dtLong.IsAssignableFrom(dtInteger));
        }

        [Test]
        public void FixedTypeIsNotAssignableFromChild()
        {
            Assert.That(!dtInteger.IsAssignableFrom(dtLong));
        }

        [Test]
        public void FixedTypeIsAssignableFromGrandparent()
        {
            Assert.That(dtLong.IsAssignableFrom(dtShort));
        }

        [Test]
        public void FixedTypeIsNotAssignableFromGrandchild()
        {
            Assert.That(!dtShort.IsAssignableFrom(dtLong));
        }

        [Test]
        public void DataTypeIsNotAssignableFromUnrelatedType()
        {
            Assert.That(!dtString.IsAssignableFrom(dtHashtable));
        }
        
        [Test]
        public void UnspecifiedReferenceTypeDefaultIsNull()
        {
            Assert.That(dtObject.GetDefaultValue() == null);
        }

        [Test]
        public void ReferenceTypeDefaultIsNotNull()
        {
            Assert.That(dtHashtable.GetDefaultValue() != null);
        }

        [Test]
        public void ReferenceTypeDefaultIsCorrectType()
        {
            var defaultVal = dtHashtable.GetDefaultValue();
            Assert.That(defaultVal, Is.TypeOf(typeof(Hashtable)));
        }

        [Test]
        public void ReferenceTypeDefaultInstancesDiffer()
        {
            var defaultVal1 = dtHashtable.GetDefaultValue();
            var defaultVal2 = dtHashtable.GetDefaultValue();
            Assert.That(defaultVal1 != defaultVal2);
        }

        [Test]
        public void ValueTypeDefaultIsNotNull()
        {
            Assert.That(dtLong.GetDefaultValue() != null);
        }

        [Test]
        public void ValueTypeUsesSpecifiedDefault()
        {
            Assert.That(dtInteger.GetDefaultValue(), Is.EqualTo(-1));
        }

        [Test]
        public void ValidateValidValue()
        {
            Assert.That(dtInteger.Validation.IsMatch("2"));
        }

        [Test]
        public void ValidateInvalidValue()
        {
            Assert.That(!dtInteger.Validation.IsMatch("-2"));
        }

        [Test]
        public void ValidateNullValue()
        {
            Assert.That(() => dtInteger.Validation.IsMatch(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ParseValidValue()
        {
            Assert.That(((IDeserializable)dtInteger).Deserialize("2"), Is.EqualTo(2));
        }

        [Test]
        public void ParseInvalidValue()
        {
            Assert.That(() => ((IDeserializable)dtInteger).Deserialize("blah"), Throws.TypeOf<FormatException>());
        }
    }
}
