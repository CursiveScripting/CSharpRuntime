using Cursive;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace CursiveCSharpBackend.Services
{
    internal static class WorkspaceSavingService
    {
        internal static XmlDocument WriteDefinition(Workspace workspace)
        {
            var doc = new XmlDocument();

            var root = doc.CreateElement("Workspace");
            doc.AppendChild(root);

            Queue<DataType> typesToWrite = new Queue<DataType>(workspace.TypesByName.Values);
            HashSet<DataType> typesWritten = new HashSet<DataType>();

            while (typesToWrite.Any())
            {
                DataType type = typesToWrite.Dequeue();

                if (type.Extends != null && !typesWritten.Contains(type.Extends))
                {
                    typesToWrite.Enqueue(type);
                    continue;
                }

                WriteType(root, type);
                typesWritten.Add(type);
            }

            foreach (var kvp in workspace.Processes)
            {
                var process = kvp.Value;
                if (!(process is SystemProcess))
                    continue;

                WriteProcess(workspace, process, kvp.Key, root, "SystemProcess");
            }

            foreach (var kvp in workspace.RequiredProcesses)
            {
                WriteProcess(workspace, kvp.Value, kvp.Key, root, "SystemProcess");
            }

            return doc;
        }

        public static string ToHexString(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

        private static void WriteType(XmlElement parent, DataType type)
        {
            var doc = parent.OwnerDocument;
            var node = doc.CreateElement("Type");
            node.Attributes.Append(doc.CreateAttribute("name", type.Name));
            node.Attributes.Append(doc.CreateAttribute("color", type.Color.ToHexString()));

            if (type.Validation != null)
            {
                var regex = type.Validation.ToString();
                node.Attributes.Append(doc.CreateAttribute("validation", regex));
            }

            if (type.Extends != null)
            {
                node.Attributes.Append(doc.CreateAttribute("extends", type.Extends.Name));
            }

            if (!string.IsNullOrEmpty(type.Guidance))
            {
                node.Attributes.Append(doc.CreateAttribute("guidance", type.Guidance));
            }

            parent.AppendChild(node);
        }

        private static void WriteProcess(Workspace workspace, Process process, string processName, XmlElement parent, string elementName)
        {
            var doc = parent.OwnerDocument;
            var processNode = doc.CreateElement(elementName);
            processNode.Attributes.Append(doc.CreateAttribute("name", processName));
            parent.AppendChild(processNode);

            foreach (var input in process.Inputs)
            {
                var inputNode = doc.CreateElement("Input");
                inputNode.Attributes.Append(doc.CreateAttribute("name", input.Name));

                var type = input.Type;
                inputNode.Attributes.Append(doc.CreateAttribute("type", type.Name));
                processNode.AppendChild(inputNode);
            }

            foreach (var output in process.Outputs)
            {
                var outputNode = doc.CreateElement("Output");
                outputNode.Attributes.Append(doc.CreateAttribute("name", output.Name));

                var type = output.Type;
                outputNode.Attributes.Append(doc.CreateAttribute("type", type.Name));
                processNode.AppendChild(outputNode);
            }
            
            foreach (var path in process.ReturnPaths)
            {
                var pathNode = doc.CreateElement("NamedReturnPath");
                pathNode.Attributes.Append(doc.CreateAttribute("name", path));
                processNode.AppendChild(pathNode);
            }
        }
    }
}
