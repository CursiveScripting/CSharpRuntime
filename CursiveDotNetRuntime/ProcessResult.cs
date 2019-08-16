namespace Cursive
{
    public struct ProcessResult
    {
        public ProcessResult(string returnPath = "", ValueSet outputs = null)
        {
            ReturnPath = returnPath;
            Outputs = outputs;
        }

        public ProcessResult(ValueSet outputs)
        {
            ReturnPath = null;
            Outputs = outputs;
        }

        public string ReturnPath { get; }

        public ValueSet Outputs { get; }
    }
}