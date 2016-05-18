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
            ChildProcess = childProcess;
        }

        private Process ChildProcess;
        private Step DefaultReturnPath;

        protected SortedList<string, Step> returnPaths = new SortedList<string, Step>();

        public void AddReturnPath(string name, Step target)
        {
            returnPaths.Add(name, target);
        }

        public void SetDefaultReturnPath(Step target)
        {
            DefaultReturnPath = target;
        }

        public override Step Run(Model model)
        {
            Model inputs = new Model(), outputs;
            
            // TODO: Somehow set up input parameters ... map them from model properties, or read them in from this step's configuration

            var outputName = ChildProcess.Run(inputs, out outputs);

            // TODO: somehow *do* something with the outputs ... map them to model properties

            Step output;
            if (outputName == null || !returnPaths.TryGetValue(outputName, out output))
                return DefaultReturnPath;

            return output;
        }
    }
}