using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cursive
{
    public abstract class Process
    {
        protected Process(string description)
        {
            this.Description = description;
        }

        public string Run(Model input)
        {
            Model output;
            return Run(input, out output);
        }

        public string Description { get; protected set; }

        public abstract string Run(Model input, out Model output);
        
        public abstract ReadOnlyCollection<string> ReturnPaths { get; }

        public abstract ReadOnlyCollection<Parameter> Inputs { get; }

        public abstract ReadOnlyCollection<Parameter> Outputs { get; }

        internal static bool SignaturesMatch(Process p1, Process p2)
        {
            if (!CollectionsMatch(p1.Inputs, p2.Inputs))
                return false;

            if (!CollectionsMatch(p1.Outputs, p2.Outputs))
                return false;

            if (!CollectionsMatch(p1.ReturnPaths, p2.ReturnPaths))
                return false;

            return true;
        }

        private static bool CollectionsMatch<T>(ReadOnlyCollection<T> p1s, ReadOnlyCollection<T> p2s)
            where T : IComparable<T>, IEquatable<T>
        {
            if (p1s != null)
            {
                if (p2s != null)
                {
                    if (p1s.Count != p2s.Count)
                        return false;

                    // TODO: should really sort the items first

                    using (var enum1 = p1s.GetEnumerator())
                    using (var enum2 = p2s.GetEnumerator())
                    {
                        while (enum1.MoveNext() && enum2.MoveNext())
                        {
                            if (enum1.Current != null && enum2.Current != null)
                            {
                                if (!enum1.Current.Equals(enum2.Current))
                                    return false;
                            }
                            else if (enum1.Current != null || enum2.Current != null)
                                return false;
                        } 
                    }
                }
                else
                    return false;
            }
            else if (p2s != null)
                return false;

            return true;
        }
    }
}