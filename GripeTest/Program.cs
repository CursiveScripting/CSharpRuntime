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

            var ageCheck = new UserStep<Person, int>(Value.CompareTo(25), p => p.Age);
            ageCheck.AddOutput("greater", new EndStep<Person>("This person is too old"));

            var ageConfirm = new UserStep<Person, Person>(IO.Print<Person>("OK, got a young person"), p => p);
            ageCheck.SetDefaultOutput(ageConfirm);
            
            var genderCheck = new UserStep<Person, string>(Value.Equals("M"), p => p.Gender);
            genderCheck.AddOutput("no", new EndStep<Person>("Lady"));
            genderCheck.AddOutput("yes", new EndStep<Person>("Gent"));
            genderCheck.SetDefaultOutput(new EndStep<Person>("Failed to determine gender"));

            ageConfirm.SetDefaultOutput(genderCheck);

            var process = new UserProcess<Person>(ageCheck);

            Console.WriteLine();
            Console.WriteLine("Running with 'Alice' ...");
            Console.WriteLine(process.Run(new Person() { Name = "Alice", Age = 21, Gender = "F" }));

            Console.WriteLine();
            Console.WriteLine("Running with 'Bob' ...");
            Console.WriteLine(process.Run(new Person() { Name = "Bob", Age = 27, Gender = "M" }));

            Console.ReadKey();
        }

        class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Gender { get; set; }
        }
    }
}
