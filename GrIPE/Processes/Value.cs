using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE.Processes
{
    public static class Value
    {
        public static SystemProcess Equals<T>() where T : IEquatable<T>
        {
            return new SystemProcess((Model inputs, out Model outputs) =>
                {
                    outputs = null;
                    return inputs["value2"].Equals(inputs["value1"]) ? "yes" : "no";
                },
                new string[] { "value1", "value2" },
                null,
                new string[] { "yes", "no" }
            );
        }

        /*
        public static SystemProcess CompareTo<T>() where T : IComparable<T>
        {
            return new SystemProcess((process, model) =>
            {
                var comparison = model.CompareTo(process.ReadInputParameter("value"));
                return comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal";
            }, new string[] { "value" }, null, "less", "equal", "greater");
        }
        */
    }
}
