using System.Collections;
using MiniJSON;

namespace TopoMap
{
    public static class TopoJsonParser
    {
        public static TopoJson Parse(string jsonText)
        {
            var json = (IDictionary)Json.Deserialize(jsonText);
            TopoJson topoJson = new TopoJson().Parse(json);
            return topoJson;
        }
    }
}