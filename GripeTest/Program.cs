using GrIPE;
using GrIPE.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GripeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing GRaphical Interactive Programming Environment");
            
            var done = new EndStep("Done");

            /*
            var car = new Car();

            var findCar = new SystemProcess(model => car);

            var getInCar = new UserStep(findCar, p => p);
            getInCar.SetDefaultReturnPath(done);

            var checkDay = new UserStep(Date.GetDayOfWeek, p => p);
            checkDay.AddReturnPath("Saturday", done);
            checkDay.AddReturnPath("Sunday", done);
            checkDay.SetDefaultReturnPath(getInCar);
            */
            var getReady = new UserStep(IO.Print);
            getReady.SetFixedInputParameter("message", "Get dressed, etc...");
            getReady.SetDefaultReturnPath(/*checkDay*/done);
            /*
            var eatBreakfast = new UserStep(IO.Print("Eating breakfast..."), p => p);
            eatBreakfast.SetDefaultReturnPath(getReady);

            var demandBreakfast = new UserStep(IO.Print("Demanding breakfast... (young enough to get away with this)"), p => p);
            demandBreakfast.SetDefaultReturnPath(eatBreakfast);

            var makeBreakfast = new UserStep(IO.Print("Making breakfast..."), p => p);
            makeBreakfast.SetDefaultReturnPath(eatBreakfast);

            var breakfastAgeCheck = new UserStep(Value.CompareTo(10), p => p.Age);
            breakfastAgeCheck.AddReturnPath("less", demandBreakfast);
            breakfastAgeCheck.SetDefaultReturnPath(makeBreakfast);
            */
            var process = new UserProcess(/*breakfastAgeCheck*/getReady);

            Console.WriteLine();
            Console.WriteLine("Running with 'Alice' ...");
            var model = new Model();
            model["Person"] = new Person() { Name = "Alice", Age = 3, Gender = "F" };
            Console.WriteLine(process.Run(model));

            Console.WriteLine();
            Console.WriteLine("Running with 'Bob' ...");
            model = new Model();
            model["Person"] = new Person() { Name = "Bob", Age = 8, Gender = "M" };
            Console.WriteLine(process.Run(model));
            
            Console.WriteLine();
            Console.WriteLine("Running with 'Carly' ...");
            model = new Model();
            model["Person"] = new Person() { Name = "Carly", Age = 15, Gender = "M" };
            Console.WriteLine(process.Run(model));

            Console.WriteLine();
            Console.WriteLine("Running with 'Dave' ...");
            model = new Model();
            model["Person"] = new Person() { Name = "Dave", Age = 34, Gender = "M" };
            Console.WriteLine(process.Run(model));

            Console.ReadKey();
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
