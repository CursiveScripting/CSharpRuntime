using GrIPE;
using GrIPE.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GripeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing GRaphical Interactive Programming Environment");

            Workspace w = new Workspace();
            w.AddDataType(new FixedType<string>("text", s => s, s => s));
            w.AddDataType(new FixedType<int>("integer", s => int.Parse(s), i => i.ToString()));
            w.AddDataType(new DataType<Person>("person"));
            w.AddDataType(new DataType<Car>("car"));

            w.AddSystemProcess("print", IO.Print);
            w.AddSystemProcess("getDay", Date.GetDayOfWeek);
            w.AddSystemProcess("compare", Value.CompareIntegers);
            w.AddSystemProcess("get", Value.GetPropertyInteger);

            XmlDocument doc = new XmlDocument();
            doc.Load("../../test.xml");

            List<string> errors;
            if (!w.LoadUserProcesses(doc, out errors))
            {
                Console.WriteLine("Error loading processes from XML:");
                foreach (var error in errors)
                    Console.WriteLine(error);
            }
            else
            {
                Console.WriteLine("Loaded processes OK from XML");
                if (Validate(w))
                    Run(w.GetProcess("Test.MorningRoutine"));
                Console.ReadKey();
            }

            w.Clear();

            var done = new EndStep("done");

            var print = IO.Print(w);

            /*
            var findCar = new SystemProcess(model => car);

            var getInCar = new UserStep(findCar, p => p);
            getInCar.SetDefaultReturnPath(done);
            */

            var getInCar = new UserStep("getInCar", print);
            getInCar.SetInputParameter("message", "Getting in the car...");
            getInCar.SetDefaultReturnPath(done);

            var checkDay = new UserStep("checkDay", Date.GetDayOfWeek(w));
            checkDay.AddReturnPath("Saturday", done);
            checkDay.AddReturnPath("Sunday", done);
            checkDay.SetDefaultReturnPath(getInCar);

            var getReady = new UserStep("getReady", print);
            getReady.SetInputParameter("message", "Get dressed, etc...");
            getReady.SetDefaultReturnPath(checkDay);

            var eatBreakfast = new UserStep("eatBreakfast", print);
            eatBreakfast.SetInputParameter("message", "Eating breakfast...");
            eatBreakfast.SetDefaultReturnPath(getReady);

            var demandBreakfast = new UserStep("demandBreakfast", print);
            demandBreakfast.SetInputParameter("message", "Demanding breakfast... (young enough to get away with this)");
            demandBreakfast.SetDefaultReturnPath(eatBreakfast);

            var makeBreakfast = new UserStep("makeBreakfast", print);
            makeBreakfast.SetInputParameter("message", "Making breakfast...");
            makeBreakfast.SetDefaultReturnPath(eatBreakfast);

            var breakfastAgeCheck = new UserStep("breakfastAgeCheck", Value.CompareIntegers(w));
            breakfastAgeCheck.MapInputParameter("value1", "age");
            breakfastAgeCheck.SetInputParameter("value2", 10);
            breakfastAgeCheck.AddReturnPath("less", demandBreakfast);
            breakfastAgeCheck.SetDefaultReturnPath(makeBreakfast);

            var getAge = new UserStep("getAge", Value.GetPropertyInteger(w));
            getAge.MapInputParameter("object", "Me");
            getAge.SetInputParameter("property", "Age");
            getAge.MapOutputParameter("value", "age");
            getAge.SetDefaultReturnPath(breakfastAgeCheck);

            var process = new UserProcess("Morning routine test", "Runs a dummy morning routine, to see if any of this can work", getAge,
                new Step[] { getAge, breakfastAgeCheck, makeBreakfast, demandBreakfast, eatBreakfast, checkDay, getInCar });

            process.AddInput(w, "Person", "person");
            process.AddInput(w, "Car", "car");
            w.AddUserProcess(process);

            if (Validate(w))
                Run(process);

            Console.ReadKey();
        }

        private static bool Validate(Workspace w)
        {
            List<string> errors;
            if (!w.Validate(out errors))
            {
                Console.WriteLine("Failed to validate workspace:");
                foreach (var error in errors)
                    Console.WriteLine(error);
                return false;
            }
            return true;
        }

        private static void Run(Process process)
        {
            var model = new Model();
            model["Car"] = new Car();

            Console.WriteLine();
            Console.WriteLine("Running with 'Alice' ...");
            model["Me"] = new Person() { Name = "Alice", Age = 3, Gender = "F" };
            Console.WriteLine(process.Run(model));

            Console.WriteLine();
            Console.WriteLine("Running with 'Bob' ...");
            model["Me"] = new Person() { Name = "Bob", Age = 8, Gender = "M" };
            Console.WriteLine(process.Run(model));

            Console.WriteLine();
            Console.WriteLine("Running with 'Carly' ...");
            model["Me"] = new Person() { Name = "Carly", Age = 15, Gender = "M" };
            Console.WriteLine(process.Run(model));

            Console.WriteLine();
            Console.WriteLine("Running with 'Dave' ...");
            model["Me"] = new Person() { Name = "Dave", Age = 34, Gender = "M" };
            Console.WriteLine(process.Run(model));
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
