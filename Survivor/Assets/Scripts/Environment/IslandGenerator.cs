using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Survivor.Environment
{
    public class IslandGenerator : MonoBehaviour
    {
        [Header("Island Settings")]
        public float radius = 100f;
        public float height = 20f;
        public int resolution = 256;
        public float noiseScale = 0.1f;
        public int seed = 0;

        [Header("Terrain Settings")]
        public int terrainSize = 1000;
        public int heightmapResolution = 513;
        public float maxHeight = 50f;
        public float baseHeight = 20f;
        public float persistence = 0.5f;
        public float lacunarity = 2f;
        public int octaves = 4;

        [Header("Water Settings")]
        public float waterLevel = 15f;
        public Material waterMaterial;

        [Header("Vegetation")]
        public GameObject palmTreePrefab;
        public GameObject bushPrefab;
        public GameObject coconutPrefab;
        public int palmTreeCount = 50;
        public int bushCount = 100;
        public int coconutCount = 30;

        [Header("Camp")]
        public GameObject tentPrefab;
        public GameObject campfirePrefab;
        public Vector3 campPosition = new Vector3(0, 0, 0);

        private Terrain terrain;
        private TerrainData terrainData;
        private WaterPhysicsSystem waterSystem;

        private void Start()
        {
            GenerateIsland();
        }

        public void GenerateIsland()
        {
            CreateTerrain();
            CreateWater();
            PlaceVegetation();
            SetupCamp();
            
            // Ensure player has swimming capabilities
            SetupPlayerSwimming();
        }

        private void CreateTerrain()
        {
            // Create terrain data
            terrainData = new TerrainData();
            terrainData.heightmapResolution = heightmapResolution;
            terrainData.size = new Vector3(terrainSize, maxHeight, terrainSize);

            // Generate heightmap
            float[,] heights = new float[heightmapResolution, heightmapResolution];
            float[,] noise = GenerateNoiseMap();

            // Create island shape with falloff
            for (int y = 0; y < heightmapResolution; y++)
            {
                for (int x = 0; x < heightmapResolution; x++)
                {
                    float centerX = (float)x / heightmapResolution - 0.5f;
                    float centerY = (float)y / heightmapResolution - 0.5f;
                    float distance = Mathf.Sqrt(centerX * centerX + centerY * centerY) * 2f;
                    float falloff = Mathf.Clamp01(1f - distance);

                    heights[y, x] = Mathf.Clamp01((noise[x, y] + baseHeight / maxHeight) * falloff);
                }
            }

            terrainData.SetHeights(0, 0, heights);

            // Create terrain object
            GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
            terrain = terrainObject.GetComponent<Terrain>();
            terrain.transform.position = new Vector3(-terrainSize / 2f, 0, -terrainSize / 2f);
        }

        private float[,] GenerateNoiseMap()
        {
            float[,] noiseMap = new float[heightmapResolution, heightmapResolution];
            System.Random prng = new System.Random(seed);

            Vector2[] octaveOffsets = new Vector2[octaves];
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000);
                float offsetY = prng.Next(-100000, 100000);
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            for (int y = 0; y < heightmapResolution; y++)
            {
                for (int x = 0; x < heightmapResolution; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = x / noiseScale * frequency + octaveOffsets[i].x;
                        float sampleY = y / noiseScale * frequency + octaveOffsets[i].y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    noiseMap[x, y] = noiseHeight;
                }
            }

            return noiseMap;
        }

        private void CreateWater()
        {
            // Calculate the lowest terrain point to determine water depth
            float lowestTerrainPoint = CalculateLowestTerrainPoint();
            float waterDepth = waterLevel - lowestTerrainPoint;
            
            Debug.Log($"[IslandGenerator] Water Level: {waterLevel}, Lowest Terrain: {lowestTerrainPoint}, Water Depth: {waterDepth}");

            // Create water surface (visual)
            GameObject waterSurface = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterSurface.name = "WaterSurface";
            waterSurface.transform.position = new Vector3(0, waterLevel, 0);
            waterSurface.transform.localScale = new Vector3(terrainSize / 10f, 1, terrainSize / 10f);

            if (waterMaterial != null)
            {
                waterSurface.GetComponent<MeshRenderer>().material = waterMaterial;
            }

            // Remove the collider from the surface (we'll use a separate volume collider)
            DestroyImmediate(waterSurface.GetComponent<Collider>());

            // Create water volume (physics) - extends from water surface down to lowest terrain point
            GameObject waterVolume = new GameObject("WaterVolume");
            float volumeCenterY = waterLevel - (waterDepth / 2f); // Center the volume between water surface and lowest point
            waterVolume.transform.position = new Vector3(0, volumeCenterY, 0);
            
            // Add a large box collider for water physics
            BoxCollider waterCollider = waterVolume.AddComponent<BoxCollider>();
            waterCollider.isTrigger = true;
            waterCollider.size = new Vector3(terrainSize, waterDepth, terrainSize); // Full depth coverage
            waterCollider.center = new Vector3(0, waterDepth / 2f, 0); // Center at water surface

            // Add water system
            waterSystem = waterVolume.AddComponent<WaterPhysicsSystem>();
            waterSystem.waterHeight = waterLevel;
            waterSystem.buoyancyForce = 3f;
            waterSystem.dragForce = 1f;
            waterSystem.swimThreshold = 0.67f;
            waterSystem.swimSpeed = 4f;
            waterSystem.swimGravity = -2f;
            waterSystem.normalGravity = -15f;
            
            // Enable underwater effects
            waterSystem.enableUnderwaterEffects = true;
            waterSystem.useAdvancedEffects = false; // Use simple effects for Built-in RP
            waterSystem.underwaterTint = new Color(0.2f, 0.4f, 0.8f, 0.3f);
            waterSystem.underwaterBlur = 0.5f;
            waterSystem.underwaterDistortion = 0.1f;
            waterSystem.effectTransitionSpeed = 2f;
            waterSystem.effectIntensity = 1f;

            Debug.Log($"[IslandGenerator] Created water volume at center Y: {volumeCenterY} with size {waterCollider.size}");
        }

        private float CalculateLowestTerrainPoint()
        {
            if (terrain == null || terrainData == null) return 0f;

            float lowestPoint = float.MaxValue;
            int resolution = terrainData.heightmapResolution;
            
            // Sample the heightmap to find the lowest point
            for (int x = 0; x < resolution; x += 10) // Sample every 10th point for performance
            {
                for (int y = 0; y < resolution; y += 10)
                {
                    float height = terrainData.GetHeight(x, y);
                    if (height < lowestPoint)
                    {
                        lowestPoint = height;
                    }
                }
            }

            // Add some buffer below the lowest point to ensure full coverage
            lowestPoint -= 5f;
            
            return lowestPoint;
        }

        private void PlaceVegetation()
        {
            if (terrain == null) return;

            GameObject vegetationParent = new GameObject("Vegetation");
            vegetationParent.transform.parent = transform;

            // Place palm trees
            for (int i = 0; i < palmTreeCount; i++)
            {
                PlaceObjectOnTerrain(palmTreePrefab, vegetationParent.transform, 2f);
            }

            // Place bushes
            for (int i = 0; i < bushCount; i++)
            {
                PlaceObjectOnTerrain(bushPrefab, vegetationParent.transform, 1f);
            }

            // Place coconuts
            for (int i = 0; i < coconutCount; i++)
            {
                PlaceObjectOnTerrain(coconutPrefab, vegetationParent.transform, 0.5f);
            }
        }

        private void PlaceObjectOnTerrain(GameObject prefab, Transform parent, float minHeight)
        {
            if (prefab == null) return;

            for (int attempts = 0; attempts < 100; attempts++)
            {
                float x = Random.Range(-terrainSize / 2f, terrainSize / 2f);
                float z = Random.Range(-terrainSize / 2f, terrainSize / 2f);
                float height = terrain.SampleHeight(new Vector3(x, 0, z));

                if (height > waterLevel + minHeight)
                {
                    Vector3 position = new Vector3(x, height, z);
                    GameObject obj = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                    obj.transform.parent = parent;
                    break;
                }
            }
        }

        private void SetupCamp()
        {
            GameObject campParent = new GameObject("Camp");
            campParent.transform.parent = transform;

            // Find suitable camp location (flat area near water)
            Vector3 finalCampPos = FindCampLocation();

            // Place tent
            if (tentPrefab != null)
            {
                GameObject tent = Instantiate(tentPrefab, finalCampPos, Quaternion.identity);
                tent.transform.parent = campParent.transform;
            }

            // Place campfire
            if (campfirePrefab != null)
            {
                Vector3 firePos = finalCampPos + new Vector3(3f, 0f, 3f);
                firePos.y = terrain.SampleHeight(firePos);
                GameObject campfire = Instantiate(campfirePrefab, firePos, Quaternion.identity);
                campfire.transform.parent = campParent.transform;
            }
        }

        private Vector3 FindCampLocation()
        {
            float bestFlatness = float.MaxValue;
            Vector3 bestPosition = campPosition;

            // Sample various positions to find the flattest area near water
            for (int i = 0; i < 100; i++)
            {
                Vector3 testPos = new Vector3(
                    Random.Range(-terrainSize / 3f, terrainSize / 3f),
                    0,
                    Random.Range(-terrainSize / 3f, terrainSize / 3f)
                );

                testPos.y = terrain.SampleHeight(testPos);
                float flatness = CalculateAreaFlatness(testPos, 5f);

                if (flatness < bestFlatness && testPos.y > waterLevel + 1f && testPos.y < waterLevel + 5f)
                {
                    bestFlatness = flatness;
                    bestPosition = testPos;
                }
            }

            return bestPosition;
        }

        private float CalculateAreaFlatness(Vector3 center, float radius)
        {
            float variance = 0f;
            float centerHeight = terrain.SampleHeight(center);
            int samples = 8;

            for (int i = 0; i < samples; i++)
            {
                float angle = i * Mathf.PI * 2f / samples;
                Vector3 samplePos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                float height = terrain.SampleHeight(samplePos);
                variance += Mathf.Abs(height - centerHeight);
            }

            return variance / samples;
        }

        private void SetupPlayerSwimming()
        {
            // Find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            
            if (player != null)
            {
                // Try to add PlayerSwimmingSetup using reflection to avoid circular dependency
                System.Type swimmingSetupType = System.Type.GetType("Survivor.Characters.PlayerSwimmingSetup, Survivor.Characters");
                if (swimmingSetupType != null)
                {
                    Component existingSetup = player.GetComponent(swimmingSetupType);
                    if (existingSetup == null)
                    {
                        existingSetup = player.AddComponent(swimmingSetupType);
                        Debug.Log("[IslandGenerator] Added PlayerSwimmingSetup to player via reflection");
                    }
                }
                else
                {
                    Debug.LogWarning("[IslandGenerator] Could not find PlayerSwimmingSetup type. Make sure the Characters assembly is available.");
                }
                
                // Try to add CharacterSwimming using reflection
                System.Type swimmingType = System.Type.GetType("Survivor.Characters.CharacterSwimming, Survivor.Characters");
                if (swimmingType != null)
                {
                    Component existingSwimming = player.GetComponent(swimmingType);
                    if (existingSwimming == null)
                    {
                        existingSwimming = player.AddComponent(swimmingType);
                        Debug.Log("[IslandGenerator] Added CharacterSwimming to player via reflection");
                    }
                }
                else
                {
                    Debug.LogWarning("[IslandGenerator] Could not find CharacterSwimming type. Make sure the Characters assembly is available.");
                }
            }
            else
            {
                Debug.LogWarning("[IslandGenerator] No player found! Make sure your player is tagged as 'Player' or named 'Player'.");
            }
        }

        [ContextMenu("Test Water System")]
        public void TestWaterSystem()
        {
            if (waterSystem == null)
            {
                Debug.LogWarning("[IslandGenerator] No water system found! Generate the island first.");
                return;
            }

            Debug.Log($"[IslandGenerator] Water System Test:");
            Debug.Log($"- Water Height: {waterSystem.waterHeight}");
            Debug.Log($"- Swim Threshold: {waterSystem.swimThreshold}");
            Debug.Log($"- Underwater Effects: {(waterSystem.enableUnderwaterEffects ? "Enabled" : "Disabled")}");
            
            // Test player position
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            
            if (player != null)
            {
                float playerY = player.transform.position.y;
                bool isUnderwater = playerY < waterSystem.waterHeight;
                Debug.Log($"- Player Y: {playerY}, Water Level: {waterSystem.waterHeight}");
                Debug.Log($"- Player Underwater: {(isUnderwater ? "Yes" : "No")}");
                
                // Check swimming components using reflection
                System.Type swimmingType = System.Type.GetType("Survivor.Characters.CharacterSwimming, Survivor.Characters");
                if (swimmingType != null)
                {
                    Component swimming = player.GetComponent(swimmingType);
                    Debug.Log($"- CharacterSwimming Component: {(swimming != null ? "Present" : "Missing")}");
                    
                    if (swimming != null)
                    {
                        // Try to get IsSwimming property via reflection
                        var isSwimmingProperty = swimmingType.GetProperty("IsSwimming");
                        if (isSwimmingProperty != null)
                        {
                            bool isSwimming = (bool)isSwimmingProperty.GetValue(swimming);
                            Debug.Log($"- Swimming Active: {(isSwimming ? "Yes" : "No")}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("- CharacterSwimming type not found!");
                }
            }
            else
            {
                Debug.LogWarning("- No player found!");
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(IslandGenerator))]
        public class IslandGeneratorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                IslandGenerator generator = (IslandGenerator)target;
                if (GUILayout.Button("Generate Island"))
                {
                    generator.GenerateIsland();
                }
            }
        }
#endif
    }
} 