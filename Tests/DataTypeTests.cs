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
    public class DataTypeTests
    {
        DataType dtObject, dtHashtable, dtShort, dtInteger, dtLong, dtString;

        [OneTimeSetUp]
        public void Prepare()
        {
            dtObject = new DataType<object>("object", Color.FromKnownColor(KnownColor.Red));
            dtHashtable = new DataType<Hashtable>("hashtable", Color.FromKnownColor(KnownColor.Orange), dtObject, () => new Hashtable());
            dtShort = new FixedType<short>("short", Color.FromKnownColor(KnownColor.Yellow), new Regex("^[0-9]+$"), s => short.Parse(s));
            dtInteger = new FixedType<int>("integer", Color.FromKnownColor(KnownColor.GreenYellow), new Regex("^[0-9]+$"), s => int.Parse(s), () => -1, dtShort, "Enter a non-negative, whole number");
            dtLong = new FixedType<long>("long", Color.FromKnownColor(KnownColor.Green), new Regex("^[0-9]+$"), s => long.Parse(s), null, dtInteger);
            dtString = new FixedType<string>("string", Color.FromKnownColor(KnownColor.SkyBlue), new Regex(".*"), s => s, () => string.Empty);
        }
        
        [Test]
        public void DataTypeIsAssignableFromParent()
        {
            Assert.That(dtHashtable.IsAssignableTo(dtObject));
        }

        [Test]
        public void DataTypeIsNotAssignableFromChild()
        {
            Assert.That(!dtObject.IsAssignableTo(dtHashtable));
        }

        [Test]
        public void FixedTypeIsAssignableFromParent()
        {
            Assert.That(dtLong.IsAssignableTo(dtInteger));
        }

        [Test]
        public void FixedTypeIsNotAssignableFromChild()
        {
            Assert.That(!dtInteger.IsAssignableTo(dtLong));
        }

        [Test]
        public void FixedTypeIsAssignableFromGrandparent()
        {
            Assert.That(dtLong.IsAssignableTo(dtShort));
        }

        [Test]
        public void FixedTypeIsNotAssignableFromGrandchild()
        {
            Assert.That(!dtShort.IsAssignableTo(dtLong));
        }

        [Test]
        public void DataTypeIsNotAssignableFromUnrelatedType()
        {
            Assert.That(!dtString.IsAssignableTo(dtHashtable));
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

        [Test]
        public void GuidanceIsPresent()
        {
            Assert.That(() => dtInteger.Guidance, Is.Not.Null);
        }

        [Test]
        public void UnsetGuidanceIsNull()
        {
            Assert.That(() => dtShort.Guidance, Is.Null);
        }
    }
}
