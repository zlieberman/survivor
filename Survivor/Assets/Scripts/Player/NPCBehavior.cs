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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public void Initialize(PlayerStats playerStats)
    {
        stats = playerStats;
        // Set agent speed based on NPC's speed stat
        agent.speed = 3.5f + (stats.speed / 100f) * 2f;
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
                    currentDestination = hit.position;
                    agent.SetDestination(currentDestination);
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
} 