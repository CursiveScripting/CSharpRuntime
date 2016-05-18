using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE.Processes
{
    public static class IO
    {
        public static SystemProcess<T> Print<T>(string message)
        {
            return new SystemProcess<T>(model =>
            {
                Console.WriteLine(message);
                return string.Empty;
            }, string.Empty);
        }
    }
}
