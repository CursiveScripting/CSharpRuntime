using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE.Processes
{
    public static class Date
    {
        public static readonly Func<Workspace,SystemProcess> GetDayOfWeek = workspace => new SystemProcess(
            workspace,
            (Model inputs, out Model outputs) =>
            {
                outputs = null;
                return DateTime.Today.DayOfWeek.ToString();
            },
            "Returns the name of the current day of the week",
            null, null,
            new string[] {
                DayOfWeek.Monday.ToString(),
                DayOfWeek.Tuesday.ToString(),
                DayOfWeek.Wednesday.ToString(),
                DayOfWeek.Thursday.ToString(),
                DayOfWeek.Friday.ToString(),
                DayOfWeek.Saturday.ToString(),
                DayOfWeek.Sunday.ToString()
            }
        );
    }
}
