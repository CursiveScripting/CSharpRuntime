namespace Cursive
{
    internal abstract class ReturningStep : Step
    {
        protected ReturningStep(string id)
            : base(id) { }

        internal Step DefaultReturnPath { get; set; }
    }
}