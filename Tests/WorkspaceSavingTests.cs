using Cursive;
using Newtonsoft.Json;
using NJsonSchema;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                "Get day of week",
                "Returns the name of the current day of the week",
                (ValueSet inputs) =>
                {
                    return Response.SyncTask(DateTime.Today.DayOfWeek.ToString());
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

            Print = new SystemProcess(
                "Print",
                "Write a message to the system console.",
                (ValueSet inputs) =>
                {
                    Console.WriteLine(inputs.Get(messageParam));
                    return Response.SyncTask();
                },
                new ValueKey[] { messageParam },
                null,
                null
            );

            EqualsText = new SystemProcess(
                "Equals text",
                "Test to see if two values are equal.",
                (ValueSet inputs) =>
                {
                    return Response.SyncTask(inputs.Get(strValue1) == inputs.Get(strValue2) ? "yes" : "no");
                },
                new ValueKey[] { strValue1, strValue2 },
                null,
                new string[] { "yes", "no" }
            );

            CompareIntegers = new SystemProcess(
                "Compare",
                "Compare two integers.",
                (ValueSet inputs) =>
                {
                    int value1 = inputs.Get(iValue1);
                    int value2 = inputs.Get(iValue2);

                    var comparison = value1.CompareTo(value2);
                    return Response.SyncTask(comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal");
                },
                new ValueKey[] { iValue1, iValue2 },
                null,
                new string[] { "less", "greater", "equal" }
            );

            GetPropertyInteger = new SystemProcess(
                "Get integer property",
                "Output the named property of a given object. Returns 'error' if the property does not exist, or if getting it fails.",
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
                new ValueKey[] { personVal, property },
                new ValueKey[] { iValue },
                new string[] { "ok", "error" }
            );

            SetPropertyInteger = new SystemProcess(
                "Set integer property",
                "Set the named property of a given object to the value specified. Returns 'error' if the property does not exist, or if setting it fails.",
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
                new ValueKey[] { personVal, property, iValue },
                null,
                new string[] { "ok", "error" }
            );

            workspace = new Workspace();
            workspace.Types.Add(text);
            workspace.Types.Add(integer);
            workspace.Types.Add(person);
            workspace.Types.Add(car);

            workspace.SystemProcesses.Add(Print);
            workspace.SystemProcesses.Add(GetDayOfWeek);
            workspace.SystemProcesses.Add(CompareIntegers);
            workspace.SystemProcesses.Add(GetPropertyInteger);

            required = new RequiredProcess(
                "Test.MorningRoutine",
                "Run basic tests",
                new ValueKey[] { me, carParam },
                new ValueKey[] { myAge },
                null
            );
            workspace.RequiredProcesses.Add(required);
        }

        [Test]
        public async Task SavedWorkspaceValidates()
        {
            var workspaceJson = JsonConvert.SerializeObject(workspace);

            Assert.That(workspaceJson, Is.Not.Null);

            string schemaJson;

            var schemaResourceName = "Cursive.workspace.json";
            using (Stream stream = typeof(Workspace).Assembly.GetManifestResourceStream(schemaResourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                schemaJson = reader.ReadToEnd();
            }

            Assert.That(schemaJson, Is.Not.Null);

            var schema = await JsonSchema.FromJsonAsync(schemaJson);

            var validationErrors = schema.Validate(workspaceJson);

            Assert.IsEmpty(validationErrors);
        }
    }
}
