using Cursive;
using Newtonsoft.Json;
using Xunit;

namespace Tests
{
    public class WorkspaceSavingTests
    {
        [Fact]
        public void SavedWorkspaceValidates()
        {
            var workspaceJson = JsonConvert.SerializeObject(new IntegerWorkspace());

            Assert.NotNull(workspaceJson);

            var validationErrors = Schemas.Workspace.Value.Validate(workspaceJson);

            Assert.Empty(validationErrors);
        }
    }
}
