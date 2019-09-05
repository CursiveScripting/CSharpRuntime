using Cursive;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tests
{
    public class IntegerWorkspace : Workspace
    {
        public IntegerWorkspace()
        {
            var integer = new FixedType<int>("integer", Color.FromKnownColor(KnownColor.Green), new Regex("[0-9]+"), s => int.Parse(s));


            SystemProcess add;

            {
                var input1 = new Parameter<int>("value 1", integer);
                var input2 = new Parameter<int>("value 2", integer);

                var output = new Parameter<int>("result", integer);

                add = new SystemProcess(
                    "Add",
                    "Adds two integers",
                    (ValueSet inputs) =>
                    {
                        int value1 = inputs.Get(input1);
                        int value2 = inputs.Get(input2);

                        var outputs = new ValueSet();
                        outputs.Set(output, value1 + value2);

                        return new ProcessResult(outputs);
                    },
                    new Parameter[] { input1, input2 },
                    new Parameter[] { output },
                    null
                );

                Add = async (int in1, int in2) =>
                {
                    var inputs = new ValueSet();

                    inputs.Set(input1, in1);
                    inputs.Set(input2, in2);

                    var result = await add.Run(inputs);

                    ValueSet outputs = result.Outputs;
                    return outputs.Get(output);
                };
            }


            SystemProcess subtract;

            {
                var input1 = new Parameter<int>("value 1", integer);
                var input2 = new Parameter<int>("value 2", integer);

                var output = new Parameter<int>("result", integer);

                subtract = new SystemProcess(
                    "Subtract",
                    "Subtracts one integer from another",
                    (ValueSet inputs) =>
                    {
                        int value1 = inputs.Get(input1);
                        int value2 = inputs.Get(input2);

                        var outputs = new ValueSet();
                        outputs.Set(output, value1 - value2);

                        return new ProcessResult(outputs);
                    },
                    new Parameter[] { input1, input2 },
                    new Parameter[] { output },
                    null
                );

                Subtract = async (int in1, int in2) =>
                {
                    var inputs = new ValueSet();

                    inputs.Set(input1, in1);
                    inputs.Set(input2, in2);

                    var result = await subtract.Run(inputs);

                    ValueSet outputs = result.Outputs;
                    return outputs.Get(output);
                };
            }


            SystemProcess multiply;

            {
                var input1 = new Parameter<int>("value 1", integer);
                var input2 = new Parameter<int>("value 2", integer);

                var output = new Parameter<int>("result", integer);

                multiply = new SystemProcess(
                    "Multiply",
                    "Multiplies two integers",
                    (ValueSet inputs) =>
                    {
                        int value1 = inputs.Get(input1);
                        int value2 = inputs.Get(input2);

                        var outputs = new ValueSet();
                        outputs.Set(output, value1 * value2);

                        return new ProcessResult(outputs);
                    },
                    new Parameter[] { input1, input2 },
                    new Parameter[] { output },
                    null
                );

                Multiply = async (int in1, int in2) =>
                {
                    var inputs = new ValueSet();

                    inputs.Set(input1, in1);
                    inputs.Set(input2, in2);

                    var result = await multiply.Run(inputs);

                    ValueSet outputs = result.Outputs;
                    return outputs.Get(output);
                };
            }


            SystemProcess compare;

            {
                var input1 = new Parameter<int>("value 1", integer);
                var input2 = new Parameter<int>("value 2", integer);

                compare = new SystemProcess(
                    "Compare",
                    "Compare two integers",
                    (ValueSet inputs) =>
                    {
                        int value1 = inputs.Get(input1);
                        int value2 = inputs.Get(input2);

                        var comparison = value1.CompareTo(value2);
                        return new ProcessResult(comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal");
                    },
                    new Parameter[] { input1, input2 },
                    null,
                    new string[] { "less", "greater", "equal" }
                );

                Compare = async (int in1, int in2) =>
                {
                    var inputs = new ValueSet();

                    inputs.Set(input1, in1);
                    inputs.Set(input2, in2);

                    var result = await compare.Run(inputs);

                    return result.ReturnPath;
                };
            }


            RequiredProcess modifyNumber;

            {
                var input = new Parameter<int>("value", integer);
                var output = new Parameter<int>("result", integer);

                modifyNumber = new RequiredProcess(
                    "Modify number",
                    "Perform some operation(s) on a number",
                    new Parameter[] { input },
                    new Parameter[] { output },
                    null
                );

                ModifyNumber = async (int in1) =>
                {
                    var inputs = new ValueSet();

                    inputs.Set(input, in1);

                    var result = await modifyNumber.Run(inputs);

                    ValueSet outputs = result.Outputs;
                    return outputs.Get(output);
                };
            }


            Types = new DataType[] { integer };
            SystemProcesses = new SystemProcess[] { add, subtract, multiply, compare };
            RequiredProcesses = new RequiredProcess[] { modifyNumber };
        }

        [JsonIgnore]
        public Func<int, Task<int>> ModifyNumber { get; }

        [JsonIgnore]
        public Func<int, int, Task<int>> Add { get; }

        [JsonIgnore]
        public Func<int, int, Task<int>> Subtract { get; }

        [JsonIgnore]
        public Func<int, int, Task<int>> Multiply { get; }

        [JsonIgnore]
        public Func<int, int, Task<string>> Compare { get; }
    }
}
