using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE.Processes
{
    public static class Value
    {
        public new static SystemProcess Equals = new SystemProcess((Model inputs, out Model outputs) =>
            {
                outputs = null;
                return inputs["value1"].Equals(inputs["value2"]) ? "yes" : "no";
            },
            "Value.Equals",
            "Test to see if two values are equal.",
            new string[] { "value1", "value2" },
            null,
            new string[] { "yes", "no" }
        );

        public static SystemProcess Compare = new SystemProcess((Model inputs, out Model outputs) =>
            {
                outputs = null;
                var value1 = inputs["value1"];
                var value2 = inputs["value2"];

                if (!(value1 is IComparable) || !(value2 is IComparable))
                    return "error";

                var comparison = (value1 as IComparable).CompareTo(value2 as IComparable);
                return comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal";
            },
            "Value.Compare",
            "Compare two values. Returns 'error' if either value doesn't implement IComparable.",
            new string[] { "value1", "value2" },
            null,
            new string[] { "less", "greater", "equal", "error" }
        );

        public static SystemProcess GetProperty = new SystemProcess((Model inputs, out Model outputs) =>
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
            "Value.GetProperty",
            "Output the named property of a given object. Returns 'error' if the property does not exist, or if getting it fails.",
            new string[] { "object", "property" },
            new string[] { "value" },
            new string[] { "ok", "error" }
        );

        public static SystemProcess SetProperty = new SystemProcess(
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
            "Value.SetProperty",
            "Set the named property of a given object to the value specified. Returns 'error' if the property does not exist, or if setting it fails.",
            new string[] { "object", "property", "value" },
            null,
            new string[] { "ok", "error" }
        );
    }
}
