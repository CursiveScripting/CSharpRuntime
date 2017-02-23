using Cursive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Processes
{
    public static class Value
    {
        private static ValueKey strValue1 = new ValueKey("value1", Program.text);
        private static ValueKey strValue2 = new ValueKey("value2", Program.text);

        public static SystemProcess EqualsText = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                outputs = null;
                return inputs[strValue1].Equals(inputs[strValue2]) ? "yes" : "no";
            },
            "Test to see if two values are equal.",
            new Cursive.ValueKey[] { strValue1, strValue2 },
            null,
            new string[] { "yes", "no" }
        );

        private static ValueKey iValue1 = new ValueKey("value1", Program.integer);
        private static ValueKey iValue2 = new ValueKey("value2", Program.integer);
        public static SystemProcess CompareIntegers = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                outputs = null;
                var value1 = inputs[iValue1];
                var value2 = inputs[iValue2];

                if (!(value1 is IComparable) || !(value2 is IComparable))
                    return "error";

                var comparison = (value1 as IComparable).CompareTo(value2 as IComparable);
                return comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal";
            },
            "Compare two integers. Returns 'error' if either value doesn't implement IComparable.",
            new Cursive.ValueKey[] { iValue1, iValue2 },
            null,
            new string[] { "less", "greater", "equal", "error" }
        );

        private static ValueKey person = new ValueKey("object", Program.person);
        private static ValueKey property = new Cursive.ValueKey("property", Program.text);
        private static ValueKey iValue = new Cursive.ValueKey("value", Program.integer);
        public static SystemProcess GetPropertyInteger = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                outputs = new ValueSet();

                var source = inputs[person];
                var propertyName = inputs[property].ToString();
                var prop = source.GetType().GetProperty(propertyName);
                if (prop == null)
                    return "error";

                try
                {
                    outputs[iValue] = prop.GetValue(source);
                }
                catch
                {
                    return "error";
                }
                return "ok";
            },
            "Output the named property of a given object. Returns 'error' if the property does not exist, or if getting it fails.",
            new Cursive.ValueKey[] { person, property },
            new Cursive.ValueKey[] { iValue },
            new string[] { "ok", "error" }
        );

        public static SystemProcess SetPropertyInteger = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                outputs = null;
                var destination = inputs[person];
                var prop = destination.GetType().GetProperty(inputs[property].ToString());
                if (prop == null)
                    return "error";

                try
                {
                    prop.SetValue(destination, inputs[iValue]);
                }
                catch
                {
                    return "error";
                }
                return "ok";
            },
            "Set the named property of a given object to the value specified. Returns 'error' if the property does not exist, or if setting it fails.",
            new Cursive.ValueKey[] { person, property, iValue },
            null,
            new string[] { "ok", "error" }
        );
    }
}
