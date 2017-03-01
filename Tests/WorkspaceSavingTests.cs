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

        private ValueKey messageParam;
        private SystemProcess Print;

        private ValueKey strValue1;
        private ValueKey strValue2;

        private SystemProcess EqualsText;

        private ValueKey iValue1, iValue2;
        private SystemProcess CompareIntegers;

        private ValueKey personVal, property, iValue;
        private SystemProcess GetPropertyInteger, SetPropertyInteger;

        Workspace workspace;
        RequiredProcess required;

        private DataType text, integer, person, car;
        private ValueKey me, carParam, myAge;

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

            me = new ValueKey("Me", person);
            carParam = new ValueKey("Car", car);
            myAge = new ValueKey("My age", integer);

            messageParam = new ValueKey("message", text);
            strValue1 = new ValueKey("value1", text);
            strValue2 = new ValueKey("value2", text);

            iValue1 = new ValueKey("value1", integer);
            iValue2 = new ValueKey("value2", integer);

            personVal = new ValueKey("object", person);
            property = new Cursive.ValueKey("property", text);
            iValue = new Cursive.ValueKey("value", integer);

            GetDayOfWeek = new SystemProcess(
                (ValueSet inputs, out ValueSet outputs) =>
                {
                    outputs = null;
                    return DateTime.Today.DayOfWeek.ToString();
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
                (ValueSet inputs, out ValueSet outputs) =>
                {
                    Console.WriteLine(inputs[messageParam]);
                    outputs = null;
                    return string.Empty;
                },
                "Write a message to the system console.",
                new Cursive.ValueKey[] { messageParam },
                null,
                null
            );

            EqualsText = new SystemProcess(
                (ValueSet inputs, out ValueSet outputs) =>
                {
                    outputs = null;
                    return inputs[strValue1].Equals(inputs[strValue2]) ? "yes" : "no";
                },
                "Test to see if two values are equal.",
                new Cursive.ValueKey[] { strValue1, strValue2 },
                null,
                new string[] { "yes", "no" }
            );

            CompareIntegers = new SystemProcess(
                (ValueSet inputs, out ValueSet outputs) =>
                {
                    outputs = null;
                    var value1 = inputs[iValue1];
                    var value2 = inputs[iValue2];

                    if (!(value1 is IComparable) || !(value2 is IComparable))
                        return "error";

                    var comparison = (value1 as IComparable).CompareTo(value2 as IComparable);
                    return comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal";
                },
                "Compare two integers. Returns 'error' if either value doesn't implement IComparable.",
                new Cursive.ValueKey[] { iValue1, iValue2 },
                null,
                new string[] { "less", "greater", "equal", "error" }
            );

            GetPropertyInteger = new SystemProcess(
                (ValueSet inputs, out ValueSet outputs) =>
                {
                    outputs = new ValueSet();

                    var source = inputs[personVal];
                    var propertyName = inputs[property].ToString();
                    var prop = source.GetType().GetProperty(propertyName);
                    if (prop == null)
                        return "error";

                    try
                    {
                        outputs[iValue] = prop.GetValue(source);
                    }
                    catch
                    {
                        return "error";
                    }
                    return "ok";
                },
                "Output the named property of a given object. Returns 'error' if the property does not exist, or if getting it fails.",
                new Cursive.ValueKey[] { personVal, property },
                new Cursive.ValueKey[] { iValue },
                new string[] { "ok", "error" }
            );

            SetPropertyInteger = new SystemProcess(
                (ValueSet inputs, out ValueSet outputs) =>
                {
                    outputs = null;
                    var destination = inputs[personVal];
                    var prop = destination.GetType().GetProperty(inputs[property].ToString());
                    if (prop == null)
                        return "error";

                    try
                    {
                        prop.SetValue(destination, inputs[iValue]);
                    }
                    catch
                    {
                        return "error";
                    }
                    return "ok";
                },
                "Set the named property of a given object to the value specified. Returns 'error' if the property does not exist, or if setting it fails.",
                new Cursive.ValueKey[] { personVal, property, iValue },
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

            doc.Schemas.Add("http://cursive.ftwinston.com", "workspace.xsd");
            doc.Validate(ValidationEventHandler);

            Assert.That(validationErrors.Count, Is.EqualTo(0));
        }

        private static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            validationErrors.Add(e.Message);
        }
    }
}
