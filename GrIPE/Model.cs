using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class Model
    {
        SortedList<string, object> elements = new SortedList<string, object>();

        public object this[string key]
        {
            get
            {
                object o;
                if (elements.TryGetValue(key, out o))
                    return o;
                return null;
            }
            set
            {
                elements[key] = value;
            }
        }

    }
}
