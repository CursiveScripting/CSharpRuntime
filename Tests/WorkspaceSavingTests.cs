using Cursive;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class WorkspaceSavingTests
    {
        [Test]
        public void SavedWorkspaceValidates()
        {
            var workspaceJson = JsonConvert.SerializeObject(new IntegerWorkspace());

            Assert.That(workspaceJson, Is.Not.Null);

            var validationErrors = Schemas.Workspace.Value.Validate(workspaceJson);

            Assert.IsEmpty(validationErrors);
        }
    }
}
