using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Survivor.Characters;
using Survivor.Characters.Dialogue;
using Survivor.Shared;
using Survivor.Generation;

namespace Survivor.Characters
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Character))]
    public class NPCWanderBehavior : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [SerializeField] private float patrolRadius = 30f;
        [SerializeField] private float minIdleTime = 2f;
        [SerializeField] private float maxIdleTime = 6f;
        [SerializeField] private float minDistanceToOtherNPCs = 2f;
        [SerializeField] private Transform patrolCenter;
        
        [Header("Animation Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParameterName = "Speed";
        [SerializeField] private string idleParameterName = "IsIdle";
        
        [Header("Player Awareness")]
        [SerializeField] private float playerDetectionRadius = 5f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float playerReactionDelay = 0.5f;
        
        private NavMeshAgent agent;
        private Character character;
        private bool isIdle;
        private bool isInDialogue;
        private float idleTimer;
        private Vector3 currentDestination;
        private bool isPlayerNearby;
        private Coroutine playerReactionCoroutine;
        private bool isInitialized = false;
        private bool isNavMeshReady = false;
        
        private void Awake()
        {
            Debug.Log($"[{gameObject.name}] Awake called");
            // Only initialize if the game object is active
            if (!gameObject.activeInHierarchy)
            {
                Debug.Log($"[{gameObject.name}] Game object is not active, skipping initialization");
                return;
            }

            agent = GetComponent<NavMeshAgent>();
            character = GetComponent<Character>();
            
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                Debug.Log($"[{gameObject.name}] Animator was null, got from component");
            }
            
            // Set default patrol center if not assigned
            if (patrolCenter == null)
            {
                var campGenerator = FindObjectOfType<CampGenerator>();
                if (campGenerator != null && campGenerator.CampSpawnPoint != null)
                {
                    patrolCenter = campGenerator.CampSpawnPoint;
                    Debug.Log($"[{gameObject.name}] Set patrol center to camp spawn point");
                }
                else
                {
                    patrolCenter = transform;
                    Debug.Log($"[{gameObject.name}] Set patrol center to self");
                }
            }

            isInitialized = true;
            Debug.Log($"[{gameObject.name}] Initialization complete");
        }
        
        private void Start()
        {
            Debug.Log($"[{gameObject.name}] Start called");
            // Only start if the game object is active and initialized
            if (!gameObject.activeInHierarchy || !isInitialized)
            {
                Debug.Log($"[{gameObject.name}] Not active or not initialized, skipping start");
                return;
            }

            // Initialize agent settings
            agent.speed = character.Stats.speed;
            agent.stoppingDistance = 0.1f;
            agent.autoBraking = true;
            
            Debug.Log($"[{gameObject.name}] Starting NavMesh check");
            // Start checking for NavMesh readiness
            StartCoroutine(WaitForNavMesh());
        }

        private IEnumerator WaitForNavMesh()
        {
            float timeout = 10f; // 10 seconds timeout
            float elapsed = 0f;

            while (elapsed < timeout && !isNavMeshReady)
            {
                if (!gameObject.activeInHierarchy)
                {
                    Debug.Log($"[{gameObject.name}] Game object became inactive while waiting for NavMesh");
                    yield break;
                }

                // Check if we can find a valid position on the NavMesh
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
                {
                    // Verify we can actually move on the NavMesh
                    NavMeshPath path = new NavMeshPath();
                    if (NavMesh.CalculatePath(transform.position, transform.position + Vector3.forward * 5f, NavMesh.AllAreas, path))
                    {
                        isNavMeshReady = true;
                        Debug.Log($"[{gameObject.name}] NavMesh is ready and verified at position {transform.position}");
                        // Start wandering behavior once NavMesh is ready
                        StartCoroutine(WanderRoutine());
                        yield break;
                    }
                    else
                    {
                        Debug.Log($"[{gameObject.name}] Found NavMesh position but cannot calculate path");
                    }
                }

                elapsed += Time.deltaTime;
                if (elapsed % 1f < 0.1f) // Log every second
                {
                    Debug.Log($"[{gameObject.name}] Still waiting for NavMesh... {elapsed:F1}s elapsed");
                }
                yield return new WaitForSeconds(0.5f);
            }

            if (!isNavMeshReady)
            {
                Debug.LogWarning($"[{gameObject.name}] NavMesh not ready after {timeout} seconds");
            }
        }
        
        private void Update()
        {
            // Only update if the game object is active, initialized, and NavMesh is ready
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady)
            {
                return;
            }

            if (!isInDialogue)
            {
                // Update animation parameters
                if (animator != null)
                {
                    animator.SetFloat(speedParameterName, agent.velocity.magnitude);
                    animator.SetBool(idleParameterName, isIdle);
                }
                
                // Check for player proximity
                CheckPlayerProximity();
            }
        }
        
        private IEnumerator WanderRoutine()
        {
            Debug.Log($"[{gameObject.name}] Starting wander routine");
            // Only run if the game object is active, initialized, and NavMesh is ready
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady)
            {
                Debug.Log($"[{gameObject.name}] Cannot start wander routine - not ready");
                yield break;
            }

            while (gameObject.activeInHierarchy)
            {
                if (!isIdle && !isInDialogue)
                {
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        Debug.Log($"[{gameObject.name}] Reached destination, starting idle");
                        StartIdle();
                    }
                }
                yield return null;
            }
        }
        
        private void StartIdle()
        {
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady)
            {
                return;
            }

            isIdle = true;
            idleTimer = Random.Range(minIdleTime, maxIdleTime);
            Debug.Log($"[{gameObject.name}] Starting idle for {idleTimer:F1} seconds");
            StartCoroutine(IdleRoutine());
        }
        
        private IEnumerator IdleRoutine()
        {
            yield return new WaitForSeconds(idleTimer);
            if (gameObject.activeInHierarchy && isInitialized && isNavMeshReady)
            {
                isIdle = false;
                Debug.Log($"[{gameObject.name}] Idle finished, setting new destination");
                SetNewDestination();
            }
        }
        
        private void SetNewDestination()
        {
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady || isInDialogue)
            {
                Debug.Log($"[{gameObject.name}] Cannot set new destination - not ready or in dialogue");
                return;
            }
            
            Debug.Log($"[{gameObject.name}] Attempting to find new destination from position: {transform.position}");
            
            // Try multiple positions with increasing radius
            float[] radiusAttempts = new float[] { patrolRadius * 0.5f, patrolRadius, patrolRadius * 1.5f };
            
            foreach (float radius in radiusAttempts)
            {
                for (int attempts = 0; attempts < 5; attempts++)
                {
                    Vector3 randomDirection = Random.insideUnitSphere * radius;
                    randomDirection += patrolCenter.position;
                    NavMeshHit hit;
                    
                    Debug.Log($"[{gameObject.name}] Trying to find NavMesh position with radius {radius} at {randomDirection}");
                    
                    if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
                    {
                        Debug.Log($"[{gameObject.name}] Found NavMesh position at {hit.position}");
                        
                        // Verify we can actually reach this position
                        NavMeshPath path = new NavMeshPath();
                        if (NavMesh.CalculatePath(transform.position, hit.position, NavMesh.AllAreas, path))
                        {
                            Debug.Log($"[{gameObject.name}] Successfully calculated path to position");
                            
                            // Check if the new position is valid (not too close to other NPCs)
                            if (IsValidPosition(hit.position))
                            {
                                currentDestination = hit.position;
                                agent.SetDestination(currentDestination);
                                Debug.Log($"[{gameObject.name}] Set new destination: {currentDestination}, Distance: {Vector3.Distance(transform.position, currentDestination)}");
                                return;
                            }
                            else
                            {
                                Debug.Log($"[{gameObject.name}] Position too close to other NPCs, trying again");
                            }
                        }
                        else
                        {
                            Debug.Log($"[{gameObject.name}] Could not calculate path to position, trying again");
                        }
                    }
                    else
                    {
                        Debug.Log($"[{gameObject.name}] Could not find valid NavMesh position, trying again");
                    }
                    
                    // If we couldn't find a valid position, try a new random direction
                    randomDirection = Random.insideUnitSphere * radius;
                    randomDirection += patrolCenter.position;
                }
            }
            
            Debug.LogWarning($"[{gameObject.name}] Could not find valid destination after multiple attempts");
            StartCoroutine(RetrySetDestination());
        }
        
        private IEnumerator RetrySetDestination()
        {
            yield return new WaitForSeconds(0.1f);
            if (gameObject.activeInHierarchy && isInitialized && isNavMeshReady)
            {
                SetNewDestination();
            }
        }
        
        private bool IsValidPosition(Vector3 position)
        {
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady)
            {
                return false;
            }

            // Check for other NPCs in the area
            Collider[] colliders = Physics.OverlapSphere(position, minDistanceToOtherNPCs);
            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject && 
                    collider.gameObject.activeInHierarchy && 
                    collider.GetComponent<NPCWanderBehavior>() != null)
                {
                    return false;
                }
            }
            return true;
        }
        
        private void CheckPlayerProximity()
        {
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady)
            {
                return;
            }

            bool wasPlayerNearby = isPlayerNearby;
            Collider[] colliders = Physics.OverlapSphere(transform.position, playerDetectionRadius, playerLayer);
            isPlayerNearby = colliders.Length > 0;
            
            // Handle player proximity state change
            if (wasPlayerNearby != isPlayerNearby)
            {
                if (isPlayerNearby)
                {
                    Debug.Log($"[{gameObject.name}] Player detected nearby");
                    if (playerReactionCoroutine != null)
                    {
                        StopCoroutine(playerReactionCoroutine);
                    }
                    playerReactionCoroutine = StartCoroutine(ReactToPlayer());
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] Player no longer nearby");
                    if (playerReactionCoroutine != null)
                    {
                        StopCoroutine(playerReactionCoroutine);
                        playerReactionCoroutine = null;
                    }
                    ResumeWandering();
                }
            }
        }
        
        private IEnumerator ReactToPlayer()
        {
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady)
            {
                yield break;
            }

            // Stop current movement
            agent.isStopped = true;
            isIdle = true;
            
            // Wait for reaction delay
            yield return new WaitForSeconds(playerReactionDelay);
            
            // Face the player
            if (isPlayerNearby && gameObject.activeInHierarchy)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    Vector3 direction = (player.transform.position - transform.position).normalized;
                    direction.y = 0;
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
        
        private void ResumeWandering()
        {
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady)
            {
                return;
            }

            agent.isStopped = false;
            isIdle = false;
            SetNewDestination();
        }
        
        // Dialogue state management
        public void OnDialogueStart()
        {
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady)
            {
                return;
            }

            Debug.Log($"[{gameObject.name}] Dialogue started");
            isInDialogue = true;
            agent.isStopped = true;
            isIdle = true;
        }
        
        public void OnDialogueEnd()
        {
            if (!gameObject.activeInHierarchy || !isInitialized || !isNavMeshReady)
            {
                return;
            }

            Debug.Log($"[{gameObject.name}] Dialogue ended");
            isInDialogue = false;
            agent.isStopped = false;
            isIdle = false;
            SetNewDestination();
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw patrol radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(patrolCenter != null ? patrolCenter.position : transform.position, patrolRadius);
            
            // Draw player detection radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
            
            // Draw minimum distance to other NPCs
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minDistanceToOtherNPCs);
        }
    }
} 