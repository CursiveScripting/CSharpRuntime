using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cursive
{
    public class WorkspaceBuilder
    {
        private Dictionary<string, DataType> TypesByName { get; } = new Dictionary<string, DataType>();

        private Dictionary<Type, DataType> TypesByType { get; } = new Dictionary<Type, DataType>();

        private Dictionary<string, RequiredProcess> RequiredProcesses { get; } = new Dictionary<string, RequiredProcess>();

        private Dictionary<string, SystemProcess> SystemProcesses { get; } = new Dictionary<string, SystemProcess>();

        public WorkspaceBuilder AddType(DataType type)
        {
            if (TypesByName.ContainsKey(type.Name))
                throw new ArgumentException($"Another type already uses this name: {type.Name}");

            TypesByName[type.Name] = type;
            TypesByType[type.SystemType] = type;

            return this;
        }

        public WorkspaceBuilder AddType<T>(string name, Color color, Func<T> getDefault = null)
        {
            if (TypesByName.ContainsKey(name))
                throw new ArgumentException($"Another type already uses this name: {name}");

            var type = new DataType<T>(name, color, null, getDefault);

            TypesByName[type.Name] = type;
            TypesByType[type.SystemType] = type;

            return this;
        }

        public WorkspaceBuilder AddType<T>(string name, Color color, Regex validation, Func<string, T> deserialize, Func<T> getDefault = null)
        {
            if (TypesByName.ContainsKey(name))
                throw new ArgumentException($"Another type already uses this name: {name}");

            var type = new FixedType<T>(name, color, validation, deserialize, getDefault);

            TypesByName[type.Name] = type;
            TypesByType[type.SystemType] = type;

            return this;
        }

        public WorkspaceBuilder AddType(string name, Color color, params string[] allowedValues)
        {
            if (TypesByName.ContainsKey(name))
                throw new ArgumentException($"Another type already uses this name: {name}");

            var type = new LookupType(name, color, allowedValues);

            TypesByName[type.Name] = type;
            TypesByType[type.SystemType] = type;

            return this;
        }

        public WorkspaceBuilder AddTypes(params DataType[] types)
        {
            foreach (var type in types)
                AddType(type);

            return this;
        }

        public WorkspaceBuilder AddTypes(IEnumerable<DataType> types)
        {
            foreach (var type in types)
                AddType(type);

            return this;
        }

        public WorkspaceBuilder AddRequiredProcess(RequiredProcess process)
        {
            if (RequiredProcesses.ContainsKey(process.Name)
                || SystemProcesses.ContainsKey(process.Name))
                throw new ArgumentException($"Another process already uses this name: {process.Name}");

            RequiredProcesses.Add(process.Name, process);

            return this;
        }

        public WorkspaceBuilder AddRequiredProcesses(params RequiredProcess[] processes)
        {
            foreach (var process in processes)
                AddRequiredProcess(process);

            return this;
        }

        public WorkspaceBuilder AddRequiredProcesses(IEnumerable<RequiredProcess> processes)
        {
            foreach (var process in processes)
                AddRequiredProcess(process);

            return this;
        }

        public WorkspaceBuilder AddSystemProcess(SystemProcess process)
        {
            if (RequiredProcesses.ContainsKey(process.Name)
                || SystemProcesses.ContainsKey(process.Name))
                throw new ArgumentException($"Another process already uses this name: {process.Name}");

            SystemProcesses.Add(process.Name, process);

            return this;
        }

        // TODO: build system process from raw function?

        // TODO: build required process from raw signature types?

        public WorkspaceBuilder AddSystemProcesses(params SystemProcess[] processes)
        {
            foreach (var process in processes)
                AddSystemProcess(process);

            return this;
        }

        public WorkspaceBuilder AddSystemProcesses(IEnumerable<SystemProcess> processes)
        {
            foreach (var process in processes)
                AddSystemProcess(process);

            return this;
        }

        public Workspace Build()
        {
            return new Workspace
            {
                Types = TypesByName.Values.ToArray(),
                RequiredProcesses = RequiredProcesses.Values.ToArray(),
                SystemProcesses = SystemProcesses.Values.ToArray(),
            };
        }
    }
}
