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


            AddInput1 = new Parameter<int>("value 1", Integer);
            AddInput2 = new Parameter<int>("value 2", Integer);

            AddOutput = new Parameter<int>("result", Integer);

            Add = new SystemProcess(
                "Add",
                "Adds two integers",
                (ValueSet inputs) =>
                {
                    int value1 = inputs.Get(AddInput1);
                    int value2 = inputs.Get(AddInput2);

                    var outputs = new ValueSet();
                    outputs.Set(AddOutput, value1 + value2);

                    return new ProcessResult(outputs);
                },
                new Parameter[] { AddInput1, AddInput2 },
                new Parameter[] { AddOutput },
                null
            );


            SubtractInput1 = new Parameter<int>("value 1", Integer);
            SubtractInput2 = new Parameter<int>("value 2", Integer);

            SubtractOutput = new Parameter<int>("result", Integer);

            Subtract = new SystemProcess(
                "Subtracts",
                "Subtracts one integer from another",
                (ValueSet inputs) =>
                {
                    int value1 = inputs.Get(SubtractInput1);
                    int value2 = inputs.Get(SubtractInput2);

                    var outputs = new ValueSet();
                    outputs.Set(SubtractOutput, value1 - value2);

                    return new ProcessResult(outputs);
                },
                new Parameter[] { SubtractInput1, SubtractInput2 },
                new Parameter[] { SubtractOutput },
                null
            );


            MultiplyInput1 = new Parameter<int>("value 1", Integer);
            MultiplyInput2 = new Parameter<int>("value 2", Integer);

            MultiplyOutput = new Parameter<int>("result", Integer);

            Multiply = new SystemProcess(
                "Multiply",
                "Multiplies two integers",
                (ValueSet inputs) =>
                {
                    int value1 = inputs.Get(MultiplyInput1);
                    int value2 = inputs.Get(MultiplyInput2);

                    var outputs = new ValueSet();
                    outputs.Set(MultiplyOutput, value1 * value2);

                    return new ProcessResult(outputs);
                },
                new Parameter[] { MultiplyInput1, MultiplyInput2 },
                new Parameter[] { MultiplyOutput },
                null
            );


            CompareInput1 = new Parameter<int>("value 1", Integer);
            CompareInput2 = new Parameter<int>("value 2", Integer);

            Compare = new SystemProcess(
                "compare",
                "Compare two integers",
                (ValueSet inputs) =>
                {
                    int value1 = inputs.Get(CompareInput1);
                    int value2 = inputs.Get(CompareInput2);

                    var comparison = value1.CompareTo(value2);
                    return new ProcessResult(comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal");
                },
                new Parameter[] { CompareInput1, CompareInput2 },
                null,
                new string[] { "less", "greater", "equal" }
            );


            EntryInput = new Parameter<int>("value", Integer);
            EntryOutput = new Parameter<int>("result", Integer);

            EntryProcess = new RequiredProcess(
                "Modify number",
                "Perform some operation(s) on a number",
                new Parameter[] { EntryInput },
                new Parameter[] { EntryOutput },
                null
            );


            Types = new DataType[] { this.Integer };
            SystemProcesses = new SystemProcess[] { Add, Subtract, Multiply, Compare };
            RequiredProcesses = new RequiredProcess[] { EntryProcess };
        }

        public RequiredProcess EntryProcess { get; }
        public Parameter<int> EntryInput { get; }
        public Parameter<int> EntryOutput { get; }

        public DataType<int> Integer { get; }

        public SystemProcess Add { get; }
        public Parameter<int> AddInput1 { get; }
        public Parameter<int> AddInput2 { get; }
        public Parameter<int> AddOutput { get; }

        public SystemProcess Subtract { get; }
        public Parameter<int> SubtractInput1 { get; }
        public Parameter<int> SubtractInput2 { get; }
        public Parameter<int> SubtractOutput { get; }

        public SystemProcess Multiply { get; }
        public Parameter<int> MultiplyInput1 { get; }
        public Parameter<int> MultiplyInput2 { get; }
        public Parameter<int> MultiplyOutput { get; }

        public SystemProcess Compare { get; }
        public Parameter<int> CompareInput1 { get; }
        public Parameter<int> CompareInput2 { get; }
    }
}
