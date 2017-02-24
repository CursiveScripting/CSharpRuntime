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
        public void TheseTestsNeedWritten()
        {
            Assert.That(true == false);
        }
    }
}
