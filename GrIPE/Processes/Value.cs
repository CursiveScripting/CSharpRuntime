using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE.Processes
{
    public static class Value
    {
        public static SystemProcess<T> Equals<T>(T compareTo) where T : IEquatable<T>
        {
            return new SystemProcess<T>(model => model.Equals(compareTo) ? "yes" : "no", "yes", "no");
        }

        public static SystemProcess<T> CompareTo<T>(T compareTo) where T : IComparable<T>
        {
            return new SystemProcess<T>(model =>
            {
                var comparison = model.CompareTo(compareTo);
                return comparison < 0 ? "less" : comparison > 0 ? "greater" : "equal";
            }, "less", "equal", "greater");
        }
    }
}
