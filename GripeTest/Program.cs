﻿using GrIPE;
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

            Workspace w = new Workspace();
            w.AddDataType(new FixedType<string>("text", s => s, s => s));
            w.AddDataType(new FixedType<int>("integer", s => int.Parse(s), i => i.ToString()));
            w.AddDataType(new DataType<Person>("person"));
            w.AddDataType(new DataType<Car>("car"));

            w.AddSystemProcess("print", IO.Print);
            w.AddSystemProcess("getDay", Date.GetDayOfWeek);
            w.AddSystemProcess("compare", Value.CompareIntegers);
            w.AddSystemProcess("get", Value.GetPropertyInteger);

            var done = new EndStep("done");

            /*
            var findCar = new SystemProcess(model => car);

            var getInCar = new UserStep(findCar, p => p);
            getInCar.SetDefaultReturnPath(done);
            */

            var getInCar = new UserStep(w, "getInCar", "print");
            getInCar.SetInputParameter("message", "Getting in the car...");
            getInCar.SetDefaultReturnPath(done);

            var checkDay = new UserStep(w, "checkDay", "getDay");
            checkDay.AddReturnPath("Saturday", done);
            checkDay.AddReturnPath("Sunday", done);
            checkDay.SetDefaultReturnPath(getInCar);

            var getReady = new UserStep(w, "getReady", "print");
            getReady.SetInputParameter("message", "Get dressed, etc...");
            getReady.SetDefaultReturnPath(checkDay);

            var eatBreakfast = new UserStep(w, "eatBreakfast", "print");
            eatBreakfast.SetInputParameter("message", "Eating breakfast...");
            eatBreakfast.SetDefaultReturnPath(getReady);

            var demandBreakfast = new UserStep(w, "demandBreakfast", "print");
            demandBreakfast.SetInputParameter("message", "Demanding breakfast... (young enough to get away with this)");
            demandBreakfast.SetDefaultReturnPath(eatBreakfast);

            var makeBreakfast = new UserStep(w, "makeBreakfast", "print");
            makeBreakfast.SetInputParameter("message", "Making breakfast...");
            makeBreakfast.SetDefaultReturnPath(eatBreakfast);

            var breakfastAgeCheck = new UserStep(w, "breakfastAgeCheck", "compare");
            breakfastAgeCheck.MapInputParameter("value1", "age");
            breakfastAgeCheck.SetInputParameter("value2", 10);
            breakfastAgeCheck.AddReturnPath("less", demandBreakfast);
            breakfastAgeCheck.SetDefaultReturnPath(makeBreakfast);

            var getAge = new UserStep(w, "getAge", "get");
            getAge.MapInputParameter("object", "Person");
            getAge.SetInputParameter("property", "Age");
            getAge.MapOutputParameter("value", "age");
            getAge.SetDefaultReturnPath(breakfastAgeCheck);

            var process = new UserProcess(w, "Morning routine test", "Runs a dummy morning routine, to see if any of this can work", getAge,
                getAge, breakfastAgeCheck, makeBreakfast, demandBreakfast, eatBreakfast, checkDay, getInCar);

            process.AddInput(w, "Person", "person");
            process.AddInput(w, "Car", "car");
            
            List<string> errors;
            if (!w.Validate(out errors))
            {
                Console.WriteLine("Failed to validate workspace:");
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
