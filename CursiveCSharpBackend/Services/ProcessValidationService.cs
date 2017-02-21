using Cursive;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CursiveCSharpBackend.Services
{
    internal static class ProcessValidationService
    {
        internal static bool ValidateProcess(Workspace workspace, UserProcess process, out List<string> errors)
        {
            var success = true;
            errors = new List<string>();

            // 0. all steps must have unique names
            var names = new SortedSet<string>();
            foreach (var step in process.Steps)
                if (names.Contains(step.Name))
                {
                    errors.Add(string.Format("More than one step uses the name '{0}' - names must be unique", step.Name));
                    success = false;
                }


            // 1a. any input/output parameter being mapped into/out of a child process must be present in that child process.
            // 1b. any input parameter can only be mapped in OR have a fixed value, not both.
            // 1c. any child process must have all the input and output parameters mapped that it expects.
            foreach (var step in process.UserSteps)
            {
                foreach (var kvp in step.FixedInputs)
                {
                    if (step.ChildProcess.Inputs.FirstOrDefault(p => p.Name == kvp.Key) == null)
                    {
                        errors.Add(string.Format("The '{0}' step sets the '{1}' input parameter, which isn't defined for the '{0}' process.", step.Name, kvp.Key));
                        success = false;
                    }
                    if (step.InputMapping.ContainsKey(kvp.Key))
                    {
                        errors.Add(string.Format("The '{0}' step sets the '{1}' input parameter twice - mapping it in and also setting a fixed value.", step.Name, kvp.Key));
                        success = false;
                    }
                }

                foreach (var kvp in step.InputMapping)
                    if (step.ChildProcess.Inputs.FirstOrDefault(p => p.Name == kvp.Key) == null)
                    {
                        errors.Add(string.Format("The '{0}' step maps the '{1}' input parameter, which isn't defined for the '{0}' process.", step.Name, kvp.Key));
                        success = false;
                    }

                foreach (var kvp in step.OutputMapping)
                    if (step.ChildProcess.Outputs.FirstOrDefault(p => p.Name == kvp.Key) == null)
                    {
                        errors.Add(string.Format("The '{0}' step maps the '{1}' output parameter, which isn't defined for the '{0}' process.", step.Name, kvp.Key));
                        success = false;
                    }

                if (step.ChildProcess.Inputs != null)
                    foreach (var parameter in step.ChildProcess.Inputs)
                        if (!step.FixedInputs.HasElement(parameter.Name) && !step.InputMapping.ContainsKey(parameter.Name))
                        {
                            errors.Add(string.Format("The '{0}' step requires the '{1}' input parameter, which has not been set.", step.Name, parameter.Name));
                            success = false;
                        }

                if (step.ChildProcess.Outputs != null)
                    foreach (var parameter in step.ChildProcess.Outputs)
                        if (!step.OutputMapping.ContainsKey(parameter.Name))
                        {
                            errors.Add(string.Format("The '{0}' step requires the '{1}' output parameter, which has not been set.", step.Name, parameter.Name));
                            success = false;
                        }
            }

            // 2a. each start step must map every input defined for its process.
            // 2b. each start step must not map any input not defined for its process.
            foreach (var input in process.Inputs)
                if (!process.FirstStep.OutputMapping.ContainsValue(input.Name))
                {
                    errors.Add(string.Format("The start step doesn't map the '{0}' input.", input.Name));
                    success = false;
                }

            foreach (var kvp in process.FirstStep.OutputMapping)
                if (process.Inputs.FirstOrDefault(p => p.Name == kvp.Value) == null)
                {
                    errors.Add(string.Format("The start step maps the '{0}' input, which is not defined for this process.", kvp.Value));
                    success = false;
                }

            // 3a. each end step must set every output defined for its process.
            // 3b. each end step must not set any output not defined for its process.
            foreach (var step in process.EndSteps)
            {
                foreach (var output in process.Outputs)
                    if (!step.InputMapping.ContainsValue(output.Name))
                    {
                        errors.Add(string.Format("The {0} end step doesn't set the '{1}' output.", string.IsNullOrEmpty(step.ReturnValue) ? "default" : "'" + step.ReturnValue + "'", output.Name));
                        success = false;
                    }

                foreach (var kvp in step.InputMapping)
                    if (process.Outputs.FirstOrDefault(p => p.Name == kvp.Value) == null)
                    {
                        errors.Add(string.Format("The {0} end step sets the '{1}' output, which is not defined for this process.", string.IsNullOrEmpty(step.ReturnValue) ? "default" : "'" + step.ReturnValue + "'", kvp.Value));
                        success = false;
                    }
            }

            // 4. every step with multiple return paths must either have EVERY path mapped, or have a default return path mapped.
            foreach (var step in process.UserSteps)
            {
                if (step.DefaultReturnPath == null)
                    foreach (var path in step.ChildProcess.ReturnPaths)
                        if (!step.ReturnPaths.ContainsKey(path))
                        {
                            errors.Add(string.Format("The '{0}' step doesn't have a default return path, but doesn't map every possible output of it's function.", step.Name));
                            success = false;
                        }
            }

            // 5. every variable must always be treated as the same type by everything that reads from it or writes to it.
            var variableTypes = new Dictionary<string, Type>();
            foreach (var input in process.Inputs)
                variableTypes[input.Name] = input.Type;

            foreach (var step in process.UserSteps)
            {
                foreach (var kvp in step.InputMapping)
                {
                    string varName = kvp.Value;
                    Type varType = step.ChildProcess.Inputs.Single(p => p.Name == kvp.Key).Type;

                    Type prevType;
                    if (variableTypes.TryGetValue(varName, out prevType))
                    {
                        if (prevType != varType && !prevType.IsAssignableFrom(varType) && !varType.IsAssignableFrom(prevType))
                        {
                            errors.Add(string.Format("The '{0}' step expects the '{1}' input parameter to be '{2}', but it has been declared as '{3}' elsewhere.", step.Name, varName, workspace.GetType(varType).Name, workspace.GetType(prevType).Name));
                            success = false;
                        }
                    }
                    else
                        variableTypes[varName] = varType;
                }

                foreach (var kvp in step.OutputMapping)
                {
                    string varName = kvp.Value;
                    Type varType = step.ChildProcess.Outputs.Single(p => p.Name == kvp.Key).Type;

                    Type prevType;
                    if (variableTypes.TryGetValue(varName, out prevType))
                    {
                        if (prevType != varType)
                        {
                            errors.Add(string.Format("The '{0}' step expects the '{1}' output parameter to be '{2}', but it has been declared as '{3}' elsewhere.", step.Name, varName, workspace.GetType(varType).Name, workspace.GetType(prevType).Name));
                            success = false;
                        }
                    }
                    else
                        variableTypes[varName] = varType;
                }
            }
            foreach (var kvp in process.FirstStep.OutputMapping)
            {
                string varName = kvp.Value;
                Type varType = process.Inputs.Single(p => p.Name == kvp.Key).Type;

                Type prevType;
                if (variableTypes.TryGetValue(varName, out prevType))
                {
                    if (prevType != varType)
                    {
                        errors.Add(string.Format("The start step expects the '{0}' output parameter to be '{1}', but it has been declared as '{2}' elsewhere.", varName, workspace.GetType(varType).Name, workspace.GetType(prevType).Name));
                        success = false;
                    }
                }
                else
                    variableTypes[varName] = varType;
            }
            foreach (var step in process.EndSteps)
            {
                foreach (var kvp in step.InputMapping)
                {
                    string varName = kvp.Key;
                    var output = process.Outputs.FirstOrDefault(p => p.Name == kvp.Value);
                    if (output == null)
                        continue;
                    Type varType = output.Type;

                    Type prevType;
                    if (variableTypes.TryGetValue(varName, out prevType))
                    {
                        if (prevType != varType)
                        {
                            errors.Add(string.Format("The '{0}' end step expects the '{1}' output parameter to be '{2}' (this is how the process declares it), but it has been declared as '{3}' elsewhere.", step.ReturnValue, varName, workspace.GetType(varType).Name, workspace.GetType(prevType).Name));
                            success = false;
                        }
                    }
                    else
                        variableTypes[varName] = varType;
                }
            }


            // 6. every variable mapped into a step should either have a default, or should first have been set by every path that could lead to that point.
            // ...this is likely to be challenging

            return success;
        }
    }
}
