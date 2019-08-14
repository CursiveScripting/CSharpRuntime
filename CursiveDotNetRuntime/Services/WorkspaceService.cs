using Cursive;

namespace CursiveRuntime.Services
{
    public class WorkspaceService
    {
        public void Clear(Workspace workspace)
        {
            foreach (var process in workspace.RequiredProcesses)
                process.Implementation = null;

            workspace.UserProcesses.Clear();
        }
    }
}
