using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections;
using Survivor.Shared;
using Survivor.Interactables;

namespace Survivor.Generation
{
    public class CampGenerator : MonoBehaviour
    {
        [Header("Camp Prefabs")]
        public GameObject tentPrefab;
        public GameObject campfirePrefab;
        public GameObject litCampfirePrefab;  // The lit campfire prefab to spawn when campfire is interacted with
        public GameObject bannerPrefab;
        public GameObject waterWellPrefab;  // New water well prefab

        [Header("Prefab Scale Settings")]
        public Vector3 tentScale = Vector3.one;
        public Vector3 campfireScale = Vector3.one;
        public Vector3 bannerScale = Vector3.one;
        public Vector3 waterWellScale = Vector3.one;  // New water well scale

        [Header("Camp Settings")]
        public float distanceFromWater = 10f;
        public float campRadius = 10f;  // How spread out the camp items should be
        public string tribeName = "Premio Tribe";
        public float clearTreesRadius = 1f; // Radius to clear trees around camp
        public float minWellDistance = 10f;  // Minimum distance from camp center
        public float maxWellDistance = 50f;  // Maximum distance from camp center
        public float wellClearRadius = 5f;  // Radius to clear trees around well (increased from 3f)
        public float defaultIslandHeight = 0.2f;  // Minimum height for valid terrain

        [Header("Banner Settings")]
        public float bannerHeight = 3f;
        public Color bannerColor = new Color(0.8f, 0.2f, 0.2f); // Red color for the tribe

        [Header("Physics Settings")]
        public float raycastHeight = 1000f; // Increased height to ensure we hit the terrain
        public float raycastDistance = 2000f; // Increased distance for the raycast

        private ProceduralIslandGenerator islandGenerator;
        [SerializeField] private Vector3 tentPosition;
        private LayerMask terrainMask;
        private Terrain terrain;
        
        private NavMeshSurface navMeshSurface;
        
        public Vector3 TentPosition 
        { 
            get { return tentPosition; }
            private set { tentPosition = value; }
        }

        public bool CampPlaced { get; private set; } = false;
        public Transform CampSpawnPoint { get; private set; }

        private void Start()
        {
            islandGenerator = GetComponent<ProceduralIslandGenerator>();
            if (islandGenerator == null)
            {
                Debug.LogError("CampGenerator requires a ProceduralIslandGenerator component on the same GameObject!");
                return;
            }

            // Get the terrain component from the same GameObject
            terrain = GetComponent<Terrain>();
            if (terrain == null)
            {
                Debug.LogError("CampGenerator requires a Terrain component on the same GameObject!");
                return;
            }

            // Set up NavMeshSurface
            navMeshSurface = GetComponent<NavMeshSurface>();
            if (navMeshSurface == null)
            {
                navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
                navMeshSurface.collectObjects = CollectObjects.Volume;
                navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                navMeshSurface.layerMask = terrainMask;
                Debug.Log("Added NavMeshSurface component");
            }

            // Set up the terrain layer mask to include both the default terrain layer and the "Terrain" layer if it exists
            terrainMask = 1 << LayerMask.NameToLayer("Default");  // Default layer
            terrainMask |= 1 << 8;  // Built-in terrain layer (Terrain And Water)
            int terrainLayer = LayerMask.NameToLayer("Terrain");
            if (terrainLayer != -1)
            {
                terrainMask |= 1 << terrainLayer;
            }
            
            Debug.Log($"Terrain mask setup complete: {terrainMask}");
        }

        public void PlaceCamp()
        {
            if (terrain == null)
            {
                Debug.LogError("No Terrain component found!");
                return;
            }

            Debug.Log("PlaceCamp called, starting placement coroutine...");
            CampPlaced = false;
            StartCoroutine(PlaceCampDelayed());
        }

        private System.Collections.IEnumerator PlaceCampDelayed()
        {
            if (terrain == null)
            {
                Debug.LogError("No Terrain component found!");
                yield break;
            }

            Debug.Log("Starting PlaceCampDelayed coroutine...");
            
            // Wait for terrain to be fully initialized
            yield return new WaitForEndOfFrame();

            try
            {
                // Find a suitable location for the camp
                Vector3 campPosition = FindCampLocation();
                Debug.Log($"Found camp location at {campPosition}");
                
                // Clear trees in the area before placing camp
                ClearTreesInArea(campPosition, clearTreesRadius);

                // Get the exact terrain position using raycast
                RaycastHit hitInfo;
                Vector3 rayStart = new Vector3(campPosition.x, raycastHeight, campPosition.z);
                bool hit = Physics.Raycast(rayStart, Vector3.down, out hitInfo, raycastDistance, terrainMask);

                // Debug ray visualization
                Debug.DrawRay(rayStart, Vector3.down * raycastDistance, Color.red, 10f);
                Debug.Log($"Raycast - Start: {rayStart}, Direction: Down, Distance: {raycastDistance}, Hit: {hit}, Layer Mask: {terrainMask}");
                
                if (!hit)
                {
                    Debug.LogError($"Could not find valid terrain position for camp! Position: {campPosition}, Height: {raycastHeight}");
                    yield break;
                }

                Debug.Log($"Hit terrain at point: {hitInfo.point}, normal: {hitInfo.normal}, distance: {hitInfo.distance}");

                // Place tent
                tentPosition = hitInfo.point;
                GameObject tent = Instantiate(tentPrefab, tentPosition, Quaternion.identity);
                tent.transform.parent = transform;
                tent.transform.localScale = tentScale;
                AddCampObjectPhysics(tent);

                // Create and set up the camp spawn point
                GameObject spawnPointObj = new GameObject("CampSpawnPoint");
                spawnPointObj.transform.parent = transform;
                spawnPointObj.transform.position = tentPosition + new Vector3(-1f, 0, 5f); // Same offset as in SpawnPlayerAtCamp
                spawnPointObj.transform.position = new Vector3(spawnPointObj.transform.position.x, 
                    GetTerrainHeight(spawnPointObj.transform.position) + 1f, 
                    spawnPointObj.transform.position.z);
                CampSpawnPoint = spawnPointObj.transform;

                // Place campfire near tent
                Vector3 fireOffset = new Vector3(-1f, 0, 7f);
                Vector3 firePosition = new Vector3(tentPosition.x + fireOffset.x, 0, tentPosition.z + fireOffset.z);
                firePosition.y = terrain.SampleHeight(firePosition);
                GameObject fire = Instantiate(campfirePrefab, firePosition, Quaternion.identity);
                fire.transform.parent = transform;
                fire.transform.localScale = campfireScale;
                
                // Set the layer to Interactable
                fire.layer = LayerMask.NameToLayer("Interactable");
                
                // Add the physical collider for collision
                AddCampObjectPhysics(fire);
                
                // Add physical collider for campfire (since we skipped it in AddCampObjectPhysics)
                CapsuleCollider physicalCollider = fire.AddComponent<CapsuleCollider>();
                physicalCollider.radius = 0.5f;
                physicalCollider.height = 1f;
                physicalCollider.center = new Vector3(0, 0.5f, 0);
                
                // Add and configure the trigger collider for interaction
                SphereCollider triggerCollider = fire.AddComponent<SphereCollider>();
                triggerCollider.radius = 3f;  // Interaction radius
                triggerCollider.isTrigger = true;  // Make it a trigger collider
                
                // Add CampfireInteractable component
                CampfireInteractable campfireInteractable = fire.AddComponent<CampfireInteractable>();
                
                // Assign the lit campfire prefab if provided
                if (litCampfirePrefab != null)
                {
                    campfireInteractable.litCampfirePrefab = litCampfirePrefab;
                    Debug.Log("Assigned lit campfire prefab to campfire interactable");
                }
                else
                {
                    Debug.LogWarning("Lit campfire prefab not assigned! Campfire won't be able to light.");
                }
                
                Debug.Log($"Campfire placed at {firePosition}");

                // Place banner near tent
                Vector3 bannerOffset = new Vector3(-3f, 0, 7f);
                Vector3 bannerPosition = new Vector3(tentPosition.x + bannerOffset.x, 0, tentPosition.z + bannerOffset.z);
                bannerPosition.y = terrain.SampleHeight(bannerPosition);
                GameObject banner = Instantiate(bannerPrefab, bannerPosition, Quaternion.identity);
                banner.transform.parent = transform;
                banner.transform.localScale = bannerScale;
                AddCampObjectPhysics(banner);
                SetupBanner(banner);

                // Place water well
                if (waterWellPrefab != null)
                {
                    Vector3 wellPosition = FindWellLocation(tentPosition);
                    if (wellPosition != Vector3.zero)
                    {
                        GameObject well = Instantiate(waterWellPrefab, wellPosition, Quaternion.identity);
                        well.transform.parent = transform;
                        well.transform.localScale = waterWellScale;
                        
                        // Set the layer to Interactable
                        well.layer = LayerMask.NameToLayer("Interactable");
                        
                        // Add the physical collider for collision
                        AddCampObjectPhysics(well);
                        
                        // Add and configure the trigger collider for interaction
                        CapsuleCollider wellTriggerCollider = well.AddComponent<CapsuleCollider>();
                        wellTriggerCollider.radius = 0.35f;
                        wellTriggerCollider.height = 2f;
                        wellTriggerCollider.center = new Vector3(0, 1f, 0);
                        wellTriggerCollider.isTrigger = true;  // Make it a trigger collider
                        
                        // Add WaterWellInteractable component
                        well.AddComponent<WaterWellInteractable>();
                        
                        Debug.Log($"Water well placed at {wellPosition}");
                    }
                    else
                    {
                        Debug.LogWarning("Could not place water well - no suitable location found");
                    }
                }
                else
                {
                    Debug.LogWarning("Water well prefab not assigned");
                }

                Debug.Log($"Camp generated successfully at position: {tentPosition}");
                CampPlaced = true;

                // Bake NavMesh after camp is placed
                if (navMeshSurface != null)
                {
                    Debug.Log("Baking NavMesh...");
                    navMeshSurface.BuildNavMesh();
                    Debug.Log("NavMesh baking complete");
                }
                else
                {
                    Debug.LogError("NavMeshSurface component not found!");
                }

                // Position the player at the camp
                SpawnPlayerAtCamp();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error placing camp objects: {e.Message}\n{e.StackTrace}");
                CampPlaced = false;
            }
        }

        private void AddCampObjectPhysics(GameObject obj)
        {
            // Add rigidbody
            Rigidbody rb = obj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            // Add appropriate collider based on object type
            if (obj.name.ToLower().Contains("tent"))
            {
                // Add a box collider for the tent
                // BoxCollider collider = obj.AddComponent<BoxCollider>();
                // collider.size = new Vector3(2f, 2f, 3f); // Adjust size based on your tent model
                // collider.center = new Vector3(0, 1f, 0);
            }
            else if (obj.name.ToLower().Contains("fire"))
            {
                // Skip adding physical collider for campfire since we handle it in the main placement code
                // The trigger collider for interaction is added separately
            }
            else if (obj.name.ToLower().Contains("banner"))
            {
                // Add a box collider for the banner
                BoxCollider collider = obj.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.2f, 3f, 0.2f);
                collider.center = new Vector3(0, 1.5f, 0);
            }
            else if (obj.name.ToLower().Contains("well"))
            {
                // Add a cylinder collider for the water well
                CapsuleCollider collider = obj.AddComponent<CapsuleCollider>();
                collider.radius = 0.35f;
                collider.height = 2f;
                collider.center = new Vector3(0, 1f, 0);
            }
        }

        private void AlignObjectToTerrain(GameObject obj, Vector3 normal)
        {
            if (normal == Vector3.zero)
            {
                normal = Vector3.up;
            }

            // Create rotation to align with terrain normal
            Quaternion alignRotation = Quaternion.FromToRotation(Vector3.up, normal);
            
            // Add random rotation around the up axis
            Quaternion randomYRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            
            // Apply both rotations
            obj.transform.rotation = alignRotation * randomYRotation;
        }

        private void ClearTreesInArea(Vector3 center, float radius)
        {
            if (islandGenerator == null || terrain == null) return;

            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null) return;

            // Get all tree instances
            TreeInstance[] trees = terrainData.treeInstances;
            System.Collections.Generic.List<TreeInstance> remainingTrees = new System.Collections.Generic.List<TreeInstance>();

            // Keep only trees outside the clearing radius
            foreach (TreeInstance tree in trees)
            {
                Vector3 worldPos = Vector3.Scale(tree.position, terrainData.size) + transform.position;
                if (Vector3.Distance(new Vector3(worldPos.x, 0, worldPos.z), 
                                   new Vector3(center.x, 0, center.z)) > radius)
                {
                    remainingTrees.Add(tree);
                }
            }

            // Update the terrain with remaining trees
            terrainData.SetTreeInstances(remainingTrees.ToArray(), true);

            // Clear any bushes or other vegetation in the area
            Collider[] colliders = Physics.OverlapSphere(center, radius);
            foreach (Collider collider in colliders)
            {
                // Check if the object is a bush or vegetation
                if (collider.gameObject.name.ToLower().Contains("bush") || 
                    collider.gameObject.name.ToLower().Contains("vegetation") ||
                    collider.gameObject.name.ToLower().Contains("plant"))
                {
                    // Only destroy if it's not part of the camp itself
                    if (!collider.gameObject.name.ToLower().Contains("well") &&
                        !collider.gameObject.name.ToLower().Contains("tent") &&
                        !collider.gameObject.name.ToLower().Contains("banner") &&
                        !collider.gameObject.name.ToLower().Contains("fire"))
                    {
                        Destroy(collider.gameObject);
                    }
                }
            }
        }

        private Vector3 GetTerrainNormal(Vector3 worldPos)
        {
            Terrain terrain = GetComponent<Terrain>();
            if (terrain == null) return Vector3.up;

            // Get terrain normal at the point
            TerrainData terrainData = terrain.terrainData;
            Vector3 terrainLocalPos = terrain.transform.InverseTransformPoint(worldPos);
            Vector2 normalizedPos = new Vector2(
                terrainLocalPos.x / terrainData.size.x,
                terrainLocalPos.z / terrainData.size.z
            );

            // Get interpolated normal
            return terrain.terrainData.GetInterpolatedNormal(normalizedPos.x, normalizedPos.y);
        }

        private Vector3 FindCampLocation()
        {
            if (islandGenerator == null)
            {
                Debug.LogError("No ProceduralIslandGenerator found! Make sure it's on the same GameObject.");
                return Vector3.zero;
            }

            if (terrain == null)
            {
                Debug.LogError("No Terrain component found!");
                return Vector3.zero;
            }

            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null)
            {
                Debug.LogError("No TerrainData found!");
                return Vector3.zero;
            }
            
            try
            {
                // Get the island information from the generator
                Vector3 islandCenter = islandGenerator.GetIslandCenter();
                float islandRadius = islandGenerator.IslandRadius;
                
                // Calculate the radius where we want to place the camp (30% of island radius)
                // This ensures we're well within the flat part of the island
                float campPlacementRadius = islandRadius - distanceFromWater;
                
                // Pick a random angle
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                
                // Calculate the position relative to island center
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * campPlacementRadius,
                    0,
                    Mathf.Sin(angle) * campPlacementRadius
                );
                
                Vector3 campPosition = islandCenter + offset;
                
                // Get the exact height at this position
                float terrainHeight = terrain.SampleHeight(campPosition);
                campPosition.y = terrainHeight;
                
                Debug.Log($"Camp placement - Center: {islandCenter}, Radius: {islandRadius}, " +
                         $"Position: {campPosition}, Height: {terrainHeight}");
                
                return campPosition;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during camp placement: {e.Message}\n{e.StackTrace}");
                return Vector3.zero;
            }
        }

        private float GetTerrainHeight(Vector3 worldPosition)
        {
            Terrain terrain = GetComponent<Terrain>();
            if (terrain == null) return 0f;
            return terrain.SampleHeight(worldPosition);
        }

        private void SetupBanner(GameObject banner)
        {
            // Find the TextMesh component or create one
            TextMesh textMesh = banner.GetComponentInChildren<TextMesh>();
            if (textMesh == null)
            {
                GameObject textObj = new GameObject("BannerText");
                textObj.transform.parent = banner.transform;
                textObj.transform.localPosition = Vector3.zero;
                textMesh = textObj.AddComponent<TextMesh>();
            }

            // Set up the text
            textMesh.text = tribeName;
            textMesh.color = bannerColor;
            textMesh.fontSize = 24;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.characterSize = 0.25f;

            // Make text face both directions
            GameObject textBack = Instantiate(textMesh.gameObject, textMesh.transform.position, textMesh.transform.rotation, banner.transform);
            textBack.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }

        private void SpawnPlayerAtCamp()
        {
            if (terrain == null)
            {
                Debug.LogError("No Terrain component found!");
                return;
            }

            // Find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("No player found with 'Player' tag!");
                return;
            }

            // Calculate spawn position slightly offset from the tent
            Vector3 spawnOffset = new Vector3(-1f, 0, 5f); // by the campfire
            Vector3 spawnPosition = tentPosition + spawnOffset;
            
            // Ensure correct height using terrain SampleHeight
            float y = GetTerrainHeight(spawnPosition);
            spawnPosition.y = y + 1f; // Add 1 unit up to prevent ground clipping

            // Move player to spawn position
            player.transform.position = spawnPosition;
            
            // Make player look at tent
            Vector3 lookDirection = tentPosition - spawnPosition;
            lookDirection.y = 0; // Keep the look rotation level
            if (lookDirection != Vector3.zero)
            {
                player.transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            Debug.Log($"Player spawned at camp: {spawnPosition}, Height: {y}");
        }

        private Vector3 FindWellLocation(Vector3 campCenter)
        {
            if (terrain == null || islandGenerator == null)
            {
                Debug.LogError("No Terrain or IslandGenerator component found!");
                return Vector3.zero;
            }

            // Get island information
            Vector3 islandCenter = islandGenerator.GetIslandCenter();
            float islandRadius = islandGenerator.IslandRadius;
            float maxWellRadius = islandRadius * 0.6f; // 60% of island radius

            // Try multiple positions to find a suitable well location
            for (int attempts = 0; attempts < 30; attempts++)
            {
                // Pick a random angle and distance within the specified range
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(minWellDistance, maxWellDistance);
                
                // Calculate position relative to camp center
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * distance,
                    0,
                    Mathf.Sin(angle) * distance
                );
                
                Vector3 wellPosition = campCenter + offset;
                
                // Check if the position is within the island's radius
                float distanceFromIslandCenter = Vector3.Distance(
                    new Vector3(wellPosition.x, 0, wellPosition.z),
                    new Vector3(islandCenter.x, 0, islandCenter.z)
                );
                
                if (distanceFromIslandCenter > maxWellRadius)
                {
                    continue; // Try another position if too far from island center
                }
                
                // Get the exact height at this position
                float terrainHeight = terrain.SampleHeight(wellPosition);
                wellPosition.y = terrainHeight;

                // Check if the position is valid (not in water, not too close to camp)
                if (terrainHeight > defaultIslandHeight && 
                    Vector3.Distance(new Vector3(wellPosition.x, 0, wellPosition.z), 
                                   new Vector3(campCenter.x, 0, campCenter.z)) >= minWellDistance)
                {
                    // Clear trees in the area
                    ClearTreesInArea(wellPosition, wellClearRadius);
                    Debug.Log($"Found well location at {wellPosition}, distance from island center: {distanceFromIslandCenter}, max allowed: {maxWellRadius}");
                    return wellPosition;
                }
            }

            Debug.LogWarning("Could not find suitable well location after 30 attempts!");
            return Vector3.zero;
        }
    }
} 