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

            var validationResult = Schemas.Validate(Schemas.Workspace, workspaceJson);

            Assert.True(validationResult.IsValid);
        }
    }
}
