using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class UserStep : Step
    {
        public UserStep(Process childProcess)
        {
            this.childProcess = childProcess;
        }

        private Process childProcess;
        private Step defaultReturnPath;

        protected SortedList<string, Step> returnPaths = new SortedList<string, Step>();

        public void AddReturnPath(string name, Step nextStep)
        {
            returnPaths.Add(name, nextStep);
        }

        public void SetDefaultReturnPath(Step nextStep)
        {
            defaultReturnPath = nextStep;
        }

        public override Step Run(Model workspace)
        {
            Model inputs = new Model(), outputs;

            // set up fixed input parameters
            foreach (var kvp in fixedParameters)
                inputs[kvp.Key] = kvp.Value;

            // map any other input parameters in from the workspace
            foreach (var kvp in inputMapping)
                inputs[kvp.Key] = workspace[kvp.Value];
            
            // actually run the process, with the inputs named as it expects
            var outputName = childProcess.Run(inputs, out outputs);

            // map any output parameters back out into the workspace
            if (outputs != null)
                foreach (var kvp in outputMapping)
                    workspace[kvp.Value] = outputs[kvp.Key];

            Step output;
            if (outputName == null || !returnPaths.TryGetValue(outputName, out output))
                return defaultReturnPath;

            return output;
        }
    }
}