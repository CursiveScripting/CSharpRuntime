using CursiveRuntime.Services;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Cursive
{
    public class Workspace
    {
        internal Dictionary<string, DataType> TypesByName { get; private set; } = new Dictionary<string, DataType>();
        internal Dictionary<string, Process> Processes { get; private set; } = new Dictionary<string, Process>();
        internal Dictionary<string, RequiredProcess> RequiredProcesses { get; private set; } = new Dictionary<string, RequiredProcess>();

        public void AddDataType(DataType dt)
        {
            TypesByName.Add(dt.Name, dt);
        }

        public void AddSystemProcess(string name, SystemProcess process)
        {
            Processes.Add(name, process);
            process.Name = name;
        }

        public void AddRequiredProcess(string name, RequiredProcess process)
        {
            RequiredProcesses.Add(name, process);
        }

        internal DataType GetType(string name)
        {
            DataType dt;
            if (!TypesByName.TryGetValue(name, out dt))
                return null;
            return dt;
        }

        public void Clear()
        {
            foreach (var process in RequiredProcesses.Values)
                process.ActualProcess = null;

            var toRemove = new List<string>();
            foreach (var kvp in Processes)
                if (kvp.Value is UserProcess)
                    toRemove.Add(kvp.Key);

            foreach (var key in toRemove)
                Processes.Remove(key);
        }

        public bool LoadProcesses(XmlDocument doc, out List<string> errors)
        {
            return ProcessLoadingService.LoadProcesses(this, doc, out errors);
        }

        public XmlDocument WriteForClient()
        {
            return WorkspaceSavingService.WriteDefinition(this);
        }
    }
}
