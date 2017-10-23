using System.IO;
using UnityEditor;
using UnityEngine;

namespace TopoMap
{
    public class TopoMapWindow : EditorWindow
    {
        [MenuItem("Window/TopoMap")]
        static void Open()
        {
            EditorWindow.GetWindow<TopoMapWindow>("TopoMap");
        }

        private GameObject saveObject;
        private string writePath = "Assets/Temp/Empty.prefab";

        void OnGUI()
        {
            EditorGUILayout.LabelField("Select Write Object");

            var newSaveObject = EditorGUILayout.ObjectField(
                "FieldSelector",
                this.saveObject,
                typeof(GameObject),
                true
            ) as GameObject;

            if (newSaveObject != this.saveObject)
            {
                this.saveObject = newSaveObject;
                this.writePath = "Assets/Temp/" + this.saveObject.name + ".prefab";
            }

            this.writePath = EditorGUILayout.TextField("WritePath", this.writePath);

            if (GUILayout.Button("Write"))
            {
                var dirPath = Path.GetDirectoryName(this.writePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                this.SaveMesh(dirPath, this.saveObject.GetComponentsInChildren<MeshFilter>());

                var prefab = PrefabUtility.CreateEmptyPrefab(this.writePath);
                PrefabUtility.ReplacePrefab(this.saveObject, prefab, ReplacePrefabOptions.ConnectToPrefab);
            }
        }

        void SaveMesh(string parentDir, MeshFilter[] filters)
        {
            foreach (var filter in filters)
            {
                var path = parentDir + "/" + filter.gameObject.name + "_" + filter.gameObject.GetInstanceID() +
                           ".asset";
                var mesh = filter.sharedMesh;
                // MeshUtility.Optimize(mesh);

                if (!File.Exists(path))
                {
                    AssetDatabase.CreateAsset(mesh, path);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}