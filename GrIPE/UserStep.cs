using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrIPE
{
    public class UserStep<ParentModel, ChildModel> : Step<ParentModel>
    {
        public UserStep(Process<ChildModel> childProcess, Func<ParentModel, ChildModel> adaptModel)
        {
            ChildProcess = childProcess;
            AdaptModel = adaptModel;
        }

        private Process<ChildModel> ChildProcess;
        private Func<ParentModel, ChildModel> AdaptModel;
        private Step<ParentModel> DefaultOutput;

        protected SortedList<string, Step<ParentModel>> outputs = new SortedList<string, Step<ParentModel>>();

        public void AddOutput(string name, Step<ParentModel> target)
        {
            outputs.Add(name, target);
        }

        public void SetDefaultOutput(Step<ParentModel> target)
        {
            DefaultOutput = target;
        }

        public override Step<ParentModel> Run(ParentModel model)
        {
            ChildModel childModel = AdaptModel(model);
            var outputName = ChildProcess.Run(childModel);

            Step<ParentModel> output;
            if (outputName == null || !outputs.TryGetValue(outputName, out output))
                return DefaultOutput;

            return output;
        }
    }
}