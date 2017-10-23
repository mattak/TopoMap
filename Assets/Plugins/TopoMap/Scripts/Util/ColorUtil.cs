using UnityEngine;

namespace TopoMap.Util
{
    public static class ColorUtil
    {
        public static Color Random()
        {
            var rand = new System.Random();
            return new Color((float) rand.NextDouble(), (float) rand.NextDouble(), (float) rand.NextDouble());
        }
    }
}