using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Survivor.Characters;

namespace Survivor.Core
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Character))]
    public class NPCBehavior : MonoBehaviour
    {
        private NavMeshAgent navAgent;
        private Character character;
        private Animator animator;
        private NPCStats stats;

        [Header("Behavior Settings")]
        public float wanderRadius = 10f;
        public float minWanderTime = 5f;
        public float maxWanderTime = 15f;
        public float interactionDistance = 2f;

        private Vector3 currentDestination;
        private bool isWandering = false;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            character = GetComponent<Character>();
            animator = GetComponent<Animator>();
        }

        public void Initialize(NPCStats stats)
        {
            this.stats = stats;
            navAgent.speed = 3.5f + (stats.speed / 100f) * 2f;
            StartCoroutine(WanderRoutine());
        }

        private IEnumerator WanderRoutine()
        {
            while (true)
            {
                if (!isWandering)
                {
                    yield return new WaitForSeconds(Random.Range(minWanderTime, maxWanderTime));
                    SetNewWanderDestination();
                }
                yield return null;
            }
        }

        private void SetNewWanderDestination()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                currentDestination = hit.position;
                navAgent.SetDestination(currentDestination);
                isWandering = true;
            }
        }

        private void Update()
        {
            if (isWandering && navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                isWandering = false;
            }

            // Update animator if available
            if (animator != null)
            {
                animator.SetFloat("Speed", navAgent.velocity.magnitude);
            }
        }

        public void StopWandering()
        {
            isWandering = false;
            navAgent.ResetPath();
        }

        public void SetDestination(Vector3 destination)
        {
            navAgent.SetDestination(destination);
        }
    }
} 