using Cursive;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Tests
{
    public class IntegerWorkspace : Workspace
    {
        public IntegerWorkspace()
        {
            Integer = new FixedType<int>("integer", Color.FromKnownColor(KnownColor.Green), new Regex("[0-9]+"), s => int.Parse(s));

            {
                Parameter<int> in1 = new Parameter<int>("value 1", Integer);
                Parameter<int> in2 = new Parameter<int>("value 2", Integer);

                Parameter<int> result = new Parameter<int>("result", Integer);

                Add = new SystemProcess(
                    "Add",
                    "Adds two integers",
                    (ValueSet inputs) =>
                    {
                        int value1 = inputs.Get(in1);
                        int value2 = inputs.Get(in2);

                        var outputs = new ValueSet();
                        outputs.Set(result, value1 + value2);

                        return new ProcessResult(outputs);
                    },
                    new Parameter[] { in1, in2 },
                    new Parameter[] { result },
                    null
                );
            }


            {
                Parameter<int> in1 = new Parameter<int>("value 1", Integer);
                Parameter<int> in2 = new Parameter<int>("value 2", Integer);

                Parameter<int> result = new Parameter<int>("result", Integer);

                Subtract = new SystemProcess(
                    "Subtracts",
                    "Subtracts one integer from another",
                    (ValueSet inputs) =>
                    {
                        int value1 = inputs.Get(in1);
                        int value2 = inputs.Get(in2);

                        var outputs = new ValueSet();
                        outputs.Set(result, value1 - value2);

                        return new ProcessResult(outputs);
                    },
                    new Parameter[] { in1, in2 },
                    new Parameter[] { result },
                    null
                );
            }


            {
                Parameter<int> in1 = new Parameter<int>("value 1", Integer);
                Parameter<int> in2 = new Parameter<int>("value 2", Integer);

                Parameter<int> result = new Parameter<int>("result", Integer);

                Multiply = new SystemProcess(
                    "Multiply",
                    "Multiplies two integers",
                    (ValueSet inputs) =>
                    {
                        int value1 = inputs.Get(in1);
                        int value2 = inputs.Get(in2);

                        var outputs = new ValueSet();
                        outputs.Set(result, value1 * value2);

                        return new ProcessResult(outputs);
                    },
                    new Parameter[] { in1, in2 },
                    new Parameter[] { result },
                    null
                );
            }


            {
                Parameter<int> in1 = new Parameter<int>("value 1", Integer);
                Parameter<int> in2 = new Parameter<int>("value 2", Integer);

                Compare = new SystemProcess(
                    "compare",
                    "Compare two integers",
                    (ValueSet inputs) =>
                    {
                        int value1 = inputs.Get(in1);
                        int value2 = inputs.Get(in2);

                        var comparison = value1.CompareTo(value2);
                        return new ProcessResult(comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal");
                    },
                    new Parameter[] { in1, in2 },
                    null,
                    new string[] { "less", "greater", "equal" }
                );
            }


            RequiredProcess entryPoint;

            {
                Parameter<int> in1 = new Parameter<int>("value", Integer);
                Parameter<int> out1 = new Parameter<int>("result", Integer);

                entryPoint = new RequiredProcess(
                    "Modify number",
                    "Perform some operation(s) on a number",
                    new Parameter[] { in1 },
                    new Parameter[] { out1 },
                    null
                );
            }


            Types = new DataType[] { this.Integer };
            SystemProcesses = new SystemProcess[] { Add, Subtract, Multiply, Compare };
            RequiredProcesses = new RequiredProcess[] { entryPoint };
        }

        public DataType<int> Integer { get; }

        public SystemProcess Add { get; }

        public SystemProcess Subtract { get; }

        public SystemProcess Multiply { get; }

        public SystemProcess Compare { get; }
    }
}
