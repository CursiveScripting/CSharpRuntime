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
using System.Xml.Schema;

namespace Tests
{
    [TestFixture]
    public class WorkspaceSavingTests
    {
        #region requirements
        private SystemProcess GetDayOfWeek;

        private ValueKey<string> messageParam;
        private SystemProcess Print;

        private ValueKey<string> strValue1, strValue2;

        private SystemProcess EqualsText;

        private ValueKey<int> iValue1, iValue2;
        private SystemProcess CompareIntegers;

        private ValueKey<Person> personVal;
        private ValueKey<string> property;
        private ValueKey<int> iValue;
        private SystemProcess GetPropertyInteger, SetPropertyInteger;

        Workspace workspace;
        RequiredProcess required;

        static DataType<string> text;
        static DataType<int> integer;
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
        
        #endregion requirements

        private static List<string> validationErrors;

        [OneTimeSetUp]
        public void Prepare()
        {
            text = new FixedType<string>("text", Color.FromKnownColor(KnownColor.Gray), new Regex(".*"), s => s, () => string.Empty);
            integer = new FixedType<int>("integer", Color.FromKnownColor(KnownColor.Green), new Regex("[0-9]+"), s => int.Parse(s));
            person = new DataType<Person>("person", Color.FromKnownColor(KnownColor.Red));
            car = new DataType<Car>("car", Color.FromKnownColor(KnownColor.Blue));

            me = new ValueKey<Person>("Me", person);
            carParam = new ValueKey<Car>("Car", car);
            myAge = new ValueKey<int>("My age", integer);

            messageParam = new ValueKey<string>("message", text);
            strValue1 = new ValueKey<string>("value1", text);
            strValue2 = new ValueKey<string>("value2", text);

            iValue1 = new ValueKey<int>("value1", integer);
            iValue2 = new ValueKey<int>("value2", integer);

            personVal = new ValueKey<Person>("object", person);
            property = new ValueKey<string>("property", text);
            iValue = new ValueKey<int>("value", integer);

            GetDayOfWeek = new SystemProcess(
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

            Print = new SystemProcess(
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

            EqualsText = new SystemProcess(
                (ValueSet inputs) =>
                {
                    return Response.SyncTask(inputs.Get(strValue1) == inputs.Get(strValue2) ? "yes" : "no");
                },
                "Test to see if two values are equal.",
                new ValueKey[] { strValue1, strValue2 },
                null,
                new string[] { "yes", "no" }
            );

            CompareIntegers = new SystemProcess(
                (ValueSet inputs) =>
                {
                    int value1 = inputs.Get(iValue1);
                    int value2 = inputs.Get(iValue2);
                    
                    var comparison = value1.CompareTo(value2);
                    return Response.SyncTask(comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal");
                },
                "Compare two integers.",
                new ValueKey[] { iValue1, iValue2 },
                null,
                new string[] { "less", "greater", "equal" }
            );

            GetPropertyInteger = new SystemProcess(
                (ValueSet inputs) =>
                {
                    var outputs = new ValueSet();

                    var source = inputs.Get(personVal);
                    var propertyName = inputs.Get(property);
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

            SetPropertyInteger = new SystemProcess(
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

            workspace = new Workspace();
            workspace.AddDataType(text);
            workspace.AddDataType(integer);
            workspace.AddDataType(person);
            workspace.AddDataType(car);

            workspace.AddSystemProcess("print", Print);
            workspace.AddSystemProcess("getDay", GetDayOfWeek);
            workspace.AddSystemProcess("compare", CompareIntegers);
            workspace.AddSystemProcess("get", GetPropertyInteger);

            required = new RequiredProcess("Run basic tests",
                new ValueKey[] { me, carParam },
                new ValueKey[] { myAge }, null);
            workspace.AddRequiredProcess("Test.MorningRoutine", required);
        }

        [Test]
        public void SavedWorkspaceValidates()
        {
            validationErrors = new List<string>();
            var doc = workspace.WriteForClient();

            Assert.That(doc, Is.Not.Null);

            doc.Schemas.Add("http://cursive.ftwinston.com", AppDomain.CurrentDomain.BaseDirectory + "workspace.xsd");
            doc.Validate(ValidationEventHandler);

            Assert.That(validationErrors.Count, Is.EqualTo(0));
        }

        private static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            validationErrors.Add(e.Message);
        }
    }
}
