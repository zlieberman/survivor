using UnityEngine;
using Survivor.Shared;

namespace Survivor.Environment
{
    [RequireComponent(typeof(BoxCollider))]
    public class WaterVolume : MonoBehaviour
    {
        [Header("Water Volume Settings")]
        public float waterHeight = 0f;
        public Vector3 volumeSize = new Vector3(20f, 5f, 20f);
        public Material waterMaterial;
        
        [Header("Water Physics")]
        public float buoyancyForce = 3f;
        public float dragForce = 1f;
        public float swimThreshold = 0.67f;
        
        [Header("Visual Settings")]
        public bool showWaterSurface = true;
        public GameObject waterSurfacePrefab;
        public float surfaceOffset = 0.1f;
        
        [Header("Swimming Settings")]
        public float swimSpeed = 4f;
        public float swimGravity = -2f;
        public float normalGravity = -15f;

        [Header("Underwater Visual Effects")]
        public bool enableUnderwaterEffects = true;
        public bool useAdvancedEffects = false;
        public Color underwaterTint = new Color(0.2f, 0.4f, 0.8f, 0.3f);
        public float underwaterBlur = 0.5f;
        public float underwaterDistortion = 0.1f;
        public float effectTransitionSpeed = 2f;
        public float effectIntensity = 1f;

        private BoxCollider waterCollider;
        private WaterPhysicsSystem waterSystem;
        private GameObject waterSurface;
        private MeshRenderer waterRenderer;

        private void Awake()
        {
            SetupWaterVolume();
        }

        private void SetupWaterVolume()
        {
            // Get or add required components
            waterCollider = GetComponent<BoxCollider>();
            waterSystem = GetComponent<WaterPhysicsSystem>();
            
            if (waterSystem == null)
            {
                waterSystem = gameObject.AddComponent<WaterPhysicsSystem>();
            }

            // Configure the collider
            waterCollider.isTrigger = true;
            waterCollider.size = volumeSize;
            waterCollider.center = new Vector3(0, -volumeSize.y / 2f, 0);

            // Configure the water system
            waterSystem.waterHeight = waterHeight;
            waterSystem.buoyancyForce = buoyancyForce;
            waterSystem.dragForce = dragForce;
            waterSystem.swimThreshold = swimThreshold;
            waterSystem.swimSpeed = swimSpeed;
            waterSystem.swimGravity = swimGravity;
            waterSystem.normalGravity = normalGravity;

            // Configure underwater effects
            waterSystem.enableUnderwaterEffects = enableUnderwaterEffects;
            waterSystem.useAdvancedEffects = useAdvancedEffects;
            waterSystem.underwaterTint = underwaterTint;
            waterSystem.underwaterBlur = underwaterBlur;
            waterSystem.underwaterDistortion = underwaterDistortion;
            waterSystem.effectTransitionSpeed = effectTransitionSpeed;
            waterSystem.effectIntensity = effectIntensity;

            // Create water surface if enabled
            if (showWaterSurface)
            {
                CreateWaterSurface();
            }
        }

        private void CreateWaterSurface()
        {
            // Remove existing surface
            if (waterSurface != null)
            {
                DestroyImmediate(waterSurface);
            }

            // Create water surface
            waterSurface = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterSurface.name = "WaterSurface";
            waterSurface.transform.parent = transform;
            waterSurface.transform.localPosition = new Vector3(0, waterHeight + surfaceOffset, 0);
            
            // Scale to match volume
            float scaleX = volumeSize.x / 10f; // Default plane is 10x10
            float scaleZ = volumeSize.z / 10f;
            waterSurface.transform.localScale = new Vector3(scaleX, 1, scaleZ);

            // Remove collider from surface (we use the volume collider)
            DestroyImmediate(waterSurface.GetComponent<Collider>());

            // Apply water material
            waterRenderer = waterSurface.GetComponent<MeshRenderer>();
            if (waterMaterial != null)
            {
                waterRenderer.material = waterMaterial;
            }
            else
            {
                // Create default water material
                CreateDefaultWaterMaterial();
            }
        }

        private void CreateDefaultWaterMaterial()
        {
            Material defaultWaterMat = new Material(Shader.Find("Standard"));
            defaultWaterMat.color = new Color(0.2f, 0.5f, 0.8f, 0.6f);
            defaultWaterMat.SetFloat("_Glossiness", 0.9f);
            defaultWaterMat.SetFloat("_Metallic", 0.0f);
            defaultWaterMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            defaultWaterMat.renderQueue = 3000;
            
            if (waterRenderer != null)
            {
                waterRenderer.material = defaultWaterMat;
            }
        }

        private void OnValidate()
        {
            // Update in editor
            if (Application.isPlaying)
            {
                SetupWaterVolume();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw water volume bounds
            Gizmos.color = new Color(0.2f, 0.5f, 0.8f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(new Vector3(0, -volumeSize.y / 2f, 0), volumeSize);
            
            // Draw water surface
            Gizmos.color = new Color(0.2f, 0.5f, 0.8f, 0.8f);
            Gizmos.DrawWireCube(new Vector3(0, waterHeight, 0), new Vector3(volumeSize.x, 0.1f, volumeSize.z));
        }

        // Public methods for runtime modification
        public void SetWaterHeight(float newHeight)
        {
            waterHeight = newHeight;
            if (waterSystem != null)
            {
                waterSystem.waterHeight = newHeight;
            }
            
            if (waterSurface != null)
            {
                waterSurface.transform.localPosition = new Vector3(0, waterHeight + surfaceOffset, 0);
            }
        }

        public void SetVolumeSize(Vector3 newSize)
        {
            volumeSize = newSize;
            if (waterCollider != null)
            {
                waterCollider.size = volumeSize;
                waterCollider.center = new Vector3(0, -volumeSize.y / 2f, 0);
            }
            
            if (waterSurface != null)
            {
                float scaleX = volumeSize.x / 10f;
                float scaleZ = volumeSize.z / 10f;
                waterSurface.transform.localScale = new Vector3(scaleX, 1, scaleZ);
            }
        }

        public bool IsPositionUnderwater(Vector3 position)
        {
            if (waterSystem != null)
            {
                return waterSystem.IsUnderwater(position);
            }
            return position.y < waterHeight;
        }

        public float GetSubmersionPercentage(Vector3 position, float objectHeight)
        {
            if (waterSystem != null)
            {
                return waterSystem.GetSubmersionPercentage(position, objectHeight);
            }
            
            // Fallback calculation
            float objectBottom = position.y - objectHeight / 2f;
            float objectTop = position.y + objectHeight / 2f;
            float submergedHeight = Mathf.Max(0, waterHeight - objectBottom);
            float totalHeight = objectTop - objectBottom;
            return Mathf.Clamp01(submergedHeight / totalHeight);
        }
    }
} 