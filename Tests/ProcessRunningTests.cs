using Cursive;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Tests
{
    [TestFixture]
    public class ProcessRunningTests
    {
        #region system processes
        public static SystemProcess GetDayOfWeek()
        {
            return new SystemProcess(
                "getDay",
                "Returns the name of the current day of the week",
                (ValueSet inputs) =>
                {
                    return new ProcessResult(DateTime.Today.DayOfWeek.ToString());
                },
                null, null,
                new string[] {
                    DayOfWeek.Monday.ToString(),
                    DayOfWeek.Tuesday.ToString(),
                    DayOfWeek.Wednesday.ToString(),
                    DayOfWeek.Thursday.ToString(),
                    DayOfWeek.Friday.ToString(),
                    DayOfWeek.Saturday.ToString(),
                    DayOfWeek.Sunday.ToString()
                }
            );
        }
        
        public static SystemProcess Print()
        {
            Parameter<string> messageParam = new Parameter<string>("message", text);

            return new SystemProcess(
                "print",
                "Write a message to the system console.",
                (ValueSet inputs) =>
                {
                    Console.WriteLine(inputs.Get(messageParam));
                    return new ProcessResult();
                },
                new Parameter[] { messageParam },
                null,
                null
            );
        }

        public static SystemProcess EqualsText()
        {
            Parameter<string> strValue1 = new Parameter<string>("value1", text);
            Parameter<string> strValue2 = new Parameter<string>("value2", text);

            return new SystemProcess(
                "Equals text",
                "Test to see if two values are equal.",
                (ValueSet inputs) =>
                {
                    return new ProcessResult(inputs.Get(strValue1).Equals(inputs.Get(strValue2)) ? "yes" : "no");
                },
                new Parameter[] { strValue1, strValue2 },
                null,
                new string[] { "yes", "no" }
            );
        }

        public static SystemProcess CompareIntegers()
        {
            Parameter<int> iValue1 = new Parameter<int>("value1", integer);
            Parameter<int> iValue2 = new Parameter<int>("value2", integer);

            return new SystemProcess(
                "compare",
                "Compare two integers",
                (ValueSet inputs) =>
                {
                    int value1 = inputs.Get(iValue1);
                    int value2 = inputs.Get(iValue2);

                    var comparison = value1.CompareTo(value2);
                    return new ProcessResult(comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal");
                },
                new Parameter[] { iValue1, iValue2 },
                null,
                new string[] { "less", "greater", "equal" }
            );
        }

        public static SystemProcess GetPropertyInteger()
        {
            var personVal = new Parameter<object>("object", objectType);
            var property = new Parameter<string>("property", text);
            var iValue = new Parameter<int>("value", integer);

            return new SystemProcess(
                "get",
                "Output the named property of a given object. Returns 'error' if the property does not exist, or if getting it fails.",
                (ValueSet inputs) =>
                {
                    var outputs = new ValueSet();

                    var source = inputs.Get(personVal);
                    var propertyName = inputs.Get(property).ToString();
                    var prop = source.GetType().GetProperty(propertyName);
                    if (prop == null)
                        return new ProcessResult("error", outputs);

                    try
                    {
                        outputs.Set(iValue, (int)prop.GetValue(source));
                    }
                    catch
                    {
                        return new ProcessResult("error", outputs);
                    }
                    return new ProcessResult("ok", outputs);
                },
                new Parameter[] { personVal, property },
                new Parameter[] { iValue },
                new string[] { "ok", "error" }
            );
        }

        public static SystemProcess SetPropertyInteger()
        {
            var personVal = new Parameter<object>("object", objectType);
            var property = new Parameter<string>("property", text);
            var iValue = new Parameter<int>("value", integer);

            return new SystemProcess(
                "Set integer property",
                "Set the named property of a given object to the value specified. Returns 'error' if the property does not exist, or if setting it fails.",
                (ValueSet inputs) =>
                {
                    var destination = inputs.Get(personVal);
                    var prop = destination.GetType().GetProperty(inputs.Get(property));
                    if (prop == null)
                        return new ProcessResult("error");

                    try
                    {
                        prop.SetValue(destination, inputs.Get(iValue));
                    }
                    catch
                    {
                        return new ProcessResult("error");
                    }
                    return new ProcessResult("ok");
                },
                new Parameter[] { personVal, property, iValue },
                null,
                new string[] { "ok", "error" }
            );
        }
        #endregion system processes
        
        Workspace workspace;
        RequiredProcess required;

        static DataType<string> text;
        static DataType<int> integer;
        static DataType<object> objectType;
        static DataType<Person> person;
        static DataType<Car> car;
        static Parameter<Person> me;
        static Parameter<Car> carParam;
        static Parameter<int> myAge;

        class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Gender { get; set; }
        }

        class Car
        {
            public Car()
            {
                Passengers = new List<Person>();
            }

            Person Driver { get; set; }
            List<Person> Passengers { get; set; }
        }


        [OneTimeSetUp]
        public void Prepare()
        {
            text = new FixedType<string>("text", Color.FromKnownColor(KnownColor.Gray), new Regex(".*"), s => s, () => string.Empty);
            integer = new FixedType<int>("integer", Color.FromKnownColor(KnownColor.Green), new Regex("[0-9]+"), s => int.Parse(s));
            objectType = new DataType<object>("object", Color.FromKnownColor(KnownColor.Blue));
            person = new DataType<Person>("person", Color.FromKnownColor(KnownColor.Red), objectType);
            car = new DataType<Car>("car", Color.FromKnownColor(KnownColor.Blue), objectType);

            me = new Parameter<Person>("Me", person);
            carParam = new Parameter<Car>("Car", car);
            myAge = new Parameter<int>("My age", integer);

            required = new RequiredProcess(
                "Test.MorningRoutine",
                "Run basic tests",
                new Parameter[] { me, carParam },
                new Parameter[] { myAge }
                , null
            );

            workspace = new Workspace
            {
                Types = new DataType[] { text, integer, person, car },
                SystemProcesses = new SystemProcess[] { Print(), GetDayOfWeek(), CompareIntegers(), GetPropertyInteger() },
                RequiredProcesses = new RequiredProcess[] { required }
            };
        }
        
        [Test]
        public void RunBasicProcess()
        {
            string processJson;
            var processResourceName = "Tests.test.json";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(processResourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                processJson = reader.ReadToEnd();
            }

            var result = workspace.LoadUserProcesses(processJson, out List<string> errors);

            Assert.IsTrue(result);
            Assert.IsNull(errors);

            var inputs = new ValueSet();
            inputs.Set(carParam, new Car());

            Console.WriteLine();
            Console.WriteLine("Running with 'Alice' ...");
            inputs.Set(me, new Person() { Name = "Alice", Age = 3, Gender = "F" });
            Console.WriteLine(required.Run(inputs));

            Console.WriteLine();
            Console.WriteLine("Running with 'Bob' ...");
            inputs.Set(me, new Person() { Name = "Bob", Age = 8, Gender = "M" });
            Console.WriteLine(required.Run(inputs));

            Console.WriteLine();
            Console.WriteLine("Running with 'Carly' ...");
            inputs.Set(me, new Person() { Name = "Carly", Age = 15, Gender = "F" });
            Console.WriteLine(required.Run(inputs));

            Console.WriteLine();
            Console.WriteLine("Running with 'Dave' ...");
            inputs.Set(me, new Person() { Name = "Dave", Age = 34, Gender = "M" });
            Console.WriteLine(required.Run(inputs));
        }
    }
}
