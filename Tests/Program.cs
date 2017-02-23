using Cursive;
using Tests.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing Cursive Scripting");

            RequiredProcess requiredProcess;
            Workspace w = CreateWorkspace(out requiredProcess);

            XmlDocument doc = new XmlDocument();
            doc.Load("../../test.xml");

            List<string> errors;
            if (!w.LoadProcesses(doc, out errors))
            {
                Console.WriteLine("Error loading processes from XML:");
                foreach (var error in errors)
                    Console.WriteLine(error);

                Console.ReadKey();
                return;
            }

            Run(requiredProcess);

            Console.ReadKey();
        }

        public static DataType text = new FixedType<string>("text", new Regex(".*"), s => s, s => s, () => string.Empty);
        public static DataType integer = new FixedType<int>("integer", new Regex("[0-9]+"), s => int.Parse(s), i => i.ToString());
        public static DataType person = new DataType<Person>("person");
        public static DataType car = new DataType<Car>("car");

        private static Parameter me = new Parameter("Me", person);
        private static Parameter carParam = new Parameter("Car", car);
        private static Parameter myAge = new Parameter("My age", integer);

        private static Workspace CreateWorkspace(out RequiredProcess required)
        {
            Workspace w = new Workspace();
            w.AddDataType(text);
            w.AddDataType(integer);
            w.AddDataType(person);
            w.AddDataType(car);

            w.AddSystemProcess("print", IO.Print);
            w.AddSystemProcess("getDay", Date.GetDayOfWeek);
            w.AddSystemProcess("compare", Processes.Value.CompareIntegers);
            w.AddSystemProcess("get", Processes.Value.GetPropertyInteger);

            required = new RequiredProcess("Run basic tests",
                new Parameter[] { me, carParam },
                new Parameter[] { myAge }, null);
            w.AddRequiredProcess("Test.MorningRoutine", required);

            return w;
        }

        private static void Run(Process process)
        {
            var inputs = new ValueSet();
            inputs[carParam] = new Car();

            Console.WriteLine();
            Console.WriteLine("Running with 'Alice' ...");
            inputs[me] = new Person() { Name = "Alice", Age = 3, Gender = "F" };
            Console.WriteLine(process.Run(inputs));

            Console.WriteLine();
            Console.WriteLine("Running with 'Bob' ...");
            inputs[me] = new Person() { Name = "Bob", Age = 8, Gender = "M" };
            Console.WriteLine(process.Run(inputs));

            Console.WriteLine();
            Console.WriteLine("Running with 'Carly' ...");
            inputs[me] = new Person() { Name = "Carly", Age = 15, Gender = "M" };
            Console.WriteLine(process.Run(inputs));

            Console.WriteLine();
            Console.WriteLine("Running with 'Dave' ...");
            inputs[me] = new Person() { Name = "Dave", Age = 34, Gender = "M" };
            Console.WriteLine(process.Run(inputs));
        }

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
    }
}
