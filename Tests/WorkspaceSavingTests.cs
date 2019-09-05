using Cursive;
using Xunit;

namespace Tests
{
    public class WorkspaceSavingTests
    {
        [Fact]
        public void SavedWorkspaceValidates()
        {
            var workspaceJson = new IntegerWorkspace().GetWorkspaceJson();

            Assert.NotNull(workspaceJson);

            var validationResult = Schemas.Validate(Schemas.Workspace, workspaceJson);

            Assert.True(validationResult.IsValid);
        }
    }
}
