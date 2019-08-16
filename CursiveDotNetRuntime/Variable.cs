[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]

namespace Cursive
{
    public class Variable : Parameter
    {
        public Variable(string name, DataType type, object initialValue = null)
            : base(name, type)
        {
            InitialValue = initialValue;
        }

        public object InitialValue { get; }
    }
}
