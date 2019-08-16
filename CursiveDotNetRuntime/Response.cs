using System.Collections.Generic;
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

        public static Task<Response> SyncTask(string returnPath = "", ValueSet outputs = null)
        {
            return Task.FromResult(new Response(returnPath, outputs));
        }

        public static Task<Response> SyncTask(ValueSet outputs)
        {
            return Task.FromResult(new Response(outputs));
        }

        public string ReturnPath { get; }

        public ValueSet Outputs { get; }
    }
}