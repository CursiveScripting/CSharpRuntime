using Cursive;
using System.Collections.Generic;
using System.Linq;

namespace Cursive.Serialization
{
    /*
    internal static class ProcessValidationService
    {
        internal static bool ValidateProcess(Workspace workspace, UserProcess process, out List<string> errors)
        {
            var success = true;
            errors = new List<string>();

            // 0. all steps must have unique names
            var names = new SortedSet<string>();
            foreach (var step in process.Steps)
                if (names.Contains(step.ID))
                {
                    errors.Add(string.Format("More than one step uses the name '{0}' - names must be unique", step.ID));
                    success = false;
                }
            
            // 1. any child process must have all the input and output parameters mapped that it expects.
            foreach (var step in process.UserSteps)
            {
                if (step.ChildProcess.Inputs != null)
                    foreach (var parameter in step.ChildProcess.Inputs)
                        if (!step.InputMapping.ContainsKey(parameter))
                        {
                            errors.Add(string.Format("Step {0} requires the '{1}' input parameter, which has not been set.", step.ID, parameter.Name));
                            success = false;
                        }

                if (step.ChildProcess.Outputs != null)
                    foreach (var parameter in step.ChildProcess.Outputs)
                        if (!step.OutputMapping.ContainsKey(parameter))
                        {
                            errors.Add(string.Format("Step {0} requires the '{1}' output parameter, which has not been set.", step.ID, parameter.Name));
                            success = false;
                        }
            }

            // 2a. each start step must map every input defined for its process.
            // 2b. each start step must not map any input not defined for its process.
            foreach (var input in process.Inputs)
                if (!process.FirstStep.OutputMapping.ContainsKey(input))
                {
                    errors.Add(string.Format("The start step doesn't map the '{0}' input.", input.Name));
                    success = false;
                }

            foreach (var kvp in process.FirstStep.OutputMapping)
                if (!process.Inputs.Any(p => p == kvp.Key))
                {
                    errors.Add(string.Format("The start step maps the '{0}' input, which is not defined for this process.", kvp.Value));
                    success = false;
                }

            // 3a. each stop step must set every output defined for its process.
            // 3b. each stop step must not set any output not defined for its process.
            foreach (var step in process.StopSteps)
            {
                foreach (var output in process.Outputs)
                    if (!step.InputMapping.ContainsKey(output))
                    {
                        errors.Add(string.Format("The {0} stop step doesn't set the '{1}' output.", string.IsNullOrEmpty(step.ReturnValue) ? "default" : "'" + step.ReturnValue + "'", output.Name));
                        success = false;
                    }

                foreach (var kvp in step.InputMapping)
                    if (!process.Outputs.Any(p => p == kvp.Key))
                    {
                        errors.Add(string.Format("The {0} stop step sets the '{1}' output, which is not defined for this process.", string.IsNullOrEmpty(step.ReturnValue) ? "default" : "'" + step.ReturnValue + "'", kvp.Key.Name));
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
                            errors.Add(string.Format("Step {0} doesn't have a default return path, but doesn't map every possible output of it's function.", step.ID));
                            success = false;
                        }
            }

            // 5. every variable mapped into a step should either have a default, or should first have been set by every path that could lead to that point.
            // ...this is likely to be challenging

            return success;
        }
    }
    */
}
