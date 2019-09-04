using Cursive;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using Xunit;

namespace Tests
{
    public class ValueSetTests
    {
        DataType<short> dtShort;
        ValueSet values;

        Parameter<short> testShort;

        public ValueSetTests()
        {
            dtShort = new FixedType<short>("short", Color.FromKnownColor(KnownColor.Yellow), new Regex("^[0-9]+$"), s => short.Parse(s));

            testShort = new Parameter<short>("testShort", dtShort);
        }
        
        [Fact]
        public void CanRetrieveSetValue()
        {
            values = new ValueSet();

            short val = 27;
            values.Set(testShort, val);
            
            Assert.Equal(27, values.Get(testShort));
        }

        [Fact]
        public void FailToGetUnsetValue()
        {
            values = new ValueSet();

            Assert.Throws<KeyNotFoundException>(() => values.Get(testShort));
        }
    }
}
