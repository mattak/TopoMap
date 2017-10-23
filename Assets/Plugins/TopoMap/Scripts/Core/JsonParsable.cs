using System.Collections;

namespace TopoMap
{
    public interface JsonParsable<T>
    {
        T Parse(IDictionary dictionary);
    }
}