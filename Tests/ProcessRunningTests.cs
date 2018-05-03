using Cursive;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Tests
{
    [TestFixture]
    public class ProcessRunningTests
    {
        #region system processes
        public static SystemProcess GetDayOfWeek()
        {
            return new SystemProcess(
                (ValueSet inputs) =>
                {
                    return Response.SyncTask(DateTime.Today.DayOfWeek.ToString());
                },
                "Returns the name of the current day of the week",
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
            ValueKey<string> messageParam = new ValueKey<string>("message", text);

            return new SystemProcess(
                (ValueSet inputs) =>
                {
                    Console.WriteLine(inputs.Get(messageParam));
                    return Response.SyncTask();
                },
                "Write a message to the system console.",
                new ValueKey[] { messageParam },
                null,
                null
            );
        }

        public static SystemProcess EqualsText()
        {
            ValueKey<string> strValue1 = new ValueKey<string>("value1", text);
            ValueKey<string> strValue2 = new ValueKey<string>("value2", text);

            return new SystemProcess(
                (ValueSet inputs) =>
                {
                    return Response.SyncTask(inputs.Get(strValue1).Equals(inputs.Get(strValue2)) ? "yes" : "no");
                },
                "Test to see if two values are equal.",
                new ValueKey[] { strValue1, strValue2 },
                null,
                new string[] { "yes", "no" }
            );
        }

        public static SystemProcess CompareIntegers()
        {
            ValueKey<int> iValue1 = new ValueKey<int>("value1", integer);
            ValueKey<int> iValue2 = new ValueKey<int>("value2", integer);

            return new SystemProcess(
                (ValueSet inputs) =>
                {
                    int value1 = inputs.Get(iValue1);
                    int value2 = inputs.Get(iValue2);

                    var comparison = value1.CompareTo(value2);
                    return Response.SyncTask(comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal");
                },
                "Compare two integers",
                new ValueKey[] { iValue1, iValue2 },
                null,
                new string[] { "less", "greater", "equal" }
            );
        }

        public static SystemProcess GetPropertyInteger()
        {
            ValueKey<object> personVal = new ValueKey<object>("object", objectType);
            ValueKey<string> property = new ValueKey<string>("property", text);
            ValueKey<int> iValue = new ValueKey<int>("value", integer);

            return new SystemProcess(
                (ValueSet inputs) =>
                {
                    var outputs = new ValueSet();

                    var source = inputs.Get(personVal);
                    var propertyName = inputs.Get(property).ToString();
                    var prop = source.GetType().GetProperty(propertyName);
                    if (prop == null)
                        return Response.SyncTask("error", outputs);

                    try
                    {
                        outputs.Set(iValue, (int)prop.GetValue(source));
                    }
                    catch
                    {
                        return Response.SyncTask("error", outputs);
                    }
                    return Response.SyncTask("ok", outputs);
                },
                "Output the named property of a given object. Returns 'error' if the property does not exist, or if getting it fails.",
                new ValueKey[] { personVal, property },
                new ValueKey[] { iValue },
                new string[] { "ok", "error" }
            );
        }

        public static SystemProcess SetPropertyInteger()
        {
            ValueKey<object> personVal = new ValueKey<object>("object", objectType);
            ValueKey<string> property = new ValueKey<string>("property", text);
            ValueKey<int> iValue = new ValueKey<int>("value", integer);

            return new SystemProcess(
                (ValueSet inputs) =>
                {
                    var destination = inputs.Get(personVal);
                    var prop = destination.GetType().GetProperty(inputs.Get(property));
                    if (prop == null)
                        return Response.SyncTask("error");

                    try
                    {
                        prop.SetValue(destination, inputs.Get(iValue));
                    }
                    catch
                    {
                        return Response.SyncTask("error");
                    }
                    return Response.SyncTask("ok");
                },
                "Set the named property of a given object to the value specified. Returns 'error' if the property does not exist, or if setting it fails.",
                new ValueKey[] { personVal, property, iValue },
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
        static ValueKey<Person> me;
        static ValueKey<Car> carParam;
        static ValueKey<int> myAge;

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
            person = new DataType<Person>("person", Color.FromKnownColor(KnownColor.Red));
            objectType = new DataType<object>("object", Color.FromKnownColor(KnownColor.Blue));
            car = new DataType<Car>("car", Color.FromKnownColor(KnownColor.Blue));

            me = new ValueKey<Person>("Me", person);
            carParam = new ValueKey<Car>("Car", car);
            myAge = new ValueKey<int>("My age", integer);
            
            workspace = new Workspace();
            workspace.AddDataType(text);
            workspace.AddDataType(integer);
            workspace.AddDataType(person);
            workspace.AddDataType(car);

            workspace.AddSystemProcess("print", Print());
            workspace.AddSystemProcess("getDay", GetDayOfWeek());
            workspace.AddSystemProcess("compare", CompareIntegers());
            workspace.AddSystemProcess("get", GetPropertyInteger());

            required = new RequiredProcess("Run basic tests",
                new ValueKey[] { me, carParam },
                new ValueKey[] { myAge }, null);
            workspace.AddRequiredProcess("Test.MorningRoutine", required);
        }
        
        [Test]
        public void RunBasicProcess()
        {
            XmlDocument doc = new XmlDocument();
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Tests.test.xml";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string processXML = reader.ReadToEnd();
                doc.LoadXml(processXML);
            }

            List<string> errors;
            if (!workspace.LoadProcesses(doc, out errors))
            {
                Console.WriteLine("Error loading processes from XML:");
                foreach (var error in errors)
                    Console.WriteLine(error);

                //Console.ReadKey();
                return;
            }

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

            Assert.That(true == false);
        }
    }
}
