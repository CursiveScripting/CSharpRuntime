using Cursive;
using System;
using System.Collections.Generic;
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

            foreach (var type in workspace.TypesByName)
            {
                var node = doc.CreateElement("Type");
                node.Attributes.Append(doc.CreateAttribute("name", type.Key));

                if (type.Value.Validation != null)
                {
                    var regex = type.Value.Validation.ToString();
                    node.Attributes.Append(doc.CreateAttribute("validation", regex));
                }

                root.AppendChild(node);
            }

            foreach (var kvp in workspace.Processes)
            {
                var process = kvp.Value;
                if (!(process is SystemProcess))
                    continue;

                WriteProcess(workspace, process, kvp.Key, root, "SystemProcess");
            }

            foreach (var kvp in workspace.RequiredProcesses)
                WriteProcess(workspace, kvp.Value, kvp.Key, root, "SystemProcess");

            return doc;
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

                var type = workspace.GetType(input.Type);
                inputNode.Attributes.Append(doc.CreateAttribute("type", type.Name));
                processNode.AppendChild(inputNode);
            }

            foreach (var output in process.Outputs)
            {
                var outputNode = doc.CreateElement("Output");
                outputNode.Attributes.Append(doc.CreateAttribute("name", output.Name));

                var type = workspace.GetType(output.Type);
                outputNode.Attributes.Append(doc.CreateAttribute("type", type.Name));
                processNode.AppendChild(outputNode);
            }

            if (process.ReturnPaths.Count == 0)
                return;

            var returnPathsNode = doc.CreateElement("ReturnPaths");
            processNode.AppendChild(returnPathsNode);

            foreach (var path in process.ReturnPaths)
            {
                var pathNode = doc.CreateElement("Path");
                pathNode.Attributes.Append(doc.CreateAttribute("name", path));
                processNode.AppendChild(pathNode);
            }
        }
    }
}
