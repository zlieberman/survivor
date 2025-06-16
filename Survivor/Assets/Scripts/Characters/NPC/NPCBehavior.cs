using UnityEngine;
using UnityEngine.AI;
using Survivor.Characters;

namespace Survivor.Characters
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCBehavior : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float minDistanceToOtherNPCs = 2f;
        public float wanderRadius = 10f;
        public float idleTime = 3f;

        private NavMeshAgent agent;
        private UnityEngine.CharacterController characterController;
        private float idleTimer;
        private bool isIdle;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            characterController = GetComponent<UnityEngine.CharacterController>();

            // Set agent speed based on character stats
            if (TryGetComponent<Character>(out var character) && character.Stats != null)
            {
                agent.speed = character.Stats.speed;
            }
        }

        private void Update()
        {
            if (isIdle)
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0)
                {
                    isIdle = false;
                    SetNewDestination();
                }
            }
            else if (agent.remainingDistance <= agent.stoppingDistance)
            {
                isIdle = true;
                idleTimer = idleTime;
            }
        }

        private void SetNewDestination()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas);

            // Check if the new position is valid (not too close to other NPCs)
            if (IsValidPosition(hit.position))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                // Try again next frame
                isIdle = true;
                idleTimer = 0.1f;
            }
        }

        private bool IsValidPosition(Vector3 position)
        {
            // Check for other NPCs in the area
            Collider[] colliders = Physics.OverlapSphere(position, minDistanceToOtherNPCs);
            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject && collider.GetComponent<NPCBehavior>() != null)
                {
                    return false;
                }
            }
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw wander radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);

            // Draw minimum distance to other NPCs
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minDistanceToOtherNPCs);
        }
    }
} 