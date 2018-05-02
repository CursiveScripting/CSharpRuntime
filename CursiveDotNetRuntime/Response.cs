using System.Threading.Tasks;

namespace Cursive
{
    public struct Response
    {
        public Response(string returnPath = "", ValueSet outputs = null)
        {
            ReturnPath = returnPath;
            Outputs = outputs;
        }

        public Response(ValueSet outputs)
        {
            ReturnPath = null;
            Outputs = outputs;
        }

        public static Task<Response> Task(string returnPath = "", ValueSet outputs = null)
        {
            return System.Threading.Tasks.Task.FromResult(new Response(returnPath, outputs));
        }

        public static Task<Response> Task(ValueSet outputs)
        {
            return System.Threading.Tasks.Task.FromResult(new Response(outputs));
        }

        public string ReturnPath { get; }
        public ValueSet Outputs { get; }
    }
}