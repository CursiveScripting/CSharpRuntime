using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class Workspace
    {
        public Workspace(params DataType[] allowedTypes)
        {
            foreach (var dt in allowedTypes)
            {
                typesByName.Add(dt.Name, dt.SystemType);
                namesByType.Add(dt.SystemType.FullName, dt.Name);
            }
        }

        private SortedList<string, Type> typesByName = new SortedList<string, Type>();
        private SortedList<string, string> namesByType = new SortedList<string, string>();

        internal Type ResolveType(string name)
        {
            Type type;
            if (!typesByName.TryGetValue(name, out type))
                return null;
            return type;
        }
        internal string ResolveName(Type type)
        {
            string name;
            if (!namesByType.TryGetValue(type.FullName, out name))
                return null;
            return name;
        }
    }
}
