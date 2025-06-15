using UnityEngine;
using UnityEngine.AI;

public class NPCBehavior : MonoBehaviour
{
    private PlayerStats stats;
    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 currentDestination;
    private float idleTimer;
    private float idleTime = 5f;
    private CharacterController characterController;
    private float minDistanceToOthers = 1.5f; // Minimum distance to maintain from other NPCs

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        
        // Ensure proper collision detection
        if (characterController != null)
        {
            characterController.radius = 0.5f;
            characterController.height = 2f;
            characterController.stepOffset = 0.3f;
            characterController.skinWidth = 0.08f;
        }
    }

    public void Initialize(PlayerStats playerStats)
    {
        stats = playerStats;
        // Set agent speed based on NPC's speed stat
        agent.speed = 3.5f + (stats.speed / 100f) * 2f;
        
        // Configure NavMeshAgent for better collision avoidance
        agent.radius = 0.5f;
        agent.height = 2f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Random.Range(0, 100); // Random priority to prevent all NPCs from trying to avoid each other at once
    }

    private void Update()
    {
        if (stats == null) return;

        // Simple wandering behavior
        if (agent.remainingDistance < 0.1f)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                // Find a new random destination within the camp area
                Vector3 randomDirection = Random.insideUnitSphere * 10f;
                randomDirection += transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
                {
                    // Check if the new position is far enough from other NPCs
                    if (IsPositionValid(hit.position))
                    {
                        currentDestination = hit.position;
                        agent.SetDestination(currentDestination);
                    }
                }
                idleTimer = 0f;
            }
        }

        // Update animator parameters
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    private bool IsPositionValid(Vector3 position)
    {
        // Check distance to other NPCs
        Collider[] colliders = Physics.OverlapSphere(position, minDistanceToOthers);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("NPC"))
            {
                return false;
            }
        }
        return true;
    }
} 