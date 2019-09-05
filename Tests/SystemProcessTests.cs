using Cursive;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class SystemProcessTests
    {
        [Theory]
        [InlineData(1, 2, "less")]
        [InlineData(5, 1, "greater")]
        [InlineData(7, 7, "equal")]
        public async Task TestReturnPath(int val1, int val2, string returnPath)
        {
            var workspace = new IntegerWorkspace();

            var result = await workspace.Compare(val1, val2);

            Assert.Equal(returnPath, result);
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(0, 0, 0)]
        [InlineData(13, 7, 20)]
        public async Task TestOutputs(int val1, int val2, int sum)
        {
            var workspace = new IntegerWorkspace();

            var result = await workspace.Add(val1, val2);

            Assert.Equal(sum, result);
        }
    }
}
