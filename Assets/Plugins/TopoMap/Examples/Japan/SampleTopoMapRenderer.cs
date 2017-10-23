using System.Linq;
using TopoMap.Util;
using UnityEngine;

namespace TopoMap.Example
{
    public class SampleTopoMapRenderer : MonoBehaviour
    {
        public TextAsset TopoJsonAsset;
        public MeshRenderer MeshPrefab;

        void Start()
        {
            var topo = TopoJsonParser.Parse(this.TopoJsonAsset.text);

            foreach (var keyvalue in topo.objects)
            {
                this.RenderTopoObject(topo, keyvalue.Value);
            }
        }

        void RenderTopoObject(TopoJson topo, TopoObject obj)
        {
            int geometryIndex = 0;

            foreach (var geometry in obj.geometries)
            {
                geometryIndex++;
                string name = null;

                if (geometry.properties != null)
                {
                    name = (string) geometry.properties.GetOrNull("nam");
                }
                if (name == null)
                {
                    name = geometryIndex.ToString();
                }

                if (geometry.IsMultiPolygon())
                {
                    var parentObject = this.NewGameObject(this.transform, name);

                    int[][] arcIndexesSet = geometry.AsMultiPolygon().arcs.Flatten().ToArray();

                    foreach (int[] arcIndexes in arcIndexesSet)
                    {
                        var verts = TopoPolygon.CreateVerts(arcIndexes, topo);
                        this.RenderGameObject(parentObject.transform, name, verts);
                    }
                }
                else if (geometry.IsPolygon())
                {
                    var parentObject = this.gameObject.transform;

                    int[] arcIndexes = geometry.AsPolygon().arcs.Flatten().ToArray();
                    var verts = TopoPolygon.CreateVerts(arcIndexes, topo);
                    this.RenderGameObject(parentObject.transform, name, verts);
                }
            }
        }

        GameObject RenderGameObject(Transform parentTransform, string name, Vector2[] verts)
        {
            return TopoPolygon.CreatePolygon(this.MeshPrefab.gameObject, parentTransform, verts, name);
        }

        GameObject NewGameObject(Transform parentTransform, string name)
        {
            var obj = new GameObject();
            obj.name = name;
            obj.transform.parent = parentTransform;
            return obj;
        }
    }
}