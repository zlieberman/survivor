using UnityEngine;
using Survivor.Shared;
using Survivor.Items;
using Survivor.Interactables;
using System.Collections.Generic;

namespace Survivor.Generation
{
    /// <summary>
    /// Spawns coconuts and firewood every hour using the same spawn rules as vegetation
    /// Implements IGameHourListener to receive hourly events from TimeManager via InteractableManager
    /// </summary>
    public class ResourceSpawner : MonoBehaviour, IGameHourListener
    {
        [Header("Spawn Settings")]
        [SerializeField] private int coconutsPerHour = 3;
        [SerializeField] private int firewoodPerHour = 2;
        [SerializeField] private float coconutOffsetFromTree = 1.5f;
        [SerializeField] private float firewoodOffsetFromTree = 2.0f;
        [SerializeField] private float maxDistanceFromCamp = 15f;
        [SerializeField] private float minTreeHeight = 1f;
        [SerializeField] private float minDistanceBetweenResources = 2f;

        [Header("References")]
        [SerializeField] private GameObject coconutPrefab;
        [SerializeField] private GameObject firewoodPrefab;
        [SerializeField] private GameObject[] treePrefabs;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Public property to get total spawn count
        public int TotalSpawnCount => coconutsPerHour + firewoodPerHour;

        private Terrain terrain;
        private TerrainData terrainData;
        private Transform vegetationParent;
        private List<GameObject> spawnedResources = new List<GameObject>();
        private Vector3 islandCenter;
        private float islandRadius;

        private void Start()
        {
            // Find terrain
            terrain = FindObjectOfType<Terrain>();
            if (terrain != null)
            {
                terrainData = terrain.terrainData;
            }

            // Find or create vegetation parent
            GameObject vegetationParentObj = GameObject.Find("Vegetation");
            if (vegetationParentObj == null)
            {
                vegetationParentObj = new GameObject("Vegetation");
            }
            vegetationParent = vegetationParentObj.transform;

            // Get island center and radius from ProceduralIslandGenerator if available
            var islandGenerator = FindObjectOfType<ProceduralIslandGenerator>();
            if (islandGenerator != null)
            {
                islandCenter = islandGenerator.GetIslandCenter();
                islandRadius = islandGenerator.IslandRadius * 0.75f; // Use 75% of island radius like vegetation
            }
            else
            {
                // Fallback values
                islandCenter = Vector3.zero;
                islandRadius = 100f;
            }

            // Try to get prefabs from ProceduralIslandGenerator if not assigned
            if (coconutPrefab == null || firewoodPrefab == null || treePrefabs == null || treePrefabs.Length == 0)
            {
                GetPrefabsFromIslandGenerator();
            }

            // Register with InteractableManager to receive hourly events
            RegisterWithInteractableManager();
        }

        private void RegisterWithInteractableManager()
        {
            // Register with InteractableManager using the singleton instance
            if (InteractableManager.Instance != null)
            {
                InteractableManager.Instance.RegisterGameHourListener(this);
                if (enableDebugLogs)
                {
                    Debug.Log("[ResourceSpawner] Successfully registered with InteractableManager for hourly events");
                    Debug.Log($"[ResourceSpawner] Total registered listeners: {InteractableManager.Instance.GetListenerCount()}");
                }
            }
            else
            {
                Debug.LogError("[ResourceSpawner] InteractableManager.Instance is null! Resource spawning will not work.");
                Debug.LogError("[ResourceSpawner] Make sure there's an InteractableManager GameObject in the scene with the InteractableManager component.");
            }
        }

        private void UnregisterFromInteractableManager()
        {
            // Unregister from InteractableManager using the singleton instance
            if (InteractableManager.Instance != null)
            {
                InteractableManager.Instance.UnregisterGameHourListener(this);
                if (enableDebugLogs)
                {
                    Debug.Log("[ResourceSpawner] Successfully unregistered from InteractableManager");
                }
            }
        }

        private void GetPrefabsFromIslandGenerator()
        {
            var islandGenerator = FindObjectOfType<ProceduralIslandGenerator>();
            if (islandGenerator != null)
            {
                var vegetationSettings = islandGenerator.GetVegetationSettings();
                if (vegetationSettings != null)
                {
                    if (coconutPrefab == null)
                    {
                        coconutPrefab = vegetationSettings.coconutPrefab;
                    }
                    if (firewoodPrefab == null)
                    {
                        firewoodPrefab = vegetationSettings.firewoodPrefab;
                    }
                    if (treePrefabs == null || treePrefabs.Length == 0)
                    {
                        treePrefabs = vegetationSettings.treePrefabs;
                    }
                }
            }
        }

        /// <summary>
        /// IGameHourListener implementation - called by TimeManager via InteractableManager every game hour
        /// </summary>
        public void OnGameHourPassed()
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ResourceSpawner] ===== OnGameHourPassed CALLED - spawning {coconutsPerHour} coconuts and {firewoodPerHour} firewood =====");
            }

            // Spawn coconuts
            for (int i = 0; i < coconutsPerHour; i++)
            {
                SpawnCoconut();
            }

            // Spawn firewood
            for (int i = 0; i < firewoodPerHour; i++)
            {
                SpawnFirewood();
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[ResourceSpawner] ===== OnGameHourPassed COMPLETED - spawned {TotalSpawnCount} total resources =====");
            }
        }

        private void SpawnCoconut()
        {
            if (coconutPrefab == null)
            {
                Debug.LogWarning("[ResourceSpawner] Coconut prefab is null - cannot spawn coconut");
                return;
            }

            // Find a random tree to spawn near
            GameObject tree = FindRandomTree();
            if (tree == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("[ResourceSpawner] No trees found - cannot spawn coconut");
                }
                return;
            }

            // Place coconut near the tree
            Vector3 coconutPosition = GetPositionNearTree(tree.transform.position, coconutOffsetFromTree);
            
            if (IsValidSpawnPosition(coconutPosition))
            {
                GameObject coconut = Instantiate(coconutPrefab, coconutPosition, Quaternion.identity);
                coconut.transform.parent = vegetationParent;

                // Set the layer to Interactable
                coconut.layer = LayerMask.NameToLayer("Interactable");

                // Add a collider for interaction
                SphereCollider collider = coconut.AddComponent<SphereCollider>();
                collider.radius = 0.5f;
                collider.isTrigger = true;

                // Add the Coconut component if it's not already there
                if (coconut.GetComponent<Coconut>() == null)
                {
                    coconut.AddComponent<Coconut>();
                }

                spawnedResources.Add(coconut);

                if (enableDebugLogs)
                {
                    Debug.Log($"[ResourceSpawner] Spawned coconut at {coconutPosition}");
                }
            }
        }

        private void SpawnFirewood()
        {
            if (firewoodPrefab == null)
            {
                Debug.LogWarning("[ResourceSpawner] Firewood prefab is null - cannot spawn firewood");
                return;
            }

            // Find a random tree to spawn near
            GameObject tree = FindRandomTree();
            if (tree == null)
            {
                if (enableDebugLogs)
                {
                    Debug.LogWarning("[ResourceSpawner] No trees found - cannot spawn firewood");
                }
                return;
            }

            // Place firewood near the tree
            Vector3 firewoodPosition = GetPositionNearTree(tree.transform.position, firewoodOffsetFromTree);
            
            if (IsValidSpawnPosition(firewoodPosition))
            {
                GameObject firewood = Instantiate(firewoodPrefab, firewoodPosition, Quaternion.identity);
                firewood.transform.parent = vegetationParent;

                // Set the layer to Interactable
                firewood.layer = LayerMask.NameToLayer("Interactable");

                // Add a collider for interaction
                SphereCollider collider = firewood.AddComponent<SphereCollider>();
                collider.radius = 0.5f;
                collider.isTrigger = true;

                // Add the Firewood component if it's not already there
                if (firewood.GetComponent<Firewood>() == null)
                {
                    firewood.AddComponent<Firewood>();
                }

                spawnedResources.Add(firewood);

                if (enableDebugLogs)
                {
                    Debug.Log($"[ResourceSpawner] Spawned firewood at {firewoodPosition}");
                }
            }
        }

        private GameObject FindRandomTree()
        {
            // Look for trees in the vegetation parent
            if (vegetationParent != null)
            {
                // Get all child objects of the vegetation parent
                Transform[] vegetationChildren = vegetationParent.GetComponentsInChildren<Transform>();
                var treeList = new List<GameObject>();
                
                foreach (Transform child in vegetationChildren)
                {
                    // Skip the vegetation parent itself
                    if (child == vegetationParent) continue;
                    
                    // Look for objects with "tree" in the name (case insensitive)
                    if (child.name.ToLower().Contains("tree"))
                    {
                        treeList.Add(child.gameObject);
                    }
                }
                
                if (treeList.Count > 0)
                {
                    return treeList[Random.Range(0, treeList.Count)];
                }
            }
            
            // Fallback: look for any objects with "tree" in the name anywhere in the scene
            var allObjects = FindObjectsOfType<GameObject>();
            var fallbackTreeList = new List<GameObject>();
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("tree"))
                {
                    fallbackTreeList.Add(obj);
                }
            }
            
            if (fallbackTreeList.Count > 0)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[ResourceSpawner] Found {fallbackTreeList.Count} trees using fallback method");
                }
                return fallbackTreeList[Random.Range(0, fallbackTreeList.Count)];
            }

            if (enableDebugLogs)
            {
                Debug.LogWarning("[ResourceSpawner] No trees found in scene - cannot spawn resources");
            }
            return null;
        }

        private Vector3 GetPositionNearTree(Vector3 treePosition, float offset)
        {
            // Get a random angle around the tree
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            
            // Calculate position offset from tree
            Vector3 offsetVector = new Vector3(
                Mathf.Cos(angle) * offset,
                0,
                Mathf.Sin(angle) * offset
            );

            // Get the ground height at the position
            Vector3 position = treePosition + offsetVector;
            if (terrain != null)
            {
                float groundHeight = terrain.SampleHeight(position);
                position.y = groundHeight + 0.2f;
            }

            return position;
        }

        private bool IsValidSpawnPosition(Vector3 position)
        {
            // Check if position is within island bounds
            Vector2 position2D = new Vector2(position.x, position.z);
            Vector2 center2D = new Vector2(islandCenter.x, islandCenter.z);
            float distanceFromCenter = Vector2.Distance(position2D, center2D);
            
            if (distanceFromCenter > islandRadius)
            {
                return false;
            }

            // Check if position is not too close to camp (if camp exists)
            GameObject camp = FindCamp();
            if (camp != null)
            {
                float distanceFromCamp = Vector3.Distance(position, camp.transform.position);
                if (distanceFromCamp < maxDistanceFromCamp)
                {
                    return false;
                }
            }

            // Check if position is not too close to other spawned resources
            foreach (var resource in spawnedResources)
            {
                if (resource != null)
                {
                    float distance = Vector3.Distance(position, resource.transform.position);
                    if (distance < minDistanceBetweenResources) // Minimum distance between resources
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private GameObject FindCamp()
        {
            // Look for camp by name (case insensitive)
            var allObjects = FindObjectsOfType<GameObject>();
            
            foreach (var obj in allObjects)
            {
                if (obj.name.ToLower().Contains("camp") || 
                    obj.name.ToLower().Contains("tent") ||
                    obj.name.ToLower().Contains("campfire"))
                {
                    return obj;
                }
            }
            
            // Fallback: look for objects with "CampSpawnPoint" as a child
            foreach (var obj in allObjects)
            {
                Transform campSpawnPoint = obj.transform.Find("CampSpawnPoint");
                if (campSpawnPoint != null)
                {
                    return obj;
                }
            }
            
            return null;
        }

        private void OnDestroy()
        {
            // Unregister from InteractableManager
            UnregisterFromInteractableManager();
        }

        // Public method to manually trigger spawning (for testing)
        [ContextMenu("Spawn Resources Now")]
        public void SpawnResourcesNow()
        {
            Debug.Log("[ResourceSpawner] ===== MANUAL SPAWN TRIGGERED =====");
            OnGameHourPassed();
            Debug.Log("[ResourceSpawner] ===== MANUAL SPAWN COMPLETED =====");
        }

        // Public method to clear all spawned resources
        [ContextMenu("Clear All Resources")]
        public void ClearAllResources()
        {
            foreach (var resource in spawnedResources)
            {
                if (resource != null)
                {
                    DestroyImmediate(resource);
                }
            }
            spawnedResources.Clear();
            Debug.Log("[ResourceSpawner] Cleared all spawned resources");
        }
    }
} 