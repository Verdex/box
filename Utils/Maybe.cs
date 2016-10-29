
namespace Box.Utils 
{
    public class Maybe<T>
    {
        public readonly T Item;
        public readonly bool HasValue;

        public Maybe( T item )
        {
            Item = item;
            HasValue = true;
        }

        public Maybe()
        {
            HasValue = false;
        }
    }
}
