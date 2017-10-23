using System.Linq;
using TopoMap.Util;
using UnityEngine;

namespace TopoMap
{
    public static class TopoPolygon
    {
        public static GameObject CreatePolygon(GameObject Prefab, Transform transform, Vector2[] verts, string name)
        {
            var obj = GameObject.Instantiate(Prefab, transform, true) as GameObject;
            obj.name = name;
            CreatePolygonMesh(obj.GetComponent<MeshFilter>(), verts);
            return obj;
        }

        public static void CreatePolygonMesh(MeshFilter meshFilter, Vector2[] vertices)
        {
            Triangulator tr = new Triangulator(vertices);
            int[] indicies = tr.Triangulate();

            Mesh mesh = new Mesh();
            mesh.vertices = vertices.Select(it => new Vector3(it.x, 0, it.y)).ToArray();
            mesh.triangles = indicies;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;
        }

        public static Vector2[] CreateVerts(int[] arcIndexes, TopoJson topo)
        {
            return arcIndexes
                .Select(arcIndex =>
                {
                    int index = (arcIndex >= 0) ? arcIndex : BitwiseNot(arcIndex);
                    double[][] arc = topo.arcs[index];
                    var points = topo.DecodeArc(arc);
                    points = (arcIndex >= 0) ? points : points.Reverse();
                    points = points.Drop(1);
                    return points.ToArray();
                })
                .Flatten()
                .ToArray();
        }

        private static int BitwiseNot(int value)
        {
            return  -1 * value - 1;
        }
    }
}