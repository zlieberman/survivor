using UnityEngine;
using Crest;
using Survivor.Generation;

namespace Survivor.Generation
{
    /// <summary>
    /// Automatically configures OceanDepthCache to work with ProceduralIslandGenerator
    /// This script should be attached to the same GameObject as ProceduralIslandGenerator
    /// </summary>
    [DefaultExecutionOrder(-99)] // Run after ProceduralIslandGenerator
    public class OceanDepthCacheSetup : MonoBehaviour
    {
        [Header("Ocean Depth Cache Settings")]
        [Tooltip("The layer mask for the OceanDepthCache to capture")]
        public LayerMask terrainLayers = 1; // Default layer
        
        [Tooltip("Resolution of the depth cache texture")]
        [UnityEngine.Range(256, 2048)]
        public int resolution = 1024;
        
        [Tooltip("Maximum terrain height for the depth cache camera")]
        public float maxTerrainHeight = 100f;
        
        [Tooltip("Far clip plane for the depth cache camera")]
        public float farClipPlane = 10000f;
        
        [Tooltip("Scale multiplier for the OceanDepthCache (relative to island size)")]
        [UnityEngine.Range(1f, 3f)]
        public float scaleMultiplier = 1.5f;

        private ProceduralIslandGenerator islandGenerator;
        private OceanDepthCache oceanDepthCache;
        private OceanRenderer oceanRenderer;

        private void Awake()
        {
            // Find components
            islandGenerator = GetComponent<ProceduralIslandGenerator>();
            oceanRenderer = FindObjectOfType<OceanRenderer>();
            
            if (islandGenerator == null)
            {
                Debug.LogError("[OceanDepthCacheSetup] No ProceduralIslandGenerator found on this GameObject!");
                return;
            }
            
            if (oceanRenderer == null)
            {
                Debug.LogError("[OceanDepthCacheSetup] No OceanRenderer found in scene!");
                return;
            }
        }

        private void Start()
        {
            // Wait for island generation to complete
            if (islandGenerator != null)
            {
                StartCoroutine(SetupOceanDepthCacheAfterGeneration());
            }
        }

        private System.Collections.IEnumerator SetupOceanDepthCacheAfterGeneration()
        {
            // Wait for island generation to complete
            while (!islandGenerator.IsGenerationComplete())
            {
                yield return null;
            }
            
            // Give it one more frame to ensure everything is set up
            yield return new WaitForEndOfFrame();
            
            // Now set up the OceanDepthCache
            SetupOceanDepthCache();
        }

        private void SetupOceanDepthCache()
        {
            // Find existing OceanDepthCache or create one
            oceanDepthCache = FindObjectOfType<OceanDepthCache>();
            
            if (oceanDepthCache == null)
            {
                Debug.LogWarning("[OceanDepthCacheSetup] No OceanDepthCache found in scene. Creating one...");
                CreateOceanDepthCache();
            }
            else
            {
                Debug.Log("[OceanDepthCacheSetup] Found existing OceanDepthCache, configuring it...");
                ConfigureOceanDepthCache(oceanDepthCache);
            }
        }

        private void CreateOceanDepthCache()
        {
            // Create new GameObject for OceanDepthCache
            GameObject depthCacheGO = new GameObject("OceanDepthCache");
            oceanDepthCache = depthCacheGO.AddComponent<OceanDepthCache>();
            
            ConfigureOceanDepthCache(oceanDepthCache);
        }

        private void ConfigureOceanDepthCache(OceanDepthCache depthCache)
        {
            if (islandGenerator == null) return;

            // Get island information
            Vector3 islandCenter = islandGenerator.GetIslandCenter();
            float islandSize = islandGenerator.GetIslandSize();
            float islandRadius = islandGenerator.IslandRadius;
            
            // Calculate appropriate scale for the OceanDepthCache
            float cacheScale = islandSize * scaleMultiplier;
            
            // Position the OceanDepthCache at the island center, at water level
            Vector3 cachePosition = islandCenter;
            cachePosition.y = oceanRenderer != null ? oceanRenderer.transform.position.y : 0f;
            
            // Configure the OceanDepthCache
            depthCache.transform.position = cachePosition;
            depthCache.transform.localScale = new Vector3(cacheScale, 1f, cacheScale);
            depthCache.transform.rotation = Quaternion.identity;
            
            // Set the layer mask to capture terrain
            depthCache._layers = terrainLayers;
            
            // Set resolution and camera settings
            depthCache._resolution = resolution;
            depthCache._cameraMaxTerrainHeight = maxTerrainHeight;
            depthCache._cameraFarClipPlane = farClipPlane;
            
            // Note: Type and RefreshMode are set to defaults (Realtime and OnStart)
            // which should work correctly for procedurally generated terrain
            
            // Populate the cache
            depthCache.PopulateCache(updateComponents: true);
            
            Debug.Log($"[OceanDepthCacheSetup] Configured OceanDepthCache at {cachePosition} with scale {cacheScale}");
            Debug.Log($"[OceanDepthCacheSetup] Island center: {islandCenter}, Island size: {islandSize}, Island radius: {islandRadius}");
            Debug.Log($"[OceanDepthCacheSetup] OceanDepthCache type: {depthCache.Type}, Refresh mode: {depthCache.RefreshMode}");
        }

        // Public method to manually refresh the cache (useful for runtime changes)
        public void RefreshOceanDepthCache()
        {
            if (oceanDepthCache != null)
            {
                oceanDepthCache.PopulateCache(updateComponents: true);
                Debug.Log("[OceanDepthCacheSetup] Manually refreshed OceanDepthCache");
            }
        }

        // Editor method to set up the cache in edit mode
        [ContextMenu("Setup Ocean Depth Cache")]
        public void EditorSetupOceanDepthCache()
        {
            if (islandGenerator == null)
            {
                islandGenerator = GetComponent<ProceduralIslandGenerator>();
            }
            
            if (islandGenerator != null)
            {
                SetupOceanDepthCache();
            }
        }
    }
} 