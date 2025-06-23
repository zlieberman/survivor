using UnityEngine;
using UnityEngine.AI;
using Survivor.Characters;
using System.Collections;

namespace Survivor.Characters
{
    /// <summary>
    /// Static class to manage camp position access without creating circular dependencies
    /// </summary>
    public static class CampPositionManager
    {
        private static Vector3? campPosition = null;
        private static bool isInitialized = false;

        public static void SetCampPosition(Vector3 position)
        {
            campPosition = position;
            isInitialized = true;
            Debug.Log($"[CampPositionManager] Camp position set to: {position}");
        }

        public static Vector3? GetCampPosition()
        {
            return campPosition;
        }

        public static bool IsInitialized()
        {
            return isInitialized;
        }

        public static void ClearCampPosition()
        {
            campPosition = null;
            isInitialized = false;
            Debug.Log("[CampPositionManager] Camp position cleared");
        }
    }

    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Character))]
    public class NPCBehavior : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float minDistanceToOtherNPCs = 2f;
        public float wanderRadius = 15f;
        public float minIdleTime = 2f;
        public float maxIdleTime = 8f;
        public float minWanderTime = 3f;
        public float maxWanderTime = 12f;

        [Header("Behavior Settings")]
        public float maxDistanceFromCamp = 30f;
        public bool avoidWater = true;
        public float waterAvoidanceDistance = 5f;
        public LayerMask waterLayer = 1 << 4; // Water layer

        [Header("Animation")]
        public Animator animator;
        public string speedParameterName = "Speed";
        public string isMovingParameterName = "IsMoving";

        // Animation IDs (matching ThirdPersonController)
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        [Header("Debug Visualization")]
        public bool enableDebugVisualization = true;
        public bool showNavMeshArea = true;
        public bool showPath = true;
        public bool showObstacles = true;
        public bool showWanderRadius = true;
        public bool showCampDistance = true;
        public bool showAgentInfo = true;
        public Color pathColor = Color.green;
        public Color obstacleColor = Color.red;
        public Color wanderRadiusColor = Color.yellow;
        public Color campDistanceColor = Color.red;
        public Color agentInfoColor = Color.white;

        private NavMeshAgent agent;
        private Character character;
        private UnityEngine.CharacterController characterController;
        private NPCCharacter npcCharacter; // Reference to NPCCharacter to check dialogue state
        private float idleTimer;
        private float wanderTimer;
        private bool isIdle;
        private bool isWandering;
        private Vector3? campPosition;
        private Vector3 startPosition;
        private Vector3 previousPosition;
        private float currentWanderTime;
        private LayerMask actualWaterLayer;
        private bool isPaused = false; // New field to track pause state

        // Behavior states
        private enum BehaviorState
        {
            Idle,
            Wandering,
            ReturningToCamp
        }
        private BehaviorState currentState = BehaviorState.Idle;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            character = GetComponent<Character>();
            characterController = GetComponent<UnityEngine.CharacterController>();
            npcCharacter = GetComponent<NPCCharacter>(); // Get NPCCharacter reference

            // Validate required components
            if (agent == null)
            {
                Debug.LogError($"[NPCBehavior] NavMeshAgent component missing on {gameObject.name}");
                enabled = false;
                return;
            }

            if (character == null)
            {
                Debug.LogError($"[NPCBehavior] Character component missing on {gameObject.name}");
                enabled = false;
                return;
            }

            // Set agent speed based on character stats
            if (character.Stats != null)
            {
                agent.speed = Mathf.Clamp(character.Stats.speed * 0.1f, 1f, 5f);
            }
            else
            {
                agent.speed = 2f; // Default speed
            }

            // Set other agent properties
            agent.stoppingDistance = 0.5f;
            agent.angularSpeed = 120f;
            agent.acceleration = 8f;

            // Auto-assign animator if not already set
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator != null)
                {
                    Debug.Log($"[NPCBehavior] Auto-assigned animator to {gameObject.name}");
                }
                else
                {
                    Debug.LogWarning($"[NPCBehavior] No Animator component found on {gameObject.name}, animations will be disabled");
                }
            }

            // Create NPC-specific animator controller if using player controller
            CreateNPCAnimatorController();

            // Assign animation IDs
            AssignAnimationIDs();

            // Store starting position
            startPosition = transform.position;
            previousPosition = transform.position;

            // Initialize water layer detection
            InitializeWaterLayer();

            // Find camp position
            FindCampPosition();

            // Check if NavMesh is available or wait for NavMeshLoadingManager
            if (!IsNavMeshAvailable())
            {
                Debug.LogWarning($"[NPCBehavior] NavMesh not available for {gameObject.name}, waiting for NavMeshLoadingManager");
                StartCoroutine(WaitForNavMeshLoadingManager());
            }
            else
            {
                // Start with idle state
                SetIdleState();
            }
        }

        private void Update()
        {
            // Don't update behavior if paused or in dialogue
            if (isPaused) return;
            
            // Check if NPC is in dialogue - if so, don't move
            if (npcCharacter != null && npcCharacter.IsInDialogue)
            {
                return;
            }
            
            UpdateBehavior();
            UpdateAnimation();
        }

        private void LateUpdate()
        {
            // Don't update movement if paused or in dialogue
            if (isPaused) return;
            
            // Check if NPC is in dialogue - if so, don't move
            if (npcCharacter != null && npcCharacter.IsInDialogue)
            {
                return;
            }
            
            // Sync character transform with NavMeshAgent movement
            if (agent != null && agent.isOnNavMesh && agent.velocity.magnitude > 0.1f)
            {
                // Update position to match agent
                transform.position = agent.nextPosition;
                
                // Update rotation to face movement direction
                if (agent.velocity.magnitude > 0.1f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * agent.angularSpeed);
                }
            }
        }

        private void UpdateBehavior()
        {
            switch (currentState)
            {
                case BehaviorState.Idle:
                    UpdateIdleState();
                    break;
                case BehaviorState.Wandering:
                    UpdateWanderingState();
                    break;
                case BehaviorState.ReturningToCamp:
                    UpdateReturningToCampState();
                    break;
            }
        }

        private void UpdateIdleState()
        {
            idleTimer -= Time.deltaTime;
            if (idleTimer <= 0)
            {
                // Decide whether to wander or return to camp
                if (ShouldReturnToCamp())
                {
                    SetReturningToCampState();
                }
                else
                {
                    SetWanderingState();
                }
            }
        }

        private void UpdateWanderingState()
        {
            // Check if agent is valid and on NavMesh
            if (agent == null || !agent.isOnNavMesh)
            {
                SetIdleState();
                return;
            }

            wanderTimer -= Time.deltaTime;
            
            // Check if we've reached our destination
            if (agent != null && agent.isOnNavMesh && agent.remainingDistance <= agent.stoppingDistance)
            {
                SetIdleState();
                return;
            }

            // Check if we should stop wandering
            if (wanderTimer <= 0)
            {
                SetIdleState();
                return;
            }

            // Check if we're stuck
            if (agent != null && agent.isOnNavMesh && agent.velocity.magnitude < 0.1f && agent.remainingDistance > agent.stoppingDistance)
            {
                // Try to find a new destination
                if (!SetNewDestination())
                {
                    SetIdleState();
                }
            }
        }

        private void UpdateReturningToCampState()
        {
            // Check if agent is valid and on NavMesh
            if (agent == null || !agent.isOnNavMesh)
            {
                SetIdleState();
                return;
            }

            if (agent != null && agent.isOnNavMesh && agent.remainingDistance <= agent.stoppingDistance)
            {
                SetIdleState();
                return;
            }

            // If we're close enough to camp, switch to idle
            if (campPosition.HasValue && Vector3.Distance(transform.position, campPosition.Value) < wanderRadius * 0.5f)
            {
                SetIdleState();
            }
        }

        private void SetIdleState()
        {
            currentState = BehaviorState.Idle;
            isIdle = true;
            isWandering = false;
            idleTimer = Random.Range(minIdleTime, maxIdleTime);
            
            // Only reset path if agent is valid and on NavMesh
            if (agent != null && agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
        }

        private void SetWanderingState()
        {
            currentState = BehaviorState.Wandering;
            isIdle = false;
            isWandering = true;
            currentWanderTime = Random.Range(minWanderTime, maxWanderTime);
            wanderTimer = currentWanderTime;
            
            // Only set destination if agent is valid and on NavMesh
            if (agent != null && agent.isOnNavMesh)
            {
                if (!SetNewDestination())
                {
                    SetIdleState();
                }
            }
            else
            {
                // If agent is not valid, just go idle
                SetIdleState();
            }
        }

        private void SetReturningToCampState()
        {
            currentState = BehaviorState.ReturningToCamp;
            isIdle = false;
            isWandering = false;

            // Only set destination if agent is valid and on NavMesh
            if (agent != null && agent.isOnNavMesh)
            {
                if (campPosition.HasValue)
                {
                    agent.SetDestination(campPosition.Value);
                }
                else
                {
                    // If no camp position, just go back to start position
                    agent.SetDestination(startPosition);
                }
            }
            else
            {
                // If agent is not valid, just go idle
                SetIdleState();
            }
        }

        private bool SetNewDestination()
        {
            // Check if agent is valid and on NavMesh
            if (agent == null || !agent.isOnNavMesh)
            {
                return false;
            }

            Vector3 basePosition = campPosition.HasValue ? campPosition.Value : startPosition;
            
            for (int attempts = 0; attempts < 10; attempts++)
            {
                // Generate random direction within wander radius
                Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
                randomDirection += basePosition;
                
                // Sample position on NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
                {
                    // Check if position is valid
                    if (IsValidPosition(hit.position))
                    {
                        agent.SetDestination(hit.position);
                        return true;
                    }
                }
            }
            
            return false;
        }

        private bool IsValidPosition(Vector3 position)
        {
            // Check distance from camp
            if (campPosition.HasValue)
            {
                float distanceFromCamp = Vector3.Distance(position, campPosition.Value);
                if (distanceFromCamp > maxDistanceFromCamp)
                {
                    return false;
                }
            }

            // Check for other NPCs in the area
            Collider[] colliders = Physics.OverlapSphere(position, minDistanceToOtherNPCs);
            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject && collider.GetComponent<NPCBehavior>() != null)
                {
                    return false;
                }
            }

            // Check for water if avoiding water
            if (avoidWater)
            {
                Collider[] waterColliders = Physics.OverlapSphere(position, waterAvoidanceDistance, actualWaterLayer);
                if (waterColliders.Length > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private bool ShouldReturnToCamp()
        {
            if (!campPosition.HasValue) return false;

            float distanceFromCamp = Vector3.Distance(transform.position, campPosition.Value);
            float returnThreshold = maxDistanceFromCamp * 0.8f; // Return when 80% of max distance

            // Higher chance to return as we get further from camp
            float returnChance = Mathf.Clamp01((distanceFromCamp - wanderRadius) / (maxDistanceFromCamp - wanderRadius));
            return Random.value < returnChance;
        }

        private void FindCampPosition()
        {
            // First, try to get camp position from the static manager
            if (CampPositionManager.IsInitialized())
            {
                campPosition = CampPositionManager.GetCampPosition();
                if (campPosition.HasValue)
                {
                    Debug.Log($"[NPCBehavior] Found camp position via CampPositionManager: {campPosition}");
                    return;
                }
            }

            // Fallback: Look for camp objects in the scene
            // Look for tent or campfire
            GameObject tent = GameObject.FindGameObjectWithTag("Tent");
            if (tent != null)
            {
                campPosition = tent.transform.position;
                Debug.Log($"[NPCBehavior] Found camp position via Tent tag: {campPosition}");
                return;
            }

            GameObject campfire = GameObject.FindGameObjectWithTag("Campfire");
            if (campfire != null)
            {
                campPosition = campfire.transform.position;
                Debug.Log($"[NPCBehavior] Found camp position via Campfire tag: {campPosition}");
                return;
            }

            // Look for any object with "tent" or "camp" in the name
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("tent") || obj.name.ToLower().Contains("camp"))
                {
                    campPosition = obj.transform.position;
                    Debug.Log($"[NPCBehavior] Found camp position via object name '{obj.name}': {campPosition}");
                    return;
                }
            }

            // Look for objects with "CampSpawnPoint" in the name
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("CampSpawnPoint"))
                {
                    campPosition = obj.transform.position;
                    Debug.Log($"[NPCBehavior] Found camp position via CampSpawnPoint: {campPosition}");
                    return;
                }
            }

            Debug.LogWarning($"[NPCBehavior] Could not find camp position for {gameObject.name}, will use start position as center");
        }

        private void UpdateAnimation()
        {
            if (animator != null)
            {
                // Use actual movement velocity from transform for more accurate animation
                Vector3 movementVelocity = (transform.position - previousPosition) / Time.deltaTime;
                float speed = movementVelocity.magnitude;
                
                // Fallback to agent velocity if transform movement is too small
                if (speed < 0.1f && agent != null && agent.isOnNavMesh)
                {
                    speed = agent.velocity.magnitude;
                }
                
                // Set animation parameters using IDs (matching ThirdPersonController)
                animator.SetFloat(_animIDSpeed, speed);
                animator.SetFloat(_animIDMotionSpeed, speed > 0.1f ? 1f : 0f);
                animator.SetBool(_animIDGrounded, true); // NPCs are always grounded
                animator.SetBool(_animIDJump, false);
                animator.SetBool(_animIDFreeFall, false);
            }
            previousPosition = transform.position;
        }

        // Public methods for external control
        public void StopWandering()
        {
            SetIdleState();
        }

        public void SetDestination(Vector3 destination)
        {
            // Check if agent is valid and on NavMesh
            if (agent != null && agent.isOnNavMesh)
            {
                agent.SetDestination(destination);
                currentState = BehaviorState.Wandering;
                isWandering = true;
                isIdle = false;
            }
            else
            {
                Debug.LogWarning($"[NPCBehavior] Cannot set destination for {gameObject.name} - agent is not valid or not on NavMesh");
            }
        }

        public void ReturnToCamp()
        {
            SetReturningToCampState();
        }

        public void Pause(bool pause)
        {
            isPaused = pause;
            if (isPaused)
            {
                Debug.Log($"[NPCBehavior] Pausing movement for {gameObject.name}");
                // Stop the agent and reset path when pausing
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                }
            }
            else
            {
                Debug.Log($"[NPCBehavior] Resuming movement for {gameObject.name}");
            }
        }

        public bool IsWandering => isWandering;
        public bool IsIdle => isIdle;
        public bool IsPaused => isPaused;
        public Vector3? CurrentDestination => (agent != null && agent.isOnNavMesh && agent.hasPath) ? agent.destination : null;
        public bool IsReady => agent != null && agent.isOnNavMesh && enabled;

        // Public properties for debug manager access
        public bool EnableDebugVisualization
        {
            get => enableDebugVisualization;
            set => enableDebugVisualization = value;
        }

        public bool ShowNavMeshArea
        {
            get => showNavMeshArea;
            set => showNavMeshArea = value;
        }

        public bool ShowPath
        {
            get => showPath;
            set => showPath = value;
        }

        public bool ShowObstacles
        {
            get => showObstacles;
            set => showObstacles = value;
        }

        // Debug method to test wandering behavior
        [ContextMenu("Test Wandering")]
        public void TestWandering()
        {
            Debug.Log($"[NPCBehavior] Testing wandering for {gameObject.name}");
            SetWanderingState();
        }

        [ContextMenu("Stop Wandering")]
        public void TestStopWandering()
        {
            Debug.Log($"[NPCBehavior] Stopping wandering for {gameObject.name}");
            SetIdleState();
        }

        [ContextMenu("Return to Camp")]
        public void TestReturnToCamp()
        {
            Debug.Log($"[NPCBehavior] Returning to camp for {gameObject.name}");
            SetReturningToCampState();
        }

        [ContextMenu("Wait for NavMesh")]
        public void TestWaitForNavMesh()
        {
            Debug.Log($"[NPCBehavior] Forcing wait for NavMesh for {gameObject.name}");
            if (!IsNavMeshAvailable())
            {
                StartCoroutine(WaitForNavMeshLoadingManager());
            }
            else
            {
                Debug.Log($"[NPCBehavior] NavMesh is already available for {gameObject.name}");
            }
        }

        [ContextMenu("Enable NavMesh Debugging")]
        public void EnableNavMeshDebugging()
        {
            if (agent != null)
            {
                // Enable detailed logging for NavMeshAgent
                Debug.Log($"[NPCBehavior] Enabled NavMesh debugging for {gameObject.name}");
                Debug.Log($"[NPCBehavior] Agent enabled: {agent.enabled}");
                Debug.Log($"[NPCBehavior] Agent isOnNavMesh: {agent.isOnNavMesh}");
                Debug.Log($"[NPCBehavior] Agent hasPath: {(agent.isOnNavMesh ? agent.hasPath.ToString() : "N/A (not on NavMesh)")}");
                Debug.Log($"[NPCBehavior] Agent speed: {agent.speed}");
                Debug.Log($"[NPCBehavior] Agent radius: {agent.radius}");
                Debug.Log($"[NPCBehavior] Agent height: {agent.height}");
            }
        }

        [ContextMenu("Disable NavMesh Debugging")]
        public void DisableNavMeshDebugging()
        {
            if (agent != null)
            {
                Debug.Log($"[NPCBehavior] Disabled NavMesh debugging for {gameObject.name}");
            }
        }

        [ContextMenu("Force Reposition on NavMesh")]
        public void ForceRepositionOnNavMesh()
        {
            if (agent != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                    Debug.Log($"[NPCBehavior] Repositioned {gameObject.name} to NavMesh position: {hit.position}");
                }
                else
                {
                    Debug.LogWarning($"[NPCBehavior] Could not find valid NavMesh position for {gameObject.name}");
                }
            }
        }

        [ContextMenu("Debug NavMesh Status")]
        public void DebugNavMeshStatus()
        {
            Debug.Log($"[NPCBehavior] === NavMesh Debug for {gameObject.name} ===");
            Debug.Log($"[NPCBehavior] Current position: {transform.position}");
            
            if (agent != null)
            {
                Debug.Log($"[NPCBehavior] Agent enabled: {agent.enabled}");
                Debug.Log($"[NPCBehavior] Agent isOnNavMesh: {agent.isOnNavMesh}");
                Debug.Log($"[NPCBehavior] Agent hasPath: {(agent.isOnNavMesh ? agent.hasPath.ToString() : "N/A")}");
                
                // Test NavMesh sampling at current position
                NavMeshHit hit;
                bool canSample = NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas);
                Debug.Log($"[NPCBehavior] Can sample NavMesh at current position: {canSample}");
                if (canSample)
                {
                    Debug.Log($"[NPCBehavior] Nearest NavMesh position: {hit.position}");
                    Debug.Log($"[NPCBehavior] Distance to NavMesh: {hit.distance}");
                    Debug.Log($"[NPCBehavior] NavMesh area mask: {hit.mask}");
                }
                
                // Test NavMesh sampling at a slightly offset position
                Vector3 testPos = transform.position + Vector3.up * 0.1f;
                bool canSampleOffset = NavMesh.SamplePosition(testPos, out hit, 5f, NavMesh.AllAreas);
                Debug.Log($"[NPCBehavior] Can sample NavMesh at offset position {testPos}: {canSampleOffset}");
                
                // Check if NavMesh is built
                bool navMeshBuilt = NavMesh.SamplePosition(Vector3.zero, out hit, 1000f, NavMesh.AllAreas);
                Debug.Log($"[NPCBehavior] NavMesh appears to be built: {navMeshBuilt}");
            }
            else
            {
                Debug.LogError($"[NPCBehavior] No NavMeshAgent found on {gameObject.name}!");
            }
            
            Debug.Log($"[NPCBehavior] === End NavMesh Debug ===");
        }

        [ContextMenu("Test Path Finding")]
        public void TestPathFinding()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                Vector3 testDestination = transform.position + Vector3.forward * 10f;
                NavMeshPath testPath = new NavMeshPath();
                
                if (NavMesh.CalculatePath(transform.position, testDestination, NavMesh.AllAreas, testPath))
                {
                    Debug.Log($"[NPCBehavior] Path found to {testDestination} with {testPath.corners.Length} corners");
                    agent.SetDestination(testDestination);
                }
                else
                {
                    Debug.LogWarning($"[NPCBehavior] No path found to {testDestination}");
                }
            }
            else
            {
                Debug.LogWarning($"[NPCBehavior] Agent is not on NavMesh for {gameObject.name}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!enableDebugVisualization) return;

            // Draw wander radius
            if (showWanderRadius)
            {
                Gizmos.color = wanderRadiusColor;
                Vector3 center = campPosition.HasValue ? campPosition.Value : startPosition;
                Gizmos.DrawWireSphere(center, wanderRadius);
            }

            // Draw max distance from camp
            if (showCampDistance && campPosition.HasValue)
            {
                Gizmos.color = campDistanceColor;
                Gizmos.DrawWireSphere(campPosition.Value, maxDistanceFromCamp);
            }

            // Draw minimum distance to other NPCs
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, minDistanceToOtherNPCs);

            // Draw current destination and path
            if (showPath && agent != null && agent.isOnNavMesh && agent.hasPath)
            {
                Gizmos.color = pathColor;
                Gizmos.DrawLine(transform.position, agent.destination);
                Gizmos.DrawWireSphere(agent.destination, 0.5f);
                
                // Draw the full path
                DrawNavMeshPath(agent.path);
            }

            // Draw NavMesh area around the agent
            if (showNavMeshArea)
            {
                DrawNavMeshArea();
            }

            // Draw obstacles
            if (showObstacles)
            {
                DrawObstacles();
            }

            // Draw agent info
            if (showAgentInfo)
            {
                DrawAgentInfo();
            }
        }

        private bool IsNavMeshAvailable()
        {
            NavMeshHit hit;
            return NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas);
        }

        private IEnumerator WaitForNavMeshLoadingManager()
        {
            // Wait for NavMeshLoadingManager to be available using static interface
            while (!IsNavMeshLoadingManagerAvailable())
            {
                yield return null;
            }

            // Wait for NavMesh to be ready using static interface
            while (!IsNavMeshLoadingManagerReady())
            {
                yield return null;
            }

            Debug.Log($"[NPCBehavior] NavMesh is now available for {gameObject.name}, starting behavior");
            
            // Ensure agent is properly positioned on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                Debug.Log($"[NPCBehavior] Repositioned {gameObject.name} to valid NavMesh position: {hit.position}");
            }
            
            SetIdleState();
        }

        // Static interface methods to avoid Core namespace dependency
        private bool IsNavMeshLoadingManagerAvailable()
        {
            // Use reflection to check if NavMeshLoadingManager exists without direct reference
            System.Type navMeshManagerType = System.Type.GetType("Survivor.Core.NavMeshLoadingManager, Assembly-CSharp");
            if (navMeshManagerType != null)
            {
                var instanceProperty = navMeshManagerType.GetProperty("Instance");
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    return instance != null;
                }
            }
            return false;
        }

        private bool IsNavMeshLoadingManagerReady()
        {
            // Use reflection to check NavMesh status without direct reference
            System.Type navMeshManagerType = System.Type.GetType("Survivor.Core.NavMeshLoadingManager, Assembly-CSharp");
            if (navMeshManagerType != null)
            {
                var instanceProperty = navMeshManagerType.GetProperty("Instance");
                if (instanceProperty != null)
                {
                    var instance = instanceProperty.GetValue(null);
                    if (instance != null)
                    {
                        var isReadyProperty = navMeshManagerType.GetProperty("IsNavMeshReady");
                        if (isReadyProperty != null)
                        {
                            return (bool)isReadyProperty.GetValue(instance);
                        }
                    }
                }
            }
            return false;
        }

        private void InitializeWaterLayer()
        {
            // Try to find water layer automatically
            int waterLayerIndex = LayerMask.NameToLayer("Water");
            if (waterLayerIndex != -1)
            {
                actualWaterLayer = 1 << waterLayerIndex;
                Debug.Log($"[NPCBehavior] Found Water layer at index {waterLayerIndex}");
            }
            else
            {
                // Fallback to the configured water layer
                actualWaterLayer = waterLayer;
                Debug.Log($"[NPCBehavior] Using configured water layer: {waterLayer.value}");
            }

            // If water avoidance is disabled, set to empty mask
            if (!avoidWater)
            {
                actualWaterLayer = 0;
            }
        }

        private void DrawNavMeshPath(NavMeshPath path)
        {
            if (path == null || path.corners.Length < 2) return;

            Gizmos.color = pathColor;
            
            // Draw lines between all path corners
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
                
                // Draw small spheres at each corner
                Gizmos.DrawWireSphere(path.corners[i], 0.2f);
            }
            
            // Draw the last corner
            Gizmos.DrawWireSphere(path.corners[path.corners.Length - 1], 0.2f);
        }

        private void DrawNavMeshArea()
        {
            if (agent == null) return;

            // Sample NavMesh around the agent to show walkable areas
            float sampleRadius = 5f;
            int sampleCount = 20;
            
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green
            
            for (int i = 0; i < sampleCount; i++)
            {
                float angle = (i / (float)sampleCount) * 360f * Mathf.Deg2Rad;
                Vector3 samplePos = transform.position + new Vector3(
                    Mathf.Cos(angle) * sampleRadius,
                    0,
                    Mathf.Sin(angle) * sampleRadius
                );
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(samplePos, out hit, sampleRadius, NavMesh.AllAreas))
                {
                    Gizmos.DrawWireSphere(hit.position, 0.1f);
                }
            }
            
            // Draw the agent's current NavMesh area
            NavMeshHit agentHit;
            if (NavMesh.SamplePosition(transform.position, out agentHit, 1f, NavMesh.AllAreas))
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(agentHit.position, 0.3f);
            }
        }

        private void DrawObstacles()
        {
            if (agent == null) return;

            // Draw obstacles in the agent's path
            float obstacleCheckRadius = 3f;
            Collider[] obstacles = Physics.OverlapSphere(transform.position, obstacleCheckRadius);
            
            Gizmos.color = obstacleColor;
            
            foreach (var obstacle in obstacles)
            {
                if (obstacle.gameObject != gameObject)
                {
                    // Draw line to obstacle
                    Gizmos.DrawLine(transform.position, obstacle.transform.position);
                    
                    // Draw sphere around obstacle
                    Gizmos.DrawWireSphere(obstacle.transform.position, 0.5f);
                }
            }
            
            // Draw water obstacles if avoiding water
            if (avoidWater && actualWaterLayer != 0)
            {
                Collider[] waterObstacles = Physics.OverlapSphere(transform.position, waterAvoidanceDistance, actualWaterLayer);
                Gizmos.color = Color.blue;
                
                foreach (var water in waterObstacles)
                {
                    Gizmos.DrawWireSphere(water.transform.position, 1f);
                    Gizmos.DrawLine(transform.position, water.transform.position);
                }
            }
            
            // Draw other NPCs that might be blocking
            NPCBehavior[] otherNPCs = FindObjectsOfType<NPCBehavior>();
            Gizmos.color = Color.magenta;
            
            foreach (var otherNPC in otherNPCs)
            {
                if (otherNPC != this && otherNPC.gameObject != gameObject)
                {
                    float distance = Vector3.Distance(transform.position, otherNPC.transform.position);
                    if (distance < minDistanceToOtherNPCs)
                    {
                        Gizmos.DrawLine(transform.position, otherNPC.transform.position);
                        Gizmos.DrawWireSphere(otherNPC.transform.position, 0.3f);
                    }
                }
            }
        }

        private void DrawAgentInfo()
        {
            if (agent == null) return;

            // This will be handled by OnGUI for better text rendering
            // Here we just draw some visual indicators
            
            // Draw agent's forward direction
            Gizmos.color = agentInfoColor;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
            
            // Draw agent's velocity
            if (agent.velocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, agent.velocity);
            }
            
            // Draw agent's radius
            Gizmos.color = new Color(agentInfoColor.r, agentInfoColor.g, agentInfoColor.b, 0.3f);
            Gizmos.DrawWireSphere(transform.position, agent.radius);
        }

        [ContextMenu("Assign Animator")]
        public void AssignAnimator()
        {
            animator = GetComponent<Animator>();
            if (animator != null)
            {
                Debug.Log($"[NPCBehavior] Manually assigned animator to {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"[NPCBehavior] No Animator component found on {gameObject.name}");
            }
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void CreateNPCAnimatorController()
        {
            if (animator == null) return;

            // Disable ThirdPersonController to prevent animation event conflicts
            var thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
            if (thirdPersonController != null)
            {
                thirdPersonController.enabled = false;
                Debug.Log($"[NPCBehavior] Disabled ThirdPersonController on {gameObject.name} to prevent animation event conflicts");
            }

            // Ensure we have the basic animation parameters set up
            if (animator.runtimeAnimatorController != null)
            {
                Debug.Log($"[NPCBehavior] Using animator controller: {animator.runtimeAnimatorController.name} for {gameObject.name}");
            }
        }
    }
} 