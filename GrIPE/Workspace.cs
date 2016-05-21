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
        private SortedList<string, Type> typesByName = new SortedList<string, Type>();
        private SortedList<string, string> namesByType = new SortedList<string, string>();
        private SortedList<string, Process> processes = new SortedList<string, Process>();

        public void AddDataType(DataType dt)
        {
            typesByName.Add(dt.Name, dt.SystemType);
            namesByType.Add(dt.SystemType.FullName, dt.Name);
        }

        public void AddSystemProcess(string name, Func<Workspace, SystemProcess> process)
        {
            processes.Add(name, process(this));
        }

        internal void AddUserProcess(UserProcess process)
        {
            processes.Add(process.Name, process);
        }

        internal Type GetType(string name)
        {
            Type type;
            if (!typesByName.TryGetValue(name, out type))
                return null;
            return type;
        }

        internal string GetNameOfType(Type type)
        {
            string name;
            if (!namesByType.TryGetValue(type.FullName, out name))
                return null;
            return name;
        }

        internal Process GetProcess(string name)
        {
            Process process;
            if (!processes.TryGetValue(name, out process))
                return null;
            return process;
        }

        public bool Validate(out List<string> errors)
        {
            bool valid = true;
            List<string> processErrors;

            errors = new List<string>();
            foreach (var process in processes.Values)
            {
                if (!(process is UserProcess))
                    continue;

                var userProcess = process as UserProcess;
                if (!userProcess.Validate(this, out processErrors))
                    foreach (var error in processErrors)
                        errors.Add(string.Format("{0}: {1}", userProcess.Name, error));
            }

            return valid;
        }

        public void Clear()
        {
            var toRemove = new List<string>();
            foreach (var kvp in processes)
                if (kvp.Value is UserProcess)
                    toRemove.Add(kvp.Key);

            foreach (var key in toRemove)
                processes.Remove(key);
        }
    }
}
