using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Survivor.Environment;
using Survivor.Shared;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Survivor.Generation
{
    // This script handles the procedural generation of the island and its features
    [DefaultExecutionOrder(-100)] // This makes the script run before default time
    [RequireComponent(typeof(Terrain))]
    [RequireComponent(typeof(TerrainCollider))]
    public class ProceduralIslandGenerator : MonoBehaviour
    {
        [System.Serializable]
        public class VegetationSettings
        {
            public GameObject[] treePrefabs = new GameObject[0];
            public GameObject[] bushPrefabs = new GameObject[0];
            public int treeCount = 300;
            public int bushCount = 200;
            public float minTreeHeight = 1f;
            public float maxDistanceFromCamp = 15f;
        }

        [System.Serializable]
        public class RiverSettings
        {
            public Material riverMaterial;
            public float riverWidth = 5f;
            public float riverDepth = 2f;
            public int riverSegments = 10;
            public float riverCurve = 0.3f;
        }

        [Header("Generation Settings")]
        public VegetationSettings vegetation;
        public RiverSettings river;

        [Header("Terrain Settings")]
        public int terrainSize = 400;
        public float maxHeight = 50f;
        public float noiseScale = 50f;
        public int octaves = 4;
        public float persistence = 0.5f;
        public float lacunarity = 2f;
        public Material terrainMaterial;
        public float defaultIslandHeight = 0.3f;

        [Header("Water Settings")]
        public Material waterMaterial;
        public float waterHeight = 0.1f; // 10% of max height

        private Terrain terrain;
        private TerrainData terrainData;
        private bool isInitialized = false;
        private bool isGenerationComplete = false;
        private GameObject waterPlane;
        private Vector3? campPosition = null; // Store camp position
        private const float CAMP_EXCLUSION_RADIUS = 10f; // Large exclusion radius around camp

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            if (!isInitialized)
            {
                InitializeComponents();
            }
            GenerateIsland();
            StartCoroutine(GenerateIslandFeatures());
        }

        private void InitializeComponents()
        {
            // Get or create terrain component
            terrain = GetComponent<Terrain>();
            if (terrain == null)
            {
                terrain = gameObject.AddComponent<Terrain>();
            }

            // Initialize terrain data
            terrainData = new TerrainData();
            terrainData.heightmapResolution = 513; // Must be power of 2 plus 1
            terrainData.size = new Vector3(terrainSize, maxHeight, terrainSize);
            terrain.terrainData = terrainData;

            // Position the terrain so its center is at world origin
            transform.position = new Vector3(-terrainSize * 0.5f, 0, -terrainSize * 0.5f);

            // Set terrain material
            if (terrainMaterial != null)
            {
                terrain.materialTemplate = terrainMaterial;
            }
            else
            {
                // Create a default material if none is assigned
                Material defaultMat = new Material(Shader.Find("Nature/Terrain/Standard"));
                defaultMat.color = new Color(0.7f, 0.7f, 0.5f); // Sandy color
                terrain.materialTemplate = defaultMat;
            }

            // Create water plane
            CreateWaterPlane();

            // Add terrain collider if missing
            TerrainCollider terrainCollider = GetComponent<TerrainCollider>();
            if (terrainCollider == null)
            {
                terrainCollider = gameObject.AddComponent<TerrainCollider>();
            }
            terrainCollider.terrainData = terrainData;
            
            // Initialize settings if null
            if (vegetation == null) vegetation = new VegetationSettings();
            if (river == null) river = new RiverSettings();

            isInitialized = true;
        }

        private void CreateWaterPlane()
        {
            // Remove existing water plane if it exists
            if (waterPlane != null)
            {
                DestroyImmediate(waterPlane);
            }

            // Create water plane
            waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterPlane.name = "Water";
            waterPlane.transform.parent = transform;

            // Scale the plane to match terrain size
            float planeScale = terrainSize / 10f; // Default plane is 10x10 units
            waterPlane.transform.localScale = new Vector3(planeScale, 1, planeScale);

            // Position the plane at water height
            waterPlane.transform.position = new Vector3(0, maxHeight * waterHeight, 0);

            // Apply water material
            MeshRenderer waterRenderer = waterPlane.GetComponent<MeshRenderer>();
            if (waterMaterial != null)
            {
                waterRenderer.material = waterMaterial;
            }
            else
            {
                // Create a default water material if none is assigned
                Material defaultWaterMat = new Material(Shader.Find("Standard"));
                defaultWaterMat.color = new Color(0.2f, 0.5f, 0.8f, 0.6f);
                defaultWaterMat.SetFloat("_Glossiness", 0.9f);
                defaultWaterMat.SetFloat("_Metallic", 0.0f);
                defaultWaterMat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                defaultWaterMat.renderQueue = 3000;
                waterRenderer.material = defaultWaterMat;
            }

            // Remove the default collider and add our own
            DestroyImmediate(waterPlane.GetComponent<Collider>());
            BoxCollider waterCollider = waterPlane.AddComponent<BoxCollider>();
            waterCollider.isTrigger = true; // Make it a trigger so objects can enter it
            waterCollider.size = new Vector3(1, 0.1f, 1); // Thin collider for water surface

            // Add water behavior script
            WaterBehavior waterBehavior = waterPlane.AddComponent<WaterBehavior>();
            waterBehavior.waterHeight = maxHeight * waterHeight;
        }

        public void GenerateIsland()
        {
            if (!isInitialized)
            {
                InitializeComponents();
            }

            // Generate heightmap
            int resolution = terrainData.heightmapResolution;
            float[,] heights = new float[resolution, resolution];

            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
            float radius = resolution * 0.4f; // Island takes up 80% of the terrain
            float flatHeight = defaultIslandHeight; // Constant height for the flat part of the island

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // Calculate distance from center (normalized)
                    float distanceFromCenter = Vector2.Distance(new Vector2(x, y), center) / radius;
                    
                    if (distanceFromCenter <= 0.75f) // Inner 80% of the radius is completely flat
                    {
                        heights[y, x] = flatHeight;
                    }
                    else // Create a smooth falloff at the edges
                    {
                        float falloff = 1 - ((distanceFromCenter - 0.75f) / 0.2f); // Smooth transition in the outer 20%
                        falloff = Mathf.Clamp01(falloff);
                        falloff = Mathf.Pow(falloff, 2); // Squared for smoother falloff
                        heights[y, x] = flatHeight * falloff;
                    }
                }
            }

            // Apply to terrain
            terrainData.SetHeights(0, 0, heights);

            // Set terrain layers for texturing
            TerrainLayer[] terrainLayers = new TerrainLayer[1];
            terrainLayers[0] = new TerrainLayer();
            terrainLayers[0].diffuseTexture = Texture2D.whiteTexture; // Default texture
            terrainLayers[0].tileSize = new Vector2(50, 50);
            terrainData.terrainLayers = terrainLayers;

            // Update terrain settings
            terrain.Flush();
            
            // Mark generation as complete
            isGenerationComplete = true;

            // Notify any waiting components
            SendMessage("OnTerrainGenerated", SendMessageOptions.DontRequireReceiver);
        }

        public bool IsGenerationComplete()
        {
            return isGenerationComplete;
        }

        private System.Collections.IEnumerator PlaceCampWithTimeout(CampGenerator campGenerator)
        {
            if (campGenerator == null)
            {
                Debug.LogError("CampGenerator is null");
                yield break;
            }

            float timeout = 5f; // 5 seconds timeout
            float elapsed = 0f;
            
            bool startedSuccessfully = false;
            try
            {
                campGenerator.SendMessage("PlaceCamp");
                startedSuccessfully = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error starting camp placement: {e.Message}");
                yield break;
            }
            
            if (startedSuccessfully)
            {
                // Wait for camp to be placed or timeout
                while (!campGenerator.CampPlaced && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                if (!campGenerator.CampPlaced)
                {
                    Debug.LogError($"Camp placement timed out after {timeout} seconds");
                }
            }
        }

        private void PlaceCampDelayed()
        {
            // Implementation of PlaceCampDelayed method
        }

        private System.Collections.IEnumerator GenerateIslandFeatures()
        {
            yield return new WaitForEndOfFrame();
            
            CampGenerator campGenerator = null;
            bool generatorFound = false;
            bool shouldContinue = true;
            
            try
            {
                campGenerator = GetComponent<CampGenerator>();
                generatorFound = (campGenerator != null);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting CampGenerator: {e.Message}\n{e.StackTrace}");
                shouldContinue = false;
            }

            if (!shouldContinue)
            {
                yield break;
            }

            if (generatorFound)
            {
                Debug.Log("Starting camp placement...");
                yield return StartCoroutine(PlaceCampWithTimeout(campGenerator));
                
                if (campGenerator.CampPlaced)
                {
                    // Store camp position after successful placement
                    campPosition = campGenerator.TentPosition;
                    Debug.Log($"Camp placed at {campPosition}, will maintain {CAMP_EXCLUSION_RADIUS} unit exclusion zone");
                    
                    try
                    {
                        Debug.Log("Camp placed successfully, generating river and vegetation");
                        CreateRiver();
                        PlaceVegetation();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error generating river and vegetation: {e.Message}\n{e.StackTrace}");
                    }
                }
                else
                {
                    Debug.LogError("Failed to place camp after timeout, skipping river and vegetation generation");
                }
            }
            else
            {
                Debug.LogWarning("No CampGenerator found - proceeding with river and vegetation");
                try
                {
                    CreateRiver();
                    PlaceVegetation();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error generating river and vegetation: {e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void CreateRiver()
        {
            if (terrain == null || terrainData == null)
            {
                Debug.LogError("Terrain or TerrainData is null");
                return;
            }

            // Create a new GameObject for the river
            GameObject riverObject = new GameObject("River");
            riverObject.transform.parent = transform;

            // Create a simple curved path for the river
            Vector3[] riverPath = GenerateRiverPath();

            // Create the river mesh
            MeshFilter meshFilter = riverObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = riverObject.AddComponent<MeshRenderer>();
            
            // Create the river mesh based on the path
            Mesh riverMesh = CreateRiverMesh(riverPath);
            meshFilter.mesh = riverMesh;

            // Apply the river material
            if (river.riverMaterial != null)
            {
                meshRenderer.material = river.riverMaterial;
            }
            else
            {
                Debug.LogWarning("River material is not assigned");
            }
        }

        private Vector3[] GenerateRiverPath()
        {
            Vector3[] path = new Vector3[river.riverSegments];
            float terrainSize = terrainData.size.x;
            float segmentLength = terrainSize / (river.riverSegments - 1);

            // Start from a random edge point
            float startX = Random.Range(0, terrainSize);
            float startZ = Random.Range(0, terrainSize);
            
            // Decide if river goes roughly north-south or east-west
            bool isNorthSouth = Random.value > 0.5f;
            
            for (int i = 0; i < river.riverSegments; i++)
            {
                float progress = i / (float)(river.riverSegments - 1);
                float xOffset = isNorthSouth ? 
                    Mathf.Sin(progress * Mathf.PI * 2f) * river.riverCurve * terrainSize : 
                    progress * terrainSize;
                float zOffset = isNorthSouth ? 
                    progress * terrainSize : 
                    Mathf.Sin(progress * Mathf.PI * 2f) * river.riverCurve * terrainSize;

                float x = isNorthSouth ? startX + xOffset : xOffset;
                float z = isNorthSouth ? zOffset : startZ + zOffset;
                float y = terrain.SampleHeight(new Vector3(x, 0, z)) - river.riverDepth;

                path[i] = new Vector3(x, y, z);
            }

            return path;
        }

        private Mesh CreateRiverMesh(Vector3[] path)
        {
            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            // Create vertices along the path
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 forward = i < path.Length - 1 ? 
                    (path[i + 1] - path[i]).normalized : 
                    (path[i] - path[i - 1]).normalized;
                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

                // Left and right vertices
                vertices.Add(path[i] + right * river.riverWidth * 0.5f);
                vertices.Add(path[i] - right * river.riverWidth * 0.5f);

                // UVs
                float uvY = i / (float)(path.Length - 1);
                uvs.Add(new Vector2(0, uvY));
                uvs.Add(new Vector2(1, uvY));

                // Create triangles
                if (i < path.Length - 1)
                {
                    int baseIndex = i * 2;
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 2);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 2);
                    triangles.Add(baseIndex + 3);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private void PlaceVegetation()
        {
            if (terrain == null || terrainData == null)
            {
                Debug.LogError("Terrain or TerrainData is null");
                return;
            }

            if (vegetation.treePrefabs == null || vegetation.treePrefabs.Length == 0)
            {
                Debug.LogWarning("No tree prefabs assigned");
                return;
            }

            // Create a parent object for vegetation
            GameObject vegetationParent = new GameObject("Vegetation");
            vegetationParent.transform.parent = transform;

            // Place trees
            for (int i = 0; i < vegetation.treeCount; i++)
            {
                PlaceRandomVegetation(vegetation.treePrefabs[Random.Range(0, vegetation.treePrefabs.Length)], vegetationParent.transform);
            }

            // Place bushes if we have bush prefabs
            if (vegetation.bushPrefabs != null && vegetation.bushPrefabs.Length > 0)
            {
                for (int i = 0; i < vegetation.bushCount; i++)
                {
                    PlaceRandomVegetation(vegetation.bushPrefabs[Random.Range(0, vegetation.bushPrefabs.Length)], vegetationParent.transform);
                }
            }
        }

        private void PlaceRandomVegetation(GameObject prefab, Transform parent)
        {
            if (prefab == null) return;

            float terrainWidth = terrainData.size.x;
            float terrainLength = terrainData.size.z;
            Vector3 islandCenter = GetIslandCenter();
            float islandRadius = IslandRadius * 0.75f; // Use 75% of island radius for vegetation

            for (int attempts = 0; attempts < 30; attempts++) // Increased attempts to find valid position
            {
                // Get random position within island bounds using polar coordinates
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(0f, islandRadius);
                
                // Convert polar to cartesian coordinates
                float x = islandCenter.x + Mathf.Cos(angle) * distance;
                float z = islandCenter.z + Mathf.Sin(angle) * distance;
                
                // Sample height at this position
                float y = terrain.SampleHeight(new Vector3(x, 0, z));
                Vector3 position = new Vector3(x, y, z);

                // Check if position is suitable
                if (IsValidVegetationPosition(position))
                {
                    // Create vegetation object
                    GameObject vegetation = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
                    vegetation.transform.parent = parent;

                    // Add slight random scale variation
                    float scale = Random.Range(0.8f, 1.2f);
                    vegetation.transform.localScale *= scale;

                    // Add appropriate physics components based on whether it's a tree or bush
                    bool isTree = prefab.name.ToLower().Contains("tree");
                    if (isTree)
                    {
                        Debug.Log($"Setting up physics for tree: {vegetation.name}");
                        
                        // Remove any existing colliders
                        Collider[] existingColliders = vegetation.GetComponents<Collider>();
                        foreach (Collider collider in existingColliders)
                        {
                            DestroyImmediate(collider);
                        }

                        // Add rigidbody
                        Rigidbody rb = vegetation.AddComponent<Rigidbody>();
                        rb.isKinematic = true;
                        rb.useGravity = false;

                        // Add MeshCollider to all children that have meshes
                        MeshFilter[] meshFilters = vegetation.GetComponentsInChildren<MeshFilter>();
                        foreach (MeshFilter meshFilter in meshFilters)
                        {
                            if (meshFilter.sharedMesh != null)
                            {
                                Debug.Log($"Adding MeshCollider to {meshFilter.gameObject.name} with mesh: {meshFilter.sharedMesh.name}");
                                MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
                                meshCollider.sharedMesh = meshFilter.sharedMesh;
                                meshCollider.convex = false;
                                meshCollider.isTrigger = false;
                            }
                        }

                        // If no mesh colliders were added, add a fallback capsule collider
                        if (vegetation.GetComponents<MeshCollider>().Length == 0)
                        {
                            Debug.LogWarning($"No meshes found for {vegetation.name}, adding fallback CapsuleCollider");
                            CapsuleCollider capsuleCollider = vegetation.AddComponent<CapsuleCollider>();
                            capsuleCollider.height = vegetation.transform.localScale.y * 2f;
                            capsuleCollider.radius = vegetation.transform.localScale.x * 0.5f;
                            capsuleCollider.center = new Vector3(0, vegetation.transform.localScale.y, 0);
                        }
                    }
                    else
                    {
                        // For bushes, add a trigger collider so they can be detected but walked through
                        SphereCollider bushCollider = vegetation.AddComponent<SphereCollider>();
                        bushCollider.isTrigger = true;
                        bushCollider.radius = vegetation.transform.localScale.x * 0.5f;
                    }
                    break;
                }
            }
        }

        private bool IsValidVegetationPosition(Vector3 position)
        {
            // Check height (avoid water level)
            if (position.y < defaultIslandHeight) return false;

            // Check distance from camp if camp exists
            if (campPosition.HasValue)
            {
                float distanceToCamp = Vector3.Distance(
                    new Vector3(position.x, 0, position.z),
                    new Vector3(campPosition.Value.x, 0, campPosition.Value.z)
                );
                
                if (distanceToCamp < CAMP_EXCLUSION_RADIUS)
                {
                    return false;
                }
            }

            return true;
        }

        // Required public methods for CampGenerator
        public Vector3 GetIslandCenter()
        {
            if (terrain == null) return Vector3.zero;
            // Return the center of the terrain in world space
            return terrain.transform.position + new Vector3(terrainSize * 0.5f, 0, terrainSize * 0.5f);
        }

        public float GetIslandSize()
        {
            return terrainSize;
        }

        public float IslandRadius
        {
            get { return terrainSize * 0.4f; } // Match the radius used in generation
        }

        public Vector3? GetCampPosition()
        {
            return campPosition;
        }
    }
} 