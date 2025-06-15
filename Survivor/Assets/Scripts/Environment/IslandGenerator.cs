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
        private WaterSystem waterSystem;

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
            GameObject waterObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterObject.name = "Water";
            waterObject.transform.position = new Vector3(0, waterLevel, 0);
            waterObject.transform.localScale = new Vector3(terrainSize / 10f, 1, terrainSize / 10f);

            if (waterMaterial != null)
            {
                waterObject.GetComponent<MeshRenderer>().material = waterMaterial;
            }

            // Add water system
            waterSystem = waterObject.AddComponent<WaterSystem>();
            waterSystem.waterHeight = waterLevel;
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