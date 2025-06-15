using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace Survivor.Generation
{
    public class MaterialGenerator : MonoBehaviour
    {
        [MenuItem("GameObject/Survivor/Generate Materials")]
        public static void GenerateMaterials()
        {
            GenerateSandMaterial();
            GenerateWaterMaterial();
        }

        private static void GenerateSandMaterial()
        {
            // Create a procedural sand texture
            int textureSize = 512;
            Texture2D sandTexture = new Texture2D(textureSize, textureSize);
            
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    // Create a sandy color with some variation
                    float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    Color sandColor = new Color(
                        0.76f + noise * 0.1f,  // Red
                        0.7f + noise * 0.1f,   // Green
                        0.5f + noise * 0.1f,   // Blue
                        1f                     // Alpha
                    );
                    sandTexture.SetPixel(x, y, sandColor);
                }
            }
            
            sandTexture.Apply();
            
            // Save the texture
            string texturePath = "Assets/Materials/SandTexture.asset";
            AssetDatabase.CreateAsset(sandTexture, texturePath);
            
            // Create material
            Material sandMaterial = new Material(Shader.Find("Standard"));
            sandMaterial.mainTexture = sandTexture;
            sandMaterial.SetFloat("_Glossiness", 0.1f);  // Low glossiness for sand
            sandMaterial.SetFloat("_Metallic", 0.0f);    // No metallic for sand
            
            // Save the material
            AssetDatabase.CreateAsset(sandMaterial, "Assets/Materials/SandMaterial.mat");
            AssetDatabase.SaveAssets();
        }

        private static void GenerateWaterMaterial()
        {
            Material waterMaterial = new Material(Shader.Find("Standard"));
            
            // Set water properties
            waterMaterial.SetColor("_Color", new Color(0.0f, 0.5f, 1.0f, 0.5f));  // Blue, semi-transparent
            waterMaterial.SetFloat("_Glossiness", 0.9f);  // High glossiness for water
            waterMaterial.SetFloat("_Metallic", 0.0f);
            
            // Make it transparent
            waterMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            waterMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            waterMaterial.SetInt("_ZWrite", 0);
            waterMaterial.DisableKeyword("_ALPHATEST_ON");
            waterMaterial.EnableKeyword("_ALPHABLEND_ON");
            waterMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            waterMaterial.renderQueue = 3000;
            
            // Save the material
            AssetDatabase.CreateAsset(waterMaterial, "Assets/Materials/WaterMaterial.mat");
            AssetDatabase.SaveAssets();
        }
    }
}
#endif 