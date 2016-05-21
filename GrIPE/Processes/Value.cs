using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE.Processes
{
    public static class Value
    {
        public static Func<Workspace, SystemProcess> EqualsText = workspace => new SystemProcess(
            workspace, 
            (Model inputs, out Model outputs) =>
            {
                outputs = null;
                return inputs["value1"].Equals(inputs["value2"]) ? "yes" : "no";
            },
            "Test to see if two values are equal.",
            new Parameter[] { new Parameter("value1", typeof(string)), new Parameter("value2", typeof(string)) },
            null,
            new string[] { "yes", "no" }
        );

        public static Func<Workspace, SystemProcess> CompareIntegers = workspace => new SystemProcess(
            workspace, 
            (Model inputs, out Model outputs) =>
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
            new Parameter[] { new Parameter("value1", typeof(int)), new Parameter("value2", typeof(int)) },
            null,
            new string[] { "less", "greater", "equal", "error" }
        );

        public static Func<Workspace, SystemProcess> GetPropertyInteger = workspace => new SystemProcess(
            workspace,
            (Model inputs, out Model outputs) =>
            {
                outputs = new Model();

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
            new Parameter[] { new Parameter("object", typeof(object)), new Parameter("property", typeof(string)) },
            new Parameter[] { new Parameter("value", typeof(int)) },
            new string[] { "ok", "error" }
        );

        public static Func<Workspace, SystemProcess> SetPropertyInteger = workspace => new SystemProcess(
            workspace,
            (Model inputs, out Model outputs) =>
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
            new Parameter[] { new Parameter("object", typeof(object)), new Parameter("property", typeof(string)), new Parameter("value", typeof(int)) },
            null,
            new string[] { "ok", "error" }
        );
    }
}
