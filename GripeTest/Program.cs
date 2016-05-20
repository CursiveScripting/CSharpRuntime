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
            
            var done = new EndStep();
            
            /*
            var findCar = new SystemProcess(model => car);

            var getInCar = new UserStep(findCar, p => p);
            getInCar.SetDefaultReturnPath(done);
            */

            var getInCar = new UserStep(IO.Print);
            getInCar.SetInputParameter("message", "Getting in the car...");
            getInCar.SetDefaultReturnPath(done);

            var checkDay = new UserStep(Date.GetDayOfWeek);
            checkDay.AddReturnPath("Saturday", done);
            checkDay.AddReturnPath("Sunday", done);
            checkDay.SetDefaultReturnPath(getInCar);

            var getReady = new UserStep(IO.Print);
            getReady.SetInputParameter("message", "Get dressed, etc...");
            getReady.SetDefaultReturnPath(checkDay);
            
            var eatBreakfast = new UserStep(IO.Print);
            eatBreakfast.SetInputParameter("message", "Eating breakfast...");
            eatBreakfast.SetDefaultReturnPath(getReady);

            var demandBreakfast = new UserStep(IO.Print);
            demandBreakfast.SetInputParameter("message", "Demanding breakfast... (young enough to get away with this)");
            demandBreakfast.SetDefaultReturnPath(eatBreakfast);

            var makeBreakfast = new UserStep(IO.Print);
            makeBreakfast.SetInputParameter("message", "Making breakfast...");
            makeBreakfast.SetDefaultReturnPath(eatBreakfast);

            var breakfastAgeCheck = new UserStep(Value.CompareIntegers);
            breakfastAgeCheck.MapInputParameter("value1", "age");
            breakfastAgeCheck.SetInputParameter("value2", 10);
            breakfastAgeCheck.AddReturnPath("less", demandBreakfast);
            breakfastAgeCheck.SetDefaultReturnPath(makeBreakfast);

            var getAge = new UserStep(Value.GetPropertyInteger);
            getAge.MapInputParameter("object", "Person");
            getAge.SetInputParameter("property", "Age");
            getAge.MapOutputParameter("value", "age");
            getAge.SetDefaultReturnPath(breakfastAgeCheck);

            var process = new UserProcess("Morning routine test", "Runs a dummy morning routine, to see if any of this can work", getAge,
                getAge, breakfastAgeCheck, makeBreakfast, demandBreakfast, eatBreakfast, checkDay, getInCar);

            List<string> errors;
            if (!process.Validate(out errors))
            {
                Console.WriteLine("Process failed to validate:");
                foreach(var error in errors)
                    Console.WriteLine(error);

                Console.ReadKey();
                return;
            }

            var model = new Model();
            model["Car"] = new Car();

            Console.WriteLine();
            Console.WriteLine("Running with 'Alice' ...");
            model["Person"] = new Person() { Name = "Alice", Age = 3, Gender = "F" };
            Console.WriteLine(process.Run(model));

            Console.WriteLine();
            Console.WriteLine("Running with 'Bob' ...");
            model["Person"] = new Person() { Name = "Bob", Age = 8, Gender = "M" };
            Console.WriteLine(process.Run(model));
            
            Console.WriteLine();
            Console.WriteLine("Running with 'Carly' ...");
            model["Person"] = new Person() { Name = "Carly", Age = 15, Gender = "M" };
            Console.WriteLine(process.Run(model));

            Console.WriteLine();
            Console.WriteLine("Running with 'Dave' ...");
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
