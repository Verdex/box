using System.Collections.Generic;
using System.Linq;

namespace Box.Utils
{
    public class DisplayArray<T>
    {
        public readonly IEnumerable<T> _enumerable;

        public DisplayArray( IEnumerable<T> enumerable )
        {
            _enumerable = enumerable.ToArray();
        }

        public override string ToString()
        {
            return " [ " + string.Join( " ", _enumerable.Select( e => e.ToString() ) ) + " ] ";
        }

    }
}
