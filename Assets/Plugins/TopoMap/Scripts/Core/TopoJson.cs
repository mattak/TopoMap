using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TopoMap
{
    [Serializable]
    public class TopoJson : JsonParsable<TopoJson>
    {
        public string type; // "Topology"
        public TopoTransform transform;
        public IDictionary<string, TopoObject> objects;
        public double[][][] arcs;
        public double[] bbox;
        
        public Func<double[], double[], double[]> transformPoint;

        public TopoJson Parse(IDictionary json)
        {
            if (json.Contains("type"))
            {
                this.type = (string) json["type"];
            }

            if (json.Contains("objects"))
            {
                this.objects = new Dictionary<string, TopoObject>();
                var dictionary = (IDictionary) json["objects"];

                foreach (var key in dictionary.Keys)
                {
                    this.objects[key.ToString()] = new TopoObject().Parse((IDictionary) dictionary[key]);
                }
            }

            if (json.Contains("arcs"))
            {
                var arcs = json["arcs"].ConvertToDoubleArrayArrayArray();
                this.arcs = arcs;
            }

            this.transformPoint = this.Identity;

            if (json.Contains("bbox"))
            {
                var collection = (ICollection) json["bbox"];
                this.bbox = new double[collection.Count];
                
                int i = 0;
                foreach (var v in collection)
                {
                    this.bbox[i++] = (double)v;
                }
            }

            if (json.Contains("transform"))
            {
                this.transform = new TopoTransform().Parse((IDictionary) json["transform"]);
                this.transformPoint = this.TransformPoint;
            }

            return this;
        }
        
        private double[] Identity(double[] point, double[] previous)
        {
            return new double[] {point[0], point[1]};
        }

        private double[] TransformPoint(double[] point, double[] previous)
        {
            if (previous == null)
            {
                previous = new double[] {0f, 0f};
            }

            previous[0] += point[0];
            previous[1] += point[1];

            double _x = previous[0] * this.transform.scale[0] + this.transform.translate[0];
            double _y = previous[1] * this.transform.scale[1] + this.transform.translate[1];
            
            return new double[2] {_x, _y};
        }

        public IEnumerable<Vector2> DecodeArc(double[][] arc)
        {
            double[] sum = new double[] {0f, 0f};
            
            return arc
                .Select(data => this.transformPoint(data, sum))
                .Select(data => new Vector2((float)data[0], (float)data[1]))
                .ToArray();
        }
    }

    [Serializable]
    public class TopoTransform
    {
        public double[] scale;
        public double[] translate;

        public TopoTransform Parse(IDictionary json)
        {
            if (json.Contains("scale"))
            {
                this.scale = json["scale"].ConvertToDoubleArray();
            }

            if (json.Contains("translate"))
            {
                this.translate = json["translate"].ConvertToDoubleArray();
            }

            return this;
        }
    }

    [Serializable]
    public class TopoObject
    {
        public string type; // GeometryCollection
        public IList<TopoGeometry> geometries;

        public TopoObject Parse(IDictionary json)
        {
            if (json.Contains("type"))
            {
                this.type = (string) json["type"];
            }

            if (json.Contains("geometries"))
            {
                var collection = (ICollection) json["geometries"];
                this.geometries = new List<TopoGeometry>();

                foreach (var geometry in collection)
                {
                    var dictionary = (IDictionary) geometry;
                    var typeString = (string) dictionary["type"];

                    var type = (TopoGeometryType) Enum.Parse(typeof(TopoGeometryType), typeString);

                    switch (type)
                    {
                        case TopoGeometryType.Polygon:
                            this.geometries.Add(new TopoGeometryPolygon().Parse(dictionary));
                            break;
                        case TopoGeometryType.MultiPolygon:
                            this.geometries.Add(new TopoGeometryMultiPolygon().Parse(dictionary));
                            break;
                        case TopoGeometryType.LineString:
                            this.geometries.Add(new TopoGeometryLineString().Parse(dictionary));
                            break;
                        case TopoGeometryType.MultiLineString:
                            this.geometries.Add(new TopoGeometryMultiLineString().Parse(dictionary));
                            break;
                        case TopoGeometryType.Point:
                            this.geometries.Add(new TopoGeometryPoint().Parse(dictionary));
                            break;
                        case TopoGeometryType.MultiPoint:
                            this.geometries.Add(new TopoGeometryMultiPoint().Parse(dictionary));
                            break;
                    }
                }
            }

            return this;
        }
    }

    [Serializable]
    public class TopoGeometry
    {
        public TopoGeometryType type;
        public IDictionary<string, object> properties;

        public virtual TopoGeometry Parse(IDictionary json)
        {
            if (json.Contains("type"))
            {
                this.type = (TopoGeometryType) Enum.Parse(typeof(TopoGeometryType), (string) json["type"]);
            }

            if (json.Contains("properties"))
            {
                this.properties = json["properties"].ConvertToDictionary<string, object>();
            }

            return this;
        }
    }

    [Serializable]
    public class TopoGeometryPoint : TopoGeometry
    {
        public int[] coordinates;

        public override TopoGeometry Parse(IDictionary json)
        {
            base.Parse(json);

            if (json.Contains("coordinates"))
            {
                this.coordinates = json["coordinates"].ConvertToIntArray();
            }

            return this;
        }
    }

    [Serializable]
    public class TopoGeometryMultiPoint : TopoGeometry
    {
        public int[][] coordinates;

        public override TopoGeometry Parse(IDictionary json)
        {
            base.Parse(json);

            if (json.Contains("coordinates"))
            {
                this.coordinates = json["coordinates"].ConvertToIntArrayArray();
            }

            return this;
        }
    }

    [Serializable]
    public class TopoGeometryLineString : TopoGeometry
    {
        public int[] arcs;

        public override TopoGeometry Parse(IDictionary json)
        {
            base.Parse(json);

            if (json.Contains("arcs"))
            {
                this.arcs = json["arcs"].ConvertToIntArray();
            }

            return this;
        }
    }

    [Serializable]
    public class TopoGeometryMultiLineString : TopoGeometry
    {
        // XXX
        public int[][] arcs;

        public override TopoGeometry Parse(IDictionary json)
        {
            base.Parse(json);

            if (json.Contains("arcs"))
            {
                this.arcs = json["arcs"].ConvertToIntArrayArray();
            }

            return this;
        }
    }

    [Serializable]
    public class TopoGeometryPolygon : TopoGeometry
    {
        public int[][] arcs;

        public override TopoGeometry Parse(IDictionary json)
        {
            base.Parse(json);

            if (json.Contains("arcs"))
            {
                this.arcs = json["arcs"].ConvertToIntArrayArray();
            }

            return this;
        }
    }

    [Serializable]
    public class TopoGeometryMultiPolygon : TopoGeometry
    {
        public int[][][] arcs;

        public override TopoGeometry Parse(IDictionary json)
        {
            base.Parse(json);

            if (json.Contains("arcs"))
            {
                this.arcs = json["arcs"].ConvertToIntArrayArrayArray();
            }

            return this;
        }
    }

    public static class TopoGeometryExtension
    {
        public static bool IsPoint(this TopoGeometry geometry)
        {
            return geometry.type == TopoGeometryType.Point;
        }

        public static bool IsMultiPoint(this TopoGeometry geometry)
        {
            return geometry.type == TopoGeometryType.MultiPoint;
        }

        public static bool IsLineString(this TopoGeometry geometry)
        {
            return geometry.type == TopoGeometryType.LineString;
        }

        public static bool IsMultiLineString(this TopoGeometry geometry)
        {
            return geometry.type == TopoGeometryType.MultiLineString;
        }

        public static bool IsPolygon(this TopoGeometry geometry)
        {
            return geometry.type == TopoGeometryType.Polygon;
        }

        public static bool IsMultiPolygon(this TopoGeometry geometry)
        {
            return geometry.type == TopoGeometryType.MultiPolygon;
        }

        public static TopoGeometryPoint AsPoint(this TopoGeometry geometry)
        {
            return (TopoGeometryPoint) geometry;
        }

        public static TopoGeometryMultiPoint AsMultiPoint(this TopoGeometry geometry)
        {
            return (TopoGeometryMultiPoint) geometry;
        }

        public static TopoGeometryLineString AsLineString(this TopoGeometry geometry)
        {
            return (TopoGeometryLineString) geometry;
        }

        public static TopoGeometryMultiLineString AsMultiLineString(this TopoGeometry geometry)
        {
            return (TopoGeometryMultiLineString) geometry;
        }

        public static TopoGeometryPolygon AsPolygon(this TopoGeometry geometry)
        {
            return (TopoGeometryPolygon) geometry;
        }

        public static TopoGeometryMultiPolygon AsMultiPolygon(this TopoGeometry geometry)
        {
            return (TopoGeometryMultiPolygon) geometry;
        }
    }

    public enum TopoGeometryType
    {
        Point,
        MultiPoint,
        LineString,
        MultiLineString,
        Polygon,
        MultiPolygon,
    }

    public static class JsonParserExtension
    {
        public static double[] ConvertToDoubleArray(this object obj)
        {
            var collection = (ICollection) obj;
            int index = 0;
            double[] result = new double[collection.Count];

            foreach (var element in collection)
            {
                result[index++] = Convert.ToDouble(element);
            }

            return result;
        }

        public static double[][] ConvertToDoubleArrayArray(this object obj)
        {
            var collection = (ICollection) obj;
            int index = 0;
            double[][] result = new double[collection.Count][];

            foreach (var element in collection)
            {
                result[index++] = element.ConvertToDoubleArray();
            }

            return result;
        }

        public static double[][][] ConvertToDoubleArrayArrayArray(this object obj)
        {
            var collection = (ICollection) obj;
            int index = 0;
            double[][][] result = new double[collection.Count][][];

            foreach (var element in collection)
            {
                result[index++] = element.ConvertToDoubleArrayArray();
            }

            return result;
        }

        public static int[] ConvertToIntArray(this object obj)
        {
            var collection = (ICollection) obj;
            int index = 0;
            int[] result = new int[collection.Count];

            foreach (var element in collection)
            {
                result[index++] = Convert.ToInt32(element);
            }

            return result;
        }

        public static int[][] ConvertToIntArrayArray(this object obj)
        {
            var collection = (ICollection) obj;
            int index = 0;
            int[][] result = new int[collection.Count][];

            foreach (var element in collection)
            {
                result[index++] = element.ConvertToIntArray();
            }

            return result;
        }

        public static int[][][] ConvertToIntArrayArrayArray(this object obj)
        {
            var collection = (ICollection) obj;
            int index = 0;
            int[][][] result = new int[collection.Count][][];

            foreach (var element in collection)
            {
                result[index++] = element.ConvertToIntArrayArray();
            }

            return result;
        }

        public static IDictionary<TKey, TValue> ConvertToDictionary<TKey, TValue>(this object obj)
        {
            var dictionary = (IDictionary) obj;
            var result = new Dictionary<TKey, TValue>();

            foreach (var key in dictionary.Keys)
            {
                var result_key = (TKey) key;
                result[result_key] = (TValue) dictionary[key];
            }

            return result;
        }
    }
}