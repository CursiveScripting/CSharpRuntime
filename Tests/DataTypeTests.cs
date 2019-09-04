using Cursive;
using System;
using System.Collections;
using System.Drawing;
using System.Text.RegularExpressions;
using Xunit;

namespace Tests
{
    public class DataTypeTests
    {
        DataType dtObject, dtHashtable, dtShort, dtInteger, dtLong, dtString;

        public DataTypeTests()
        {
            dtObject = new DataType<object>("object", Color.FromKnownColor(KnownColor.Red));
            dtHashtable = new DataType<Hashtable>("hashtable", Color.FromKnownColor(KnownColor.Orange), dtObject, () => new Hashtable());
            dtShort = new FixedType<short>("short", Color.FromKnownColor(KnownColor.Yellow), new Regex("^[0-9]+$"), s => short.Parse(s));
            dtInteger = new FixedType<int>("integer", Color.FromKnownColor(KnownColor.GreenYellow), new Regex("^[0-9]+$"), s => int.Parse(s), () => -1, dtShort, "Enter a non-negative, whole number");
            dtLong = new FixedType<long>("long", Color.FromKnownColor(KnownColor.Green), new Regex("^[0-9]+$"), s => long.Parse(s), null, dtInteger);
            dtString = new FixedType<string>("string", Color.FromKnownColor(KnownColor.SkyBlue), new Regex(".*"), s => s, () => string.Empty);
        }
        
        [Fact]
        public void DataTypeIsAssignableFromParent()
        {
            Assert.True(dtHashtable.IsAssignableTo(dtObject));
        }

        [Fact]
        public void DataTypeIsNotAssignableFromChild()
        {
            Assert.False(dtObject.IsAssignableTo(dtHashtable));
        }

        [Fact]
        public void FixedTypeIsAssignableFromParent()
        {
            Assert.True(dtLong.IsAssignableTo(dtInteger));
        }

        [Fact]
        public void FixedTypeIsNotAssignableFromChild()
        {
            Assert.False(dtInteger.IsAssignableTo(dtLong));
        }

        [Fact]
        public void FixedTypeIsAssignableFromGrandparent()
        {
            Assert.True(dtLong.IsAssignableTo(dtShort));
        }

        [Fact]
        public void FixedTypeIsNotAssignableFromGrandchild()
        {
            Assert.False(dtShort.IsAssignableTo(dtLong));
        }

        [Fact]
        public void DataTypeIsNotAssignableFromUnrelatedType()
        {
            Assert.False(dtString.IsAssignableTo(dtHashtable));
        }
        
        [Fact]
        public void UnspecifiedReferenceTypeDefaultIsNull()
        {
            Assert.Null(dtObject.GetDefaultValue());
        }

        [Fact]
        public void ReferenceTypeDefaultIsNotNull()
        {
            Assert.NotNull(dtHashtable.GetDefaultValue());
        }

        [Fact]
        public void ReferenceTypeDefaultIsCorrectType()
        {
            var defaultVal = dtHashtable.GetDefaultValue();
            Assert.IsType<Hashtable>(defaultVal);
        }

        [Fact]
        public void ReferenceTypeDefaultInstancesDiffer()
        {
            var defaultVal1 = dtHashtable.GetDefaultValue();
            var defaultVal2 = dtHashtable.GetDefaultValue();
            Assert.NotSame(defaultVal1, defaultVal2);
        }

        [Fact]
        public void ValueTypeDefaultIsNotNull()
        {
            Assert.NotNull(dtLong.GetDefaultValue());
        }

        [Fact]
        public void ValueTypeUsesSpecifiedDefault()
        {
            Assert.Equal(-1, dtInteger.GetDefaultValue());
        }

        [Fact]
        public void ValidateValidValue()
        {
            Assert.Matches(dtInteger.Validation, "2");
        }

        [Fact]
        public void ValidateInvalidValue()
        {
            Assert.DoesNotMatch(dtInteger.Validation, "-2");
        }

        [Fact]
        public void ValidateNullValue()
        {
            Assert.Throws<ArgumentNullException>(() => dtInteger.Validation.IsMatch(null));
        }

        [Fact]
        public void ParseValidValue()
        {
            Assert.Equal(2, ((IDeserializable)dtInteger).Deserialize("2"));
        }

        [Fact]
        public void ParseInvalidValue()
        {
            Assert.Throws<FormatException>(() => ((IDeserializable)dtInteger).Deserialize("blah"));
        }

        [Fact]
        public void GuidanceIsPresent()
        {
            Assert.NotNull(dtInteger.Guidance);
        }

        [Fact]
        public void UnsetGuidanceIsNull()
        {
            Assert.Null(dtShort.Guidance);
        }
    }
}
