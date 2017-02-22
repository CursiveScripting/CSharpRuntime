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
        public static SystemProcess EqualsText = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                outputs = null;
                return inputs["value1"].Equals(inputs["value2"]) ? "yes" : "no";
            },
            "Test to see if two values are equal.",
            new Parameter[] { new Parameter("value1", Program.text), new Parameter("value2", Program.text) },
            null,
            new string[] { "yes", "no" }
        );

        public static SystemProcess CompareIntegers = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                outputs = null;
                var value1 = inputs["value1"];
                var value2 = inputs["value2"];

                if (!(value1 is IComparable) || !(value2 is IComparable))
                    return "error";

                var comparison = (value1 as IComparable).CompareTo(value2 as IComparable);
                return comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal";
            },
            "Compare two integers. Returns 'error' if either value doesn't implement IComparable.",
            new Parameter[] { new Parameter("value1", Program.integer), new Parameter("value2", Program.integer) },
            null,
            new string[] { "less", "greater", "equal", "error" }
        );
        
        public static SystemProcess GetPropertyInteger = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                outputs = new ValueSet();

                var source = inputs["object"];
                var prop = source.GetType().GetProperty(inputs["property"].ToString());
                if (prop == null)
                    return "error";

                try
                {
                    outputs["value"] = prop.GetValue(source);
                }
                catch
                {
                    return "error";
                }
                return "ok";
            },
            "Output the named property of a given object. Returns 'error' if the property does not exist, or if getting it fails.",
            new Parameter[] { new Parameter("object", Program.person), new Parameter("property", Program.text) },
            new Parameter[] { new Parameter("value", Program.integer) },
            new string[] { "ok", "error" }
        );

        public static SystemProcess SetPropertyInteger = new SystemProcess(
            (ValueSet inputs, out ValueSet outputs) =>
            {
                outputs = null;
                var destination = inputs["object"];
                var prop = destination.GetType().GetProperty(inputs["property"].ToString());
                if (prop == null)
                    return "error";

                try
                {
                    prop.SetValue(destination, inputs["value"]);
                }
                catch
                {
                    return "error";
                }
                return "ok";
            },
            "Set the named property of a given object to the value specified. Returns 'error' if the property does not exist, or if setting it fails.",
            new Parameter[] { new Parameter("object", Program.person), new Parameter("property", Program.text), new Parameter("value", Program.integer) },
            null,
            new string[] { "ok", "error" }
        );
    }
}
