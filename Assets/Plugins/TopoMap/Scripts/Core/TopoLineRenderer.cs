using System.Linq;
using TopoMap.Util;
using UnityEngine;

namespace TopoMap
{
    public class TopoLineRenderer : MonoBehaviour
    {
        public GameObject Prefab;
        public Material RoadMaterial;

        void OnEnable()
        {
            this.DrawLine();
        }

        void DrawLine()
        {
//            var topo = TopoJsonParser.ParseFromResource("data/all_5_28_12.topojson");
//            var topoObject = topo.objects["roads"];
//
//            foreach (var geometry in topoObject.geometries)
//            {
//                if (geometry.IsLineString())
//                {
//                    // [1]
//                    CreateLine(geometry.AsLineString().arcs, topo);
//                }
//                else if (geometry.IsMultiLineString())
//                {
//                    // [[1], [2], [3]]
//                    CreateMultiLine(geometry.AsMultiLineString().arcs, topo);
//                }
//            }
        }

        private void CreateLine(int[] arcs, TopoJson topo)
        {
            Vector2[] points = arcs
                .Select(arcIndex => topo.DecodeArc(topo.arcs[arcIndex]).ToArray())
                .Flatten()
                .ToArray();

            var obj = TopoLine.CreateLine(this.Prefab, this.gameObject.transform, points, 0.01f);
            this.ApplyMaterial(obj, this.RoadMaterial);
        }

        private void CreateMultiLine(int[][] arcsSet, TopoJson topo)
        {
            foreach (var arcs in arcsSet)
            {
                CreateLine(arcs, topo);
            }
        }

        private GameObject ApplyMaterial(GameObject obj, Material material)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            renderer.material = material;
            return obj;
        }
    }
}