using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Survivor.Generation
{
    public class TreePrefabGenerator : MonoBehaviour
    {
        private static readonly Color TRUNK_COLOR = new Color(0.45f, 0.25f, 0.1f); // Warm brown color

        [MenuItem("GameObject/Survivor/Generate Tree Prefabs", false, 10)]
        public static void GenerateTreePrefabs()
        {
            GenerateBasicTree("SmallTree", 3f, 0.3f, Color.green * 0.8f, TRUNK_COLOR);
            GenerateBasicTree("MediumTree", 4f, 0.4f, Color.green * 0.7f, TRUNK_COLOR * 0.9f);
            GenerateBasicTree("LargeTree", 5f, 0.5f, Color.green * 0.6f, TRUNK_COLOR * 0.8f);
            
            // Force Unity to refresh the asset database
            AssetDatabase.Refresh();
        }

        private static void GenerateBasicTree(string name, float height, float width, Color leafColor, Color trunkColor)
        {
            // Create parent object
            GameObject treeObject = new GameObject(name);

            // Create trunk
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(treeObject.transform);
            trunk.transform.localPosition = new Vector3(0, height * 0.4f, 0);
            trunk.transform.localScale = new Vector3(width * 0.3f, height * 0.8f, width * 0.3f);

            // Create foliage (using a capsule for a more natural look)
            GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            foliage.name = "Foliage";
            foliage.transform.SetParent(treeObject.transform);
            foliage.transform.localPosition = new Vector3(0, height * 0.8f, 0);
            foliage.transform.localScale = new Vector3(width * 2f, height * 0.6f, width * 2f);

            // Create materials
            Material trunkMaterial = new Material(Shader.Find("Standard"));
            trunkMaterial.color = trunkColor;
            trunk.GetComponent<MeshRenderer>().material = trunkMaterial;

            Material foliageMaterial = new Material(Shader.Find("Standard"));
            foliageMaterial.color = leafColor;
            foliage.GetComponent<MeshRenderer>().material = foliageMaterial;

            // Save materials
            string materialPath = "Assets/Prefabs/Trees/Materials";
            if (!AssetDatabase.IsValidFolder(materialPath))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs/Trees", "Materials");
            }
            AssetDatabase.CreateAsset(trunkMaterial, $"{materialPath}/{name}_Trunk.mat");
            AssetDatabase.CreateAsset(foliageMaterial, $"{materialPath}/{name}_Foliage.mat");

            // Create the prefab
            string prefabPath = $"Assets/Prefabs/Trees/{name}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(treeObject, prefabPath);
            
            if (prefab != null)
            {
                Debug.Log($"Created tree prefab: {prefabPath}");
            }
            else
            {
                Debug.LogError($"Failed to create tree prefab: {name}");
            }

            // Cleanup
            DestroyImmediate(treeObject);
        }
    }
}
#endif 